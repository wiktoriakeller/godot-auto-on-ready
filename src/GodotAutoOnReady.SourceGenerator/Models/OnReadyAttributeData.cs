using Microsoft.CodeAnalysis;

namespace GodotAutoOnReady.SourceGenerator.Models;

internal record OnReadyAttributeData : BaseAttributeData
{
    internal int Order { get; private set; } = 0;

    public OnReadyAttributeData(string name, AttributeData attribute) : base(name)
    {
        Setup(attribute);
    }

    internal override int GetOrder() => 3 + Order;

    internal override string GetSourceCode() => $"{Name}();";

    private void Setup(AttributeData attribute)
    {
        foreach (var arg in attribute.NamedArguments)
        {
            if (arg.Key.ToLower() == nameof(Order).ToLower())
            {
                Order = int.Parse(arg.Value.Value?.ToString());
                Order = Math.Max(Order, 0);
            }
        }
    }
}
