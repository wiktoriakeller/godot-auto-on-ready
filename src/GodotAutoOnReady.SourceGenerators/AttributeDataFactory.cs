using GodotAutoOnReady.SourceGenerators.Attributes;
using GodotAutoOnReady.SourceGenerators.Helpers;
using GodotAutoOnReady.SourceGenerators.Models;
using Microsoft.CodeAnalysis;

namespace GodotAutoOnReady.SourceGenerators;

internal class AttributeDataFactory
{
    internal BaseAttributeData? GetAttributeData(ISymbol? symbol)
    {
        if(symbol is null)
        {
            return null;
        }

        if(symbol is IMethodSymbol methodSymbol &&
            methodSymbol.ReturnsVoid &&
            methodSymbol.Parameters.Length == 0 &&
            methodSymbol.GetAttributes().TryGetAttribute(out var methodAttribute, Attributes.OnReadyAttributeSource.Name))
        {
            var name = methodSymbol.Name;
            return new Models.OnReadyAttributeData(name, methodAttribute!);
        }

        if(symbol is IPropertySymbol propertySymbol &&
            propertySymbol.GetAttributes().TryGetAttribute(out var propAttribute, GetNodeAttributeSource.Name, Attributes.GetResAttributeSource.Name))
        {
            var name = propertySymbol.Name;
            var type = propertySymbol.Type.Name;
            return GetAttributeDataByName(propAttribute!, name, type);
        }

        if(symbol is IFieldSymbol fieldSymbol && 
           fieldSymbol.GetAttributes().TryGetAttribute(out var fieldAttribute, GetNodeAttributeSource.Name, Attributes.GetResAttributeSource.Name))
        {
            var name = fieldSymbol.Name;
            var type = fieldSymbol.Type.Name;
            return GetAttributeDataByName(fieldAttribute!, name, type);
        }

        return null;
    }

    private BaseAttributeData? GetAttributeDataByName(AttributeData attribute, string name, string type)
    {
        if (attribute.AttributeClass?.MetadataName == GetNodeAttributeSource.Name)
        {
            return new GetNodeAttributeData(name, type, attribute!);
        }

        if (attribute.AttributeClass?.MetadataName == Attributes.GetResAttributeSource.Name)
        {
            return new Models.GetResAttributeData(name, type, attribute!);
        }

        return null;
    }
}
