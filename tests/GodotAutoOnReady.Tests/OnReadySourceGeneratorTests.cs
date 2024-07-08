namespace GodotAutoOnReady.Tests;

public class OnReadySourceGeneratorTests
{
    [Fact]
    public async Task GivenOnReadyGetMembers_GeneratesReadyMethodThatSetsTheMarkedMembers()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.Attributes;

        namespace RPGGame;

        [GenerateOnReady]
        public partial class Sword : Node
        {
            [OnReadyGet("%SomeProp")]
            public DummyNode Node { get; set; } = null!;

            [OnReadyGet("%SomeField")]
            private DummyNode Field = null!;
        }
        """;

        await VerifyHelper.Verify(source, nameof(GivenOnReadyGetMembers_GeneratesReadyMethodThatSetsTheMarkedMembers));
    }

    [Fact]
    public async Task GivenMembersWithAndWithoutOnReadyGet_GeneratesReadyMethodThatSetsOnlyTheMarkedMembers()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.Attributes;

        namespace RPGGame;

        [GenerateOnReady]
        public partial class Sword : Node
        {
            [OnReadyGet("%SomeProp")]
            public DummyNode Node { get; set; } = null!;

            [OnReadyGet("%SomeField")]
            private DummyNode Field = null!;

            public DummyNode Node2 { get; set; } = null!;
        
            private DummyNode Field2 = null!;
        }
        """;

        await VerifyHelper.Verify(source, nameof(GivenMembersWithAndWithoutOnReadyGet_GeneratesReadyMethodThatSetsOnlyTheMarkedMembers));
    }

    [Fact]
    public async Task GivenCustomInitMethodName_GeneratesMethodNamedInitThatSetsTheMarkedMembers()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.Attributes;

        namespace RPGGame;

        [GenerateOnReady("Init")]
        public partial class Sword : Node
        {
            [OnReadyGet("%SomeProp")]
            public DummyNode Node { get; set; } = null!;

            [OnReadyGet("%SomeField")]
            private DummyNode Field = null!;

            public DummyNode Node2 { get; set; } = null!;
        
            private DummyNode Field2 = null!;
        }
        """;

        await VerifyHelper.Verify(source, nameof(GivenCustomInitMethodName_GeneratesMethodNamedInitThatSetsTheMarkedMembers));
    }

    [Fact]
    public async Task WhenNamespaceIsNotSpecified_GeneratesClassWithReadyMethodWithoutNamespace()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.Attributes;

        [GenerateOnReady]
        public partial class Sword : Node
        {
            [OnReadyGet("%SomeProp")]
            public DummyNode Node { get; set; } = null!;

            [OnReadyGet("%SomeField")]
            private DummyNode Field = null!;
        }
        """;

        await VerifyHelper.Verify(source, nameof(WhenNamespaceIsNotSpecified_GeneratesClassWithReadyMethodWithoutNamespace));
    }

    [Fact]
    public async Task WhenReadyDeclarationAlreadyExists_GeneratesInitMethodWithDefaultName()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.Attributes;

        namespace RPGGame;

        [GenerateOnReady]
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

        await VerifyHelper.Verify(source, nameof(WhenReadyDeclarationAlreadyExists_GeneratesInitMethodWithDefaultName));
    }

    [Fact]
    public async Task WhenDefaultConstructorExists_GeneratesInitMethodWithDefaultName()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.Attributes;

        namespace RPGGame;

        [GenerateOnReady]
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

        await VerifyHelper.Verify(source, nameof(WhenDefaultConstructorExists_GeneratesInitMethodWithDefaultName));
    }

    [Fact]
    public async Task WhenNullableIsDisabled_GeneratesFileWithoutNullableDisableInstruction()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.Attributes;
        
        namespace RPGGame;
        
        [GenerateOnReady]
        public partial class Sword : Node
        {
            [OnReadyGet("%SomeProp")]
            public DummyNode Node { get; set; } = null!;
        
            [OnReadyGet("%SomeField")]
            private DummyNode Field = null!;
        }
        """;

        await VerifyHelper.Verify(source, nameof(WhenNullableIsDisabled_GeneratesFileWithoutNullableDisableInstruction), true);
    }

    [Fact]
    public async Task WhenAdditionalUsingsAreDeclared_GeneratesFileWithCopiedUsings()
    {
        var source = """
        using Godot;
        using RPGGame.Components;
        using RPGGame.Controllers;
        using GodotAutoOnReady.Attributes;
        
        namespace RPGGame;
        
        [GenerateOnReady]
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
    public async Task WhenOnReadyGetMembersThatAllowNull_GeneratesReadyMethodThatSetsMembersWithGetOrNull()
    {
        var source = """
        using Godot;
        using RPGGame.Components;
        using RPGGame.Controllers;
        using GodotAutoOnReady.Attributes;
        
        namespace RPGGame;
        
        [GenerateOnReady]
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

        await VerifyHelper.Verify(source, nameof(WhenOnReadyGetMembersThatAllowNull_GeneratesReadyMethodThatSetsMembersWithGetOrNull));
    }

    [Fact]
    public async Task WhenOnReadyGetMembersHaveNamedParameters_GeneratesReadyMethodThatSetsMarkedMembers()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.Attributes;
        
        namespace RPGGame;
        
        [GenerateOnReady]
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

        await VerifyHelper.Verify(source, nameof(WhenOnReadyGetMembersHaveNamedParameters_GeneratesReadyMethodThatSetsMarkedMembers));
    }

    [Fact]
    public async Task GivenActionMethodsMarkedWithOnReadyAttribute_GeneratesReadyMethodThatInvokesThemAfterSettingOnReadyGetMembers()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.Attributes;
        
        namespace RPGGame;
        
        [GenerateOnReady]
        public partial class Sword : Node
        {
            [OnReadyGet(path: "%SomeProp", orNull: true)]
            public DummyNode Node { get; set; } = null!;

            [OnReadyGet(orNull: false, path: "%SomeProp2")]
            public DummyNode Node2 { get; set; } = null!;

            [OnReadyGet(path: "%SomeField")]
            private DummyNode Field = null!;

            [OnReady]
            private void InvokeInReady1()
            {
                
            }

            [OnReady]
            private void InvokeInReady2()
            {
                
            }
        }
        """;

        await VerifyHelper.Verify(source, nameof(GivenActionMethodsMarkedWithOnReadyAttribute_GeneratesReadyMethodThatInvokesThemAfterSettingOnReadyGetMembers));
    }
}
