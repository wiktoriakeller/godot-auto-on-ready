using GodotAutoOnReady.SourceGenerator.Common;
using GodotAutoOnReady.SourceGenerator.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Text;

namespace GodotAutoOnReady.SourceGenerator.Models;

internal record GetNodeAttributeData : BaseAttributeData
{
    internal string Type { get; private set; }
    internal string Path { get; private set; } = "";
    internal bool OrNull { get; private set; } = false;
    internal string AfterPath { get; private set; } = "";
    internal string NodeType { get; private set; } = "";

    internal GetNodeAttributeData(
        string name,
        string type,
        AttributeData attribute,
        Location? location) : base(name)
    {
        Type = type;
        var diagnostics = Setup(attribute, location);
        Diagnostics = new EquatableArray<Diagnostic>(diagnostics);
    }

    internal override int GetOrder() => 1;

    internal override string GetSourceCode()
    {
        var getSyntax = OrNull ? "GetNodeOrNull" : "GetNode";
        var type = AfterPath.Length > 0 && NodeType.Length > 0 ? NodeType : Type;
        return $"{Name} = {getSyntax}<{type}>(\"{Path}\"){AfterPath};";
    }
    
    private List<Diagnostic> Setup(AttributeData attribute, Location? location)
    {
        static string Capitalize(string str) => str.Length > 1 ? char.ToUpperInvariant(str[0]) + str.Substring(1) : str.ToUpperInvariant();

        var diagnostics = new List<Diagnostic>();
        if (attribute.ConstructorArguments.Length == 1)
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
                OrNull = arg.Value.Value is not null && bool.Parse(arg.Value.Value.ToString());
            }

            if(arg.Key.ToLower() == nameof(NodeType).ToLower())
            {
                NodeType = arg.Value.Value?.ToString() ?? "";
            }
        }

        var name = Name[0] == '_' && Name.Length > 1 ? Name.Substring(1) : Name;
        Path = string.IsNullOrEmpty(Path) ? Capitalize(name) : Path;

        if (Path.Contains(":"))
        {
            var colonSplitted = Path.Split(':');
            Path = colonSplitted[0];
            var sb = new StringBuilder();

            for(int i = 1; i < colonSplitted.Length; i++)
            {
                var part = colonSplitted[i];
                sb.Append(".").Append(part);
            }

            AfterPath = sb.ToString();
        }

        if(AfterPath.Length > 0 && string.IsNullOrEmpty(NodeType) && location is not null)
        {
            diagnostics.Add(DiagnosticsHelper.CreateDiagnostic(location, DiagnosticsHelper.NoNodeType));
        }

        return diagnostics;
    }
}
