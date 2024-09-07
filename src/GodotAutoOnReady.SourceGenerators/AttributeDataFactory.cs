using GodotAutoOnReady.SourceGenerators.Attributes;
using GodotAutoOnReady.SourceGenerators.Helpers;
using GodotAutoOnReady.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GodotAutoOnReady.SourceGenerators;

internal class AttributeDataFactory
{
    internal BaseAttributeData? GetAttributeData(MemberDeclarationSyntax member)
    {
        if (member is MethodDeclarationSyntax methodSyntax &&
            methodSyntax.ParameterList.Parameters.Count == 0 &&
            methodSyntax.ReturnType is PredefinedTypeSyntax predefined && predefined.Keyword.IsKind(SyntaxKind.VoidKeyword) &&
            SourceGeneratorHelper.TryGetAttribute(methodSyntax.AttributeLists, out var methodAttribute, OnReadyAttribute.Name))
        {
            var methodName = methodSyntax.Identifier.ValueText;

            if(methodName != SourceData.ReadyMethodName && methodAttribute is not null)
            {
                return new OnReadyAttributeData(methodName, methodAttribute);
            }
        }

        string name = "";
        string type = "";
        AttributeSyntax? attribute = null;

        if (member is PropertyDeclarationSyntax propSyntax &&
            SourceGeneratorHelper.TryGetAttribute(propSyntax.AttributeLists, out var propAttribute, GetNodeAttribute.Name, GetResAttribute.Name))
        {
            name = propSyntax.Identifier.ValueText;
            type = propSyntax.Type.ToString();
            attribute = propAttribute;
        }
        else if (member is FieldDeclarationSyntax fieldSyntax &&
            SourceGeneratorHelper.TryGetAttribute(fieldSyntax.AttributeLists, out var fieldAttribute, GetNodeAttribute.Name, GetResAttribute.Name))
        {
            name = fieldSyntax.Declaration.Variables.Select(x => x.Identifier.ValueText).First();
            type = fieldSyntax.Declaration.Type.ToString();
            attribute = fieldAttribute;
        }

        var attributeName = (attribute?.Name as IdentifierNameSyntax)?.Identifier.ValueText;
        if(attributeName == GetNodeAttribute.Name)
        {
            return new GetNodeAttributeData(name, type, attribute!);
        }

        if(attributeName == GetResAttribute.Name)
        {
            return new GetResAttributeData(name, type, attribute!);
        }

        return null;
    }
}
