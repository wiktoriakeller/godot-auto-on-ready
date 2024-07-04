using GodotSourceGenerators.SourceGenerators.Attributes;

namespace GodotSourceGenerators.Console;

[GenerateReadyMethod]
public partial class TestItem : BaseItem
{
    [OnReady("%path")]
    public DummyNode Node { get; set; } = null!;

    [OnReady("random path")]
    public DummyNode Node2 { get; set; } = null!;

    [OnReady("cat")]
    public DummyNode Prop { get; set; } = null!;

    [OnReady("cat field")]
    private DummyNode Field = null!;
}
