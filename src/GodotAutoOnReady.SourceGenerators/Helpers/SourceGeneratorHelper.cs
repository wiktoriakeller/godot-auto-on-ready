using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace GodotAutoOnReady.SourceGenerators.Helpers;

internal static class SourceGeneratorHelper
{
    internal static bool TryGetAttribute(
        this ImmutableArray<AttributeData> attributeList, out AttributeData? attribute, params string[] attributeNames)
    {
        attribute = null;

        foreach (var attributeData in attributeList)
        {
            if (attributeNames.Contains(attributeData.AttributeClass?.MetadataName))
            {
                attribute = attributeData;
                return true;
            }
        }

        return false;
    }

    internal static List<string> GetUsingDeclarations(in SyntaxNode root)
    {
        var usingDeclarations = new List<string>();

        foreach (var rootChild in root.ChildNodes())
        {
            if (rootChild is UsingDirectiveSyntax usingSyntax)
            {
                usingDeclarations.Add($"using {usingSyntax.Name!.GetText()};");
            }
        }

        return usingDeclarations;
    }

    internal static string GetNamespace(in BaseTypeDeclarationSyntax syntax)
    {
        //Default namespace case
        string nameSpace = string.Empty;

        //Get the containing syntax node for the type declaration (could be a nested type)
        SyntaxNode? potentialNamespaceParent = syntax.Parent;

        //Keep moving out of nested classes
        while (potentialNamespaceParent != null &&
                potentialNamespaceParent is not NamespaceDeclarationSyntax
                && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        //Build up the final namespace by looping until we no longer have a namespace declaration
        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            nameSpace = namespaceParent.Name.ToString();

            //Keep moving out of the namespace declarations until we run out of nested namespace declarations
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }

        return nameSpace;
    }
}
