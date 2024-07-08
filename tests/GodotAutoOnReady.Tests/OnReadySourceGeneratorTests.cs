namespace GodotAutoOnReady.Tests;

public class OnReadySourceGeneratorTests
{
    [Fact]
    public async Task GeneratesReadyCorrectly()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.SourceGenerators.Attributes;

        namespace RPGGame;

        [GenerateReady]
        public partial class Sword : Node
        {
            [OnReadyGet("%SomeProp")]
            public DummyNode Node { get; set; } = null!;

            [OnReadyGet("%SomeField")]
            private DummyNode Field = null!;
        }
        """;

        await VerifyHelper.Verify(source);
    }

    [Fact]
    public async Task GeneratesReadyCorrectlyInCustomInitMethod()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.SourceGenerators.Attributes;

        namespace RPGGame;

        [GenerateReady("Init")]
        public partial class Sword : Node
        {
            [OnReadyGet("%SomeProp")]
            public DummyNode Node { get; set; } = null!;

            [OnReadyGet("%SomeField")]
            private DummyNode Field = null!;
        }
        """;

        await VerifyHelper.Verify(source);
    }

    [Fact]
    public async Task GeneratesReadyCorrectlyWithoutNamespace()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.SourceGenerators.Attributes;

        [GenerateReady]
        public partial class Sword : Node
        {
            [OnReadyGet("%SomeProp")]
            public DummyNode Node { get; set; } = null!;

            [OnReadyGet("%SomeField")]
            private DummyNode Field = null!;
        }
        """;

        await VerifyHelper.Verify(source);
    }

    [Fact]
    public async Task GeneratesReadyCorrectlyWhenReadyDeclarationAlreadyExists()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.SourceGenerators.Attributes;

        namespace RPGGame;

        [GenerateReady]
        public partial class Sword : Node
        {
            [OnReadyGet("%SomeProp")]
            public DummyNode Node { get; set; } = null!;

            [OnReadyGet("%SomeField")]
            private DummyNode Field = null!;

            public override void _Ready()
            {
                OnReadyInit(); //Generated method for nodes initialization
            }
        }
        """;

        await VerifyHelper.Verify(source);
    }

    [Fact]
    public async Task GeneratesReadyCorrectlyWhenConstructorDeclarationAlreadyExists()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.SourceGenerators.Attributes;

        namespace RPGGame;

        [GenerateReady]
        public partial class Sword : Node
        {
            [OnReadyGet("%SomeProp")]
            public DummyNode Node { get; set; } = null!;

            [OnReadyGet("%SomeField")]
            private DummyNode Field = null!;

            public Sword()
            {

            }
        }
        """;

        await VerifyHelper.Verify(source);
    }

    [Fact]
    public async Task GeneratesReadyCorrectlyWhenNullableIsDisabled()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.SourceGenerators.Attributes;
        
        namespace RPGGame;
        
        [GenerateReady]
        public partial class Sword : Node
        {
            [OnReadyGet("%SomeProp")]
            public DummyNode Node { get; set; } = null!;
        
            [OnReadyGet("%SomeField")]
            private DummyNode Field = null!;
        }
        """;

        await VerifyHelper.Verify(source, true);
    }

    [Fact]
    public async Task GeneratesReadyCorrectlyAndCopiesAdditionalNamespaces()
    {
        var source = """
        using Godot;
        using RPGGame.Components;
        using RPGGame.Controllers;
        using GodotAutoOnReady.SourceGenerators.Attributes;
        
        namespace RPGGame;
        
        [GenerateReady]
        public partial class Sword : Node
        {
            [OnReadyGet("%SomeProp")]
            public DummyNode Node { get; set; } = null!;
        
            [OnReadyGet("%SomeField")]
            private DummyNode Field = null!;
        }
        """;

        await VerifyHelper.Verify(source);
    }

    [Fact]
    public async Task GeneratesReadyCorrectlyWhenOnReadyAttributeAllowsNull()
    {
        var source = """
        using Godot;
        using RPGGame.Components;
        using RPGGame.Controllers;
        using GodotAutoOnReady.SourceGenerators.Attributes;
        
        namespace RPGGame;
        
        [GenerateReady]
        public partial class Sword : Node
        {
            [OnReadyGet("%SomeProp", true)]
            public DummyNode Node { get; set; } = null!;

            [OnReadyGet("%SomeField")]
            private DummyNode Field = null!;

            [OnReadyGet("%SomeField2", true)]
            public DummyNode Field2 = null!;
        }
        """;

        await VerifyHelper.Verify(source);
    }

    [Fact]
    public async Task GeneratesReadyCorrectlyWhenNamedArgumentsAreUsed()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.SourceGenerators.Attributes;
        
        namespace RPGGame;
        
        [GenerateReady]
        public partial class Sword : Node
        {
            [OnReadyGet(path: "%SomeProp", orNull: true)]
            public DummyNode Node { get; set; } = null!;

            [OnReadyGet(orNull: false, path: "%SomeProp2")]
            public DummyNode Node2 { get; set; } = null!;

            [OnReadyGet(path: "%SomeField")]
            private DummyNode Field = null!;
        }
        """;

        await VerifyHelper.Verify(source);
    }
}
