using GodotAutoOnReady.SourceGenerators.Common;

namespace GodotAutoOnReady.SourceGenerators;

internal readonly record struct OnReadyData
{
    internal const string DefaultInitMethodName = "OnReadyInit";
    internal const string ReadyMethodName = "_Ready";

    internal readonly string ClassName;
    internal readonly string ClassModifiers;
    internal readonly string MethodName;
    internal readonly string MethodModifiers;
    internal readonly string ClassNamespace;
    internal readonly string BaseClass;
    internal readonly bool HasConstructor;
    internal readonly bool NullableDisable;
    internal readonly string AssemblyName;
    internal readonly EquatableArray<string> UsingDeclarations;
    internal readonly EquatableArray<OnReadyAttributeData> Attributes;

    internal OnReadyData(
        string className,
        string classModifiers,
        string methodName,
        string methodModifiers,
        string classNamespace,
        string baseClass,
        bool hasConstructor,
        bool nullableDisable,
        string moduleName,
        EquatableArray<string> usingDeclarations,
        EquatableArray<OnReadyAttributeData> attributes)
    {
        ClassName = className;
        ClassModifiers = classModifiers;
        MethodName = methodName;
        MethodModifiers = methodModifiers;
        ClassNamespace = classNamespace;
        BaseClass = baseClass;
        HasConstructor = hasConstructor;
        Attributes = attributes;
        NullableDisable = nullableDisable;
        AssemblyName = moduleName;
        UsingDeclarations = usingDeclarations;
    }

    internal bool CanGenerateReadyMethod() => MethodName == ReadyMethodName && !HasConstructor;
}
