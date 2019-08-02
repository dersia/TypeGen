# TypeGen

Single-class-per-file C# to TypeScript generator

[![Build status](https://ci.appveyor.com/api/projects/status/pwi1gh8o1byigo2x?svg=true)](https://ci.appveyor.com/project/JacekBurzynski/typegen)

***Project's website: http://jburzynski.net/TypeGen***

***Complete documentation: http://typegen.readthedocs.io***

## How to get

* [NuGet](https://www.nuget.org/packages/TypeGen)
* [NuGet - .NET CLI tool](https://www.nuget.org/packages/TypeGen.DotNetCli)

## Quick start

1. Add [TypeGen NuGet package](https://www.nuget.org/packages/TypeGen) to your project.

2. Mark your C# classes/enums as exportable to TypeScript:

```c#
// with attributes

[ExportTsClass]
public class ProductDto
{
    public decimal Price { get; set; }
    public string[] Tags { get; set; }
}

// or with a generation spec (created anywhere in your project)

public class MyGenerationSpec : GenerationSpec
{
    public MyGenerationSpec()
    {
        AddClass<ProductDto>();
    }
}
```

3. If you're using a generation spec, create a file named `tgconfig.json` directly in your project folder and place the following content in this file:

```json
{
  "generationSpecs": ["MyGenerationSpec"]
}
```

4. Build your project and type `TypeGen generate` or `TypeGen -p "MyProjectName" generate` (depending on the current working directory of the PM Console) into the Package Manager Console (you might need to restart Visual Studio), or `dotnet typegen generate` in the system console if you're using TypeGen .NET CLI tool.

This will generate a single TypeScript file (named *product-dto.ts*) in your project directory. The file will look like this:

```typescript
export class ProductDto {
    price: number;
    tags: string[];
}
```

## Features

Some of TypeGen's features include:

* generating TypeScript classes, interfaces and enums - single class per file
* generating barrel (index) files
* support for collection (or nested collection) types
* generic classes/types generation
* support for inheritance
* customizable convertion between C#/TypeScript names (naming conventions)

For a complete list of features with examples, please refer to the project's documentation: http://typegen.readthedocs.io