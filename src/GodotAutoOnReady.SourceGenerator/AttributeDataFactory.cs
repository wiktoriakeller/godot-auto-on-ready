﻿using GodotAutoOnReady.SourceGenerator.Attributes;
using GodotAutoOnReady.SourceGenerator.Helpers;
using GodotAutoOnReady.SourceGenerator.Models;
using Microsoft.CodeAnalysis;

namespace GodotAutoOnReady.SourceGenerator;

internal class AttributeDataFactory
{
    internal BaseAttributeData? GetAttributeData(ISymbol? symbol)
    {
        if(symbol is null)
        {
            return null;
        }

        var location = symbol.Locations.FirstOrDefault();
        if(symbol is IMethodSymbol methodSymbol &&
            methodSymbol.ReturnsVoid &&
            methodSymbol.Parameters.Length == 0 &&
            methodSymbol.GetAttributes().TryGetAttribute(out var methodAttribute, Attributes.OnReadyAttributeSource.Name))
        {
            var name = methodSymbol.Name;
            return new OnReadyAttributeData(name, methodAttribute!);
        }

        if(symbol is IPropertySymbol propertySymbol &&
            propertySymbol.GetAttributes().TryGetAttribute(out var propAttribute, GetNodeAttributeSource.Name, Attributes.GetResAttributeSource.Name))
        {
            var name = propertySymbol.Name;
            var type = propertySymbol.Type.Name;
            return GetAttributeDataByName(propAttribute!, name, type, location);
        }

        if(symbol is IFieldSymbol fieldSymbol && 
           fieldSymbol.GetAttributes().TryGetAttribute(out var fieldAttribute, GetNodeAttributeSource.Name, Attributes.GetResAttributeSource.Name))
        {
            var name = fieldSymbol.Name;
            var type = fieldSymbol.Type.Name;
            return GetAttributeDataByName(fieldAttribute!, name, type, location);
        }

        return null;
    }

    private BaseAttributeData? GetAttributeDataByName(AttributeData attribute, string name, string type, Location? location)
    {
        if (attribute.AttributeClass?.MetadataName == GetNodeAttributeSource.Name)
        {
            return new GetNodeAttributeData(name, type, attribute!, location);
        }

        if (attribute.AttributeClass?.MetadataName == GetResAttributeSource.Name)
        {
            return new GetResAttributeData(name, type, attribute!);
        }

        return null;
    }
}
