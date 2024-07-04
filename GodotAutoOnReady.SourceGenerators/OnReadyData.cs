using GodotSourceGenerators.SourceGenerators.Common;

namespace GodotSourceGenerators.SourceGenerators;

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
    internal readonly EquatableArray<OnReadyAttributeData> Attributes;

    internal OnReadyData(
        string className,
        string classModifiers,
        string methodName,
        string methodModifiers,
        string classNamespace,
        string baseClass,
        bool hasConstructor,
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
    }

    internal bool CanGenerateReadyMethod() => MethodName == ReadyMethodName && !HasConstructor;
}
