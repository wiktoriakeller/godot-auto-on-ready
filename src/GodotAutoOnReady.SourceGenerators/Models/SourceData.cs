using GodotAutoOnReady.SourceGenerators.Common;

namespace GodotAutoOnReady.SourceGenerators.Models;

internal readonly record struct SourceData
{
    internal const string ReadyMethodName = "_Ready";
    internal const string ReadyMethodModifiers = "public override void";

    internal const string DefaultInitMethodName = "OnReadyInit";
    internal const string InitMethodMofidiers = "public void";

    internal readonly string ClassName;
    internal readonly string ClassModifiers;
    internal readonly string MethodName;
    internal readonly string MethodModifiers;
    internal readonly string ClassNamespace;
    internal readonly string BaseClass;
    internal readonly bool NullableDisable;
    internal readonly string AssemblyName;
    internal readonly EquatableArray<string> OnReadyMethods;
    internal readonly EquatableArray<string> UsingDeclarations;
    internal readonly EquatableArray<OnReadyGetAttributeData> Props;

    internal SourceData(
        string className,
        string classModifiers,
        string methodName,
        string methodModifiers,
        string classNamespace,
        string baseClass,
        bool nullableDisable,
        string assemblyName,
        EquatableArray<string> usingDeclarations,
        EquatableArray<string> onReadyMethods,
        EquatableArray<OnReadyGetAttributeData> props)
    {
        ClassName = className;
        ClassModifiers = classModifiers;
        MethodName = methodName;
        MethodModifiers = methodModifiers;
        ClassNamespace = classNamespace;
        BaseClass = baseClass;
        Props = props;
        NullableDisable = nullableDisable;
        AssemblyName = assemblyName;
        UsingDeclarations = usingDeclarations;
        OnReadyMethods = onReadyMethods;
    }

    public bool CanGenerateReadyMethod() => MethodName == ReadyMethodName;
}
