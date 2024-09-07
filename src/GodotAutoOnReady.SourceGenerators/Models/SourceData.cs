using GodotAutoOnReady.SourceGenerators.Common;

namespace GodotAutoOnReady.SourceGenerators.Models;

internal readonly record struct SourceData
{
    internal const string ReadyMethodName = "_Ready";
    internal const string ReadyMethodModifiers = "public override void";

    internal const string DefaultInitMethodName = "OnReadySetup";
    internal const string InitMethodMofidiers = "public void";

    internal readonly bool NullableDisable;
    internal readonly string AssemblyName;

    internal readonly string ClassNamespace;
    internal readonly string ClassName;
    internal readonly string ClassModifiers;
    internal readonly string BaseClass;

    internal readonly string MethodName;
    internal readonly string MethodModifiers;

    internal readonly EquatableArray<string> UsingDeclarations;
    internal readonly EquatableArray<BaseAttributeData> Members;

    internal SourceData(
        string className,
        string classModifiers,
        string methodName,
        string methodModifiers,
        string classNamespace,
        string baseClass,
        bool nullableDisable,
        string assemblyName,
        List<string> usingDeclarations,
        List<BaseAttributeData> members)
    {
        ClassName = className;
        ClassModifiers = classModifiers;
        MethodName = methodName;
        MethodModifiers = methodModifiers;
        ClassNamespace = classNamespace;
        BaseClass = baseClass;
        NullableDisable = nullableDisable;
        AssemblyName = assemblyName;
        UsingDeclarations = new EquatableArray<string>(usingDeclarations);
        
        members.Sort();
        Members = new EquatableArray<BaseAttributeData>();
    }

    public bool GenerateReadyMethod() => MethodName == ReadyMethodName;
}
