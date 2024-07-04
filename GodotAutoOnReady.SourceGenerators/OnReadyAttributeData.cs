namespace GodotSourceGenerators.SourceGenerators;

internal readonly record struct OnReadyAttributeData
{
    internal readonly string VariableName;
    internal readonly string TypeName;
    internal readonly string Path;

    internal OnReadyAttributeData(string variableName, string typeName, string path)
    {
        VariableName = variableName;
        TypeName = typeName;
        Path = path;
    }
}
