using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Csv.SourceGenerator;

internal sealed class MemberMetadata
{
    public ISymbol Symbol { get; }

    public string FullTypeName { get; }
    public ITypeSymbol MemberType { get; }
    public bool IsField { get; }
    public bool IsProperty { get; }
    public bool IsSettable { get; }

    public bool IsConstructorParameter { get; set; }
    public bool HasExplicitDefaultValueFromConstructor { get; set; }
    public object? ExplicitDefaultValueFromConstructor { get; set; }

    public string Key { get; }
    public int Index { get; }
    public bool IsStringKey { get; }

    public byte[] Utf8Key => utf8Key ??= System.Text.Encoding.UTF8.GetBytes(Key);
    byte[]? utf8Key;

    public MemberMetadata(ISymbol symbol, ReferenceSymbols references, int sequentialOrder)
    {
        Symbol = symbol;
        Key = symbol.Name;
        Index = sequentialOrder;

        var columnAttribute = symbol.GetAttribute(references.ColumnAttribute);
        if (columnAttribute is { ConstructorArguments.Length: > 0 })
        {
            var value = columnAttribute.ConstructorArguments[0].Value;
            if (value is int i)
            {
                Index = i;
            }
            else if (value is string s)
            {
                Key = s;
                IsStringKey = true;
            }
        }

        if (symbol is IFieldSymbol f)
        {
            IsProperty = false;
            IsField = true;
            IsSettable = !f.IsReadOnly; // readonly field can not set.
            MemberType = f.Type;
        }
        else if (symbol is IPropertySymbol p)
        {
            IsProperty = true;
            IsField = false;
            IsSettable = !p.IsReadOnly;
            MemberType = p.Type;
        }
        else
        {
            throw new Exception("member is not field or property.");
        }

        FullTypeName = MemberType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    public Location GetLocation(TypeDeclarationSyntax fallback)
    {
        var location = Symbol.Locations.FirstOrDefault() ?? fallback.Identifier.GetLocation();
        return location;
    }

    public string EmitDefaultValue()
    {
        if (!HasExplicitDefaultValueFromConstructor)
        {
            return $"default({FullTypeName})";
        }

        return ExplicitDefaultValueFromConstructor switch
        {
            null => $"default({FullTypeName})",
            string x => $"\"{x}\"",
            float x => $"{x}f",
            double x => $"{x}d",
            decimal x => $"{x}m",
            bool x => x ? "true" : "false",
            _ => ExplicitDefaultValueFromConstructor.ToString()
        };
    }
}