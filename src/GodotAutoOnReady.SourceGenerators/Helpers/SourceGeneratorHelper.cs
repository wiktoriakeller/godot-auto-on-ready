using GodotAutoOnReady.SourceGenerators.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GodotAutoOnReady.SourceGenerators.Helpers;

internal static class SourceGeneratorHelper
{
    internal static bool TryGetAttribute(in SyntaxList<AttributeListSyntax> attributeList, string attributeName, out AttributeSyntax? attribute)
    {
        attribute = null;

        foreach (var attributeListSyntax in attributeList)
        {
            attribute = attributeListSyntax.Attributes.FirstOrDefault(x => x.Name is IdentifierNameSyntax identifier && identifier.Identifier.Text == attributeName);
            if (attribute is not null)
            {
                return true;
            }
        }

        return false;
    }

    internal static EquatableArray<string> GetUsingDeclarations(in SyntaxNode root)
    {
        var usingDeclarations = new List<string>();

        foreach (var rootChild in root.ChildNodes())
        {
            if (rootChild is UsingDirectiveSyntax usingSyntax)
            {
                usingDeclarations.Add($"using {usingSyntax.Name!.GetText()};");
            }
        }

        return new EquatableArray<string>(usingDeclarations);
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
