using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GodotAutoOnReady.SourceGenerators.Models;

internal abstract record BaseAttributeData : IComparable<BaseAttributeData>
{
    internal readonly string Name;
    protected abstract HashSet<string> ArgumentNames { get; }

    internal BaseAttributeData(string name)
    {
        Name = name;
    }

    public int CompareTo(BaseAttributeData other) => GetOrder().CompareTo(other.GetOrder());

    internal abstract int GetOrder();

    protected Dictionary<string, string> GetArgumentsFromAttribute(AttributeSyntax attribute)
    {
        var kvp = new Dictionary<string, string>(ArgumentNames.Count);

        for (int i = 0; i < attribute.ArgumentList?.Arguments.Count; i++)
        {
            var argument = attribute.ArgumentList.Arguments[i];
            var nameColon = argument.NameColon?.Name.Identifier.ValueText;
            var value = argument.Expression.ChildTokens().First().ValueText;
            var argumentName = argument.NameEquals?.Name.Identifier.ValueText;

            if (argumentName != null && ArgumentNames.Contains(argumentName))
            {
                kvp[argumentName] = value;
            }

            if (nameColon != null && ArgumentNames.Contains(nameColon))
            {
                kvp[nameColon] = value;
            }
        }

        return kvp;
    }
}
