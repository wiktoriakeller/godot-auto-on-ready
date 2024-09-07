using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace GodotAutoOnReady.SourceGenerators.Models;

internal record GetResAttributeData : BaseAttributeData
{
    internal string Type { get; private set; }
    internal string Path { get; private set; } = "";

    protected override HashSet<string> ArgumentNames
    {
        get => ["path"];
    }

    internal GetResAttributeData(
        string name, 
        string type, 
        AttributeSyntax attribute) : base(name)
    {
        Type = type;
        Setup(attribute);
    }

    internal override int GetOrder() => 2;

    private void Setup(AttributeSyntax attribute)
    {
        var arguments = GetArgumentsFromAttribute(attribute);

        foreach (var kvp in arguments)
        {
            if (kvp.Key == "path")
            {
                Path = kvp.Value;
            }
        }
    }
}