namespace GodotAutoOnReady.SourceGenerators;

internal readonly record struct OnReadyAttributeData
{
    internal readonly string VariableName;
    internal readonly string TypeName;
    internal readonly string Path;
    internal readonly bool AllowNull;

    internal OnReadyAttributeData(string variableName, string typeName, string path, bool allowNull)
    {
        VariableName = variableName;
        TypeName = typeName;
        Path = path;
        AllowNull = allowNull;
    }
}
