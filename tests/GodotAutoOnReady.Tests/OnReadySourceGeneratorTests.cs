﻿namespace GodotAutoOnReady.Tests;

public class OnReadySourceGeneratorTests
{
    [Fact]
    public async Task GeneratesReadyCorrectly()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.SourceGenerators.Attributes;

        namespace RPGGame;

        [GenerateReadyMethod]
        public partial class Sword : Node
        {
            [OnReady("%SomeProp")]
            public DummyNode Node { get; set; } = null!;

            [OnReady("%SomeField")]
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

        [GenerateReadyMethod("Init")]
        public partial class Sword : Node
        {
            [OnReady("%SomeProp")]
            public DummyNode Node { get; set; } = null!;

            [OnReady("%SomeField")]
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

        [GenerateReadyMethod]
        public partial class Sword : Node
        {
            [OnReady("%SomeProp")]
            public DummyNode Node { get; set; } = null!;

            [OnReady("%SomeField")]
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

        [GenerateReadyMethod]
        public partial class Sword : Node
        {
            [OnReady("%SomeProp")]
            public DummyNode Node { get; set; } = null!;

            [OnReady("%SomeField")]
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

        [GenerateReadyMethod]
        public partial class Sword : Node
        {
            [OnReady("%SomeProp")]
            public DummyNode Node { get; set; } = null!;

            [OnReady("%SomeField")]
            private DummyNode Field = null!;

            public Sword()
            {

            }

            public override void _Ready()
            {
                OnReadyInit(); //Generated method for nodes initialization
            }
        }
        """;

        await VerifyHelper.Verify(source);
    }
}
