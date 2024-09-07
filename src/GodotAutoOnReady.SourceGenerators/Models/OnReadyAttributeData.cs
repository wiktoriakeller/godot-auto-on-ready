using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GodotAutoOnReady.SourceGenerators.Models;

internal record OnReadyAttributeData : BaseAttributeData
{
    internal int Order { get; private set; } = 0;

    protected override HashSet<string> ArgumentNames
    {
        get => [nameof(Order)];
    }

    public OnReadyAttributeData(string name, AttributeSyntax attribute) : base(name)
    {
        Setup(attribute);
    }

    internal override int GetOrder() => 3 + Order;

    private void Setup(AttributeSyntax attribute)
    {
        var arguments = GetArgumentsFromAttribute(attribute);

        foreach (var kvp in arguments)
        {
            if (kvp.Key == nameof(Order))
            {
                Order = Math.Max(int.Parse(kvp.Value), 0);
            }
        }
    }
}