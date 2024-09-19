namespace GodotAutoOnReady.SourceGenerator.Models;

internal abstract record BaseAttributeData : IComparable<BaseAttributeData>
{
    internal readonly string Name;

    internal BaseAttributeData(string name)
    {
        Name = name;
    }

    public int CompareTo(BaseAttributeData other) => GetOrder().CompareTo(other.GetOrder());

    internal abstract int GetOrder();

    internal abstract string GetSourceCode();
}
