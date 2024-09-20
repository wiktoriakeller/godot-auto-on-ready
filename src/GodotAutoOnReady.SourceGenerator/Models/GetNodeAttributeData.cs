using Microsoft.CodeAnalysis;

namespace GodotAutoOnReady.SourceGenerator.Models;

internal record GetNodeAttributeData : BaseAttributeData
{
    internal string Type { get; private set; }
    internal string Path { get; private set; } = "";
    internal bool OrNull { get; private set; } = false;

    internal GetNodeAttributeData(
        string name,
        string type,
        AttributeData attribute) : base(name)
    {
        Type = type;
        Setup(attribute);
    }

    internal override int GetOrder() => 1;

    internal override string GetSourceCode()
    {
        var getSyntax = OrNull ? "GetNodeOrNull" : "GetNode";
        return $"{Name} = {getSyntax}<{Type}>(\"{Path}\");";
    }
    
    private void Setup(AttributeData attribute)
    {
        if(attribute.ConstructorArguments.Length == 1)
        {
            Path = attribute.ConstructorArguments[0].Value?.ToString() ?? string.Empty;    
        }

        foreach(var arg in attribute.NamedArguments)
        {
            if(arg.Key.ToLower() == nameof(Path).ToLower())
            {
                Path = arg.Value.ToString();
            }

            if(arg.Key.ToLower() == nameof(OrNull).ToLower())
            {
                OrNull = bool.Parse(arg.Value.Value?.ToString());
            }
        }

        var name = Name[0] == '_' && Name.Length > 1 ? Name.Substring(1) : Name;
        var upperCasedName = name.Length > 1 ? char.ToUpper(name[0]) + name.Substring(1) : name.ToUpper();
        Path = string.IsNullOrEmpty(Path) ? upperCasedName : Path;
    }
}
