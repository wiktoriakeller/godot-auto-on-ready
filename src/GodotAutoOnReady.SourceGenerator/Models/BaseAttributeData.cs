using GodotAutoOnReady.SourceGenerator.Common;
using Microsoft.CodeAnalysis;

namespace GodotAutoOnReady.SourceGenerator.Models;

internal abstract record BaseAttributeData : IComparable<BaseAttributeData>
{
    internal readonly string Name;
    internal EquatableArray<Diagnostic> Diagnostics = new([]);

    internal BaseAttributeData(string name)
    {
        Name = name;
    }

    public int CompareTo(BaseAttributeData other) => GetOrder().CompareTo(other.GetOrder());

    internal abstract int GetOrder();

    internal abstract string GetSourceCode();
}
