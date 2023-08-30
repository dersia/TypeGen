using System;
using System.Collections.Generic;

namespace TypeGen.Core.TypeModel.Csharp;

internal class CsGenericParameter : CsType
{
    public CsGenericParameter(string name, bool nullable) : base(name, nullable)
    {
    }
}