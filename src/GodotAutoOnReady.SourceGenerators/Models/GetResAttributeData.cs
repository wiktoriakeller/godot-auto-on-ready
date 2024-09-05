namespace GodotAutoOnReady.SourceGenerators.Models;

internal readonly record struct GetResAttributeData
{
    internal readonly string VariableName;
    internal readonly string TypeName;
    internal readonly string Path;

    internal GetResAttributeData(string variableName, string typeName, string path)
    {
        VariableName = variableName;
        TypeName = typeName;
        Path = path;
    }
}