namespace GodotAutoOnReady.SourceGenerators.Models;

internal readonly record struct OnReadyGetAttributeData
{
    internal readonly string VariableName;
    internal readonly string TypeName;
    internal readonly string Path;
    internal readonly bool OrNull;

    internal OnReadyGetAttributeData(string variableName, string typeName, string path, bool orNull)
    {
        VariableName = variableName;
        TypeName = typeName;
        Path = path;
        OrNull = orNull;
    }
}
