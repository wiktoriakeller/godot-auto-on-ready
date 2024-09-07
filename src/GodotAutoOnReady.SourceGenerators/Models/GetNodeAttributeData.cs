using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GodotAutoOnReady.SourceGenerators.Models;

internal record GetNodeAttributeData : BaseAttributeData
{
    internal string Type { get; private set; }
    internal string Path { get; private set; } = "";
    internal bool OrNull { get; private set; } = false;

    protected override HashSet<string> ArgumentNames
    {
        get => [nameof(Path), nameof(OrNull)];
    }

    internal GetNodeAttributeData(
        string name,
        string type,
        AttributeSyntax attribute) : base(name)
    {
        Type = type;
        Setup(attribute);
    }

    internal override int GetOrder() => 1;
    
    private void Setup(AttributeSyntax attribute)
    {
        var arguments = GetArgumentsFromAttribute(attribute);
        var path = "";

        foreach(var kvp in arguments)
        {
            if(kvp.Key == nameof(Path))
            {
                path = kvp.Value;
            }

            if(kvp.Key == nameof(OrNull))
            {
                OrNull = bool.Parse(kvp.Value);
            }
        }

        if (string.IsNullOrEmpty(path))
        {
            if (path[0] == '_')
            {
                path = path.Length == 1 ? string.Empty : path.Substring(1);
            }

            path = char.ToUpper(path[0]) + path.Length > 1 ? path.Substring(1) : string.Empty;
        }

        Path = path;
    }
}
