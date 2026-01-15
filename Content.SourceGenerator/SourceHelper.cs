using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Content.SourceGenerator;

public static class SourceHelper
{
    public static bool HasAttribute(ISymbol type, string attributeName)
    {
        foreach (var attribute in type.GetAttributes())
            if (attribute.AttributeClass?.ToDisplayString() == attributeName)
                return true;

        return false;
    }

    public static (ClassDeclarationSyntax, bool reportAttributeFound) GetClassDeclarationForSourceGen(
        GeneratorSyntaxContext context, string requiredAttributeName)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

        foreach (var attributeListSyntax in classDeclarationSyntax.AttributeLists)
        foreach (var attributeSyntax in attributeListSyntax.Attributes)
        {
            if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                continue;

            var attributeName = attributeSymbol.ContainingType.ToDisplayString();

            if (attributeName == requiredAttributeName)
                return (classDeclarationSyntax, true);
        }

        return (classDeclarationSyntax, false);
    }
}