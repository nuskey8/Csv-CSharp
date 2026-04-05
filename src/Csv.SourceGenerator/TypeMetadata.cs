using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Csv.SourceGenerator;

internal record class TypeMetadata
{
    public TypeDeclarationSyntax Syntax { get; }
    public INamedTypeSymbol Symbol { get; }
    public string TypeName { get; }
    public string FullTypeName { get; }
    public bool KeyAsPropertyName { get; }
    public IReadOnlyList<IMethodSymbol> Constructors { get; }

    public IReadOnlyList<MemberMetadata> Members => members ??= GetSerializeMembers();

    readonly ReferenceSymbols references;
    MemberMetadata[]? members;

    public TypeMetadata(
        TypeDeclarationSyntax syntax,
        INamedTypeSymbol symbol,
        ReferenceSymbols references)
    {
        Syntax = syntax;
        Symbol = symbol;
        this.references = references;

        TypeName = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        FullTypeName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        KeyAsPropertyName = (bool)symbol.GetAttributes()
            .FindAttributeShortName("CsvObjectAttribute").ConstructorArguments[0].Value!;

        Constructors = symbol.InstanceConstructors
            .Where(x => !x.IsImplicitlyDeclared) // remove empty ctor(struct always generate it), record's clone ctor
            .ToArray();
    }

    public bool IsPartial()
    {
        return Syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }

    public bool IsNested()
    {
        return Syntax.Parent is TypeDeclarationSyntax;
    }

    MemberMetadata[] GetSerializeMembers()
    {
        members ??= Symbol.GetAllMembers() // iterate includes parent type
            .Where(x => x is (IFieldSymbol or IPropertySymbol) and { IsStatic: false, IsImplicitlyDeclared: false })
            .Where(x =>
            {
                if (KeyAsPropertyName && x.DeclaredAccessibility != Accessibility.Public) return false;
                if (!KeyAsPropertyName && !x.ContainsAttribute(references.ColumnAttribute)) return false;
                if (x.ContainsAttribute(references.IgnoreMemberAttribute)) return false;

                if (x is IPropertySymbol p)
                {
                    if (p.GetMethod == null || p.SetMethod == null) return false;
                    if (p.IsIndexer) return false;
                }
                return true;
            })
            .Select((x, i) => new MemberMetadata(x, references, i))
            .OrderBy(x => x.Index)
            .ToArray();

        return members;
    }
}