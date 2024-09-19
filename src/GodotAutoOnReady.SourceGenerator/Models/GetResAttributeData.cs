using Microsoft.CodeAnalysis;

namespace GodotAutoOnReady.SourceGenerator.Models;

internal record GetResAttributeData : BaseAttributeData
{
    internal string Type { get; private set; }
    internal string Path { get; private set; } = "";

    internal GetResAttributeData(
        string name, 
        string type,
        AttributeData attribute) : base(name)
    {
        Type = type;
        Setup(attribute);
    }

    internal override int GetOrder() => 2;

    internal override string GetSourceCode() => $"{Name} = GD.Load<{Type}>(\"{Path}\");";

    private void Setup(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length == 1)
        {
            Path = attribute.ConstructorArguments[0].Value?.ToString() ?? string.Empty;
        }
    }
}
