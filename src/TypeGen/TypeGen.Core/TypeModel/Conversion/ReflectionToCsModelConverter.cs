#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TypeGen.Core.Extensions;
using TypeGen.Core.TypeModel.Csharp;
using TypeGen.Core.Validation;

namespace TypeGen.Core.TypeModel.Conversion;

internal class ReflectionToCsModelConverter
{
    public static CsType ConvertType(MemberInfo memberInfo)
    {
        Requires.NotNull(memberInfo, nameof(memberInfo));

        var nullability = memberInfo.IsNullable();
        Requires.NotNull(nullability, nameof(nullability));

        return ConvertTypePrivate(nullability!);
    }

    private static CsType ConvertTypePrivate(NullabilityInfo nullabilityInfo)
        => nullabilityInfo.Type switch
        {
            { IsPrimitive: true } => ConvertPrimitive(nullabilityInfo.Type, nullabilityInfo.ReadState == NullabilityState.Nullable),
            { IsEnum: true } => ConvertEnum(nullabilityInfo.Type, nullabilityInfo.ReadState == NullabilityState.Nullable),
            { IsClass: true } => ConvertClass(nullabilityInfo),
            { IsInterface: true } => ConvertInterface(nullabilityInfo),
            not null when nullabilityInfo.Type.Name.StartsWith("Nullable") => ConvertPrimitive(nullabilityInfo.Type.GenericTypeArguments[0], nullabilityInfo.ReadState == NullabilityState.Nullable),
            not null when nullabilityInfo.Type.IsStruct() =>  ConvertStruct(nullabilityInfo),
            _ => throw new ArgumentException($"Type '{nullabilityInfo.Type!.FullName}' is not supported. Only classes, interfaces, structs, enums and generic parameters can be translated to TypeScript.")
        };

    private static CsGenericParameter ConvertGenericParameter(NullabilityInfo nullabilityInfo)
    {
        var typeName = nullabilityInfo.Type.Name;
        if (typeName.StartsWith("Nullable") && nullabilityInfo.Type.GenericTypeArguments?.Length > 0)
        {
            typeName = nullabilityInfo.Type.GenericTypeArguments[0].Name;
        }
        return new CsGenericParameter(typeName, nullabilityInfo.ReadState == NullabilityState.Nullable);
    }
    
    private static CsPrimitive ConvertPrimitive(Type type, bool isNullable)
    {
        return new CsPrimitive(type.FullName, type.Name, isNullable);
    }
    
    private static CsEnum ConvertEnum(Type type, bool isNullable)
    {
        var values = GetEnumValues(type);
        var tgAttributes = GetTgAttributes(type);

        return new CsEnum(type.FullName, type.Name, tgAttributes, values, isNullable);
    }

    private static CsGpType ConvertClass(NullabilityInfo nullabilityInfo)
    {
        var genericTypes = GetGenericTypes(nullabilityInfo);
        var @base = GetBase(nullabilityInfo.Type);
        var implementedInterfaces = GetImplementedInterfaces(nullabilityInfo.Type);
        var tgAttributes = GetTgAttributes(nullabilityInfo.Type);
        var fields = GetFields(nullabilityInfo.Type);
        var properties = GetProperties(nullabilityInfo.Type);

        return CsGpType.Class(nullabilityInfo.Type.FullName!,
            nullabilityInfo.Type.Name,
            nullabilityInfo.ReadState == NullabilityState.Nullable,
            genericTypes,
            @base,
            implementedInterfaces,
            fields,
            properties,
            tgAttributes);
    }

    private static CsGpType ConvertInterface(NullabilityInfo nullabilityInfo)
    {
        var genericTypes = GetGenericTypes(nullabilityInfo);
        var @base = GetBase(nullabilityInfo.Type);
        var tgAttributes = GetTgAttributes(nullabilityInfo.Type);
        var properties = GetProperties(nullabilityInfo.Type);

        return CsGpType.Interface(nullabilityInfo.Type.FullName!,
            nullabilityInfo.Type.Name,
            nullabilityInfo.ReadState == NullabilityState.Nullable,
            genericTypes,
            @base,
            properties,
            tgAttributes);
    }
    
    private static CsGpType ConvertStruct(NullabilityInfo nullabilityInfo)
    {
        var genericTypes = GetGenericTypes(nullabilityInfo);
        var tgAttributes = GetTgAttributes(nullabilityInfo.Type);
        var fields = GetFields(nullabilityInfo.Type);
        var properties = GetProperties(nullabilityInfo.Type);

        return CsGpType.Struct(nullabilityInfo.Type.FullName!,
            nullabilityInfo.Type.Name,
            nullabilityInfo.ReadState == NullabilityState.Nullable,
            genericTypes,
            fields,
            properties,
            tgAttributes);
    }
    
    private static IReadOnlyCollection<CsEnumValue> GetEnumValues(Type type)
    {
        return type.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(fieldInfo =>
            {
                var enumValue = fieldInfo.GetValue(null);
                var underlyingValue = Convert.ChangeType(enumValue, Enum.GetUnderlyingType(type));
                return new CsEnumValue(fieldInfo.Name, underlyingValue);
            })
            .ToList();
    }

    private static IReadOnlyCollection<CsType> GetGenericTypes(NullabilityInfo nullabilityInfo)
    {
        var genericTypes = nullabilityInfo.GetGenericArguments();
        return genericTypes.Select(ConvertGenericParameter).ToList();
    }

    private static CsGpType? GetBase(Type type)
        => type.BaseType != null ? (CsGpType)ConvertType(type.BaseType) : null;
    
    private static IReadOnlyCollection<CsGpType> GetImplementedInterfaces(Type type)
        => type.GetInterfaces()
            .Select(ConvertType)
            .Cast<CsGpType>()
            .ToList();
    
    private static IReadOnlyCollection<Attribute> GetTgAttributes(Type type)
        => type.GetCustomAttributes().GetTypeGenAttributes().ToList();
    
    private static IReadOnlyCollection<CsProperty> GetProperties(Type type)
        => type.GetProperties()
            .Select(propertyInfo =>
            {
                var propertyType = ConvertType(propertyInfo);
                var name = propertyInfo.Name;
                var defaultValue = propertyInfo.GetValue(null);
                return new CsProperty(propertyType, name, defaultValue);
            })
            .ToList();
    
    private static IReadOnlyCollection<CsField> GetFields(Type type)
    {
        return type.GetFields()
            .Select(fieldInfo =>
            {
                var fieldType = ConvertType(fieldInfo);
                var name = fieldInfo.Name;
                var defaultValue = fieldInfo.GetValue(null);
                return new CsField(fieldType, name, defaultValue);
            })
            .ToList();
    }
}