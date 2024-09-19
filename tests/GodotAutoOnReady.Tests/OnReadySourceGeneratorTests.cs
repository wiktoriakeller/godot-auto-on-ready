using FluentAssertions;
using FluentAssertions.Execution;
using GodotAutoOnReady.SourceGenerator;
using GodotAutoOnReady.SourceGenerator.Attributes;
using GodotAutoOnReady.SourceGenerator.Common;
using GodotAutoOnReady.Tests.Helpers;

namespace GodotAutoOnReady.Tests;

public class OnReadySourceGeneratorTests
{
    [Fact]
    public async Task GivenGetNodeMembers_GeneratesReadyMethodThatSetsTheMarkedMembers()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.Attributes;

        namespace RPGGame;

        [GenerateOnReady]
        public partial class Sword : Node
        {
            [GetNode("%SomeProp")]
            public DummyNode Node { get; set; } = null!;

            [GetNode("%SomeField")]
            private DummyNode Field = null!;
        }
        """;

        await VerifyHelper.Verify(source, nameof(GivenGetNodeMembers_GeneratesReadyMethodThatSetsTheMarkedMembers));
    }

    [Fact]
    public async Task GivenMembersWithAndWithoutGetNode_GeneratesReadyMethodThatSetsOnlyTheMarkedMembers()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.Attributes;

        namespace RPGGame;

        [GenerateOnReady]
        public partial class Sword : Node
        {
            [GetNode("%SomeProp")]
            public DummyNode Node { get; set; } = null!;

            [GetNode("%SomeField")]
            private DummyNode Field = null!;

            public DummyNode Node2 { get; set; } = null!;
        
            private DummyNode Field2 = null!;
        }
        """;

        await VerifyHelper.Verify(source, nameof(GivenMembersWithAndWithoutGetNode_GeneratesReadyMethodThatSetsOnlyTheMarkedMembers));
    }

    [Fact]
    public async Task GivenCustomSetupMethodName_GeneratesMethodNamedInitThatSetsTheMarkedMembers()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.Attributes;

        namespace RPGGame;

        [GenerateOnReady(SetupMethod = "Init")]
        public partial class Sword : Node
        {
            [GetNode("/root/Title")]
            public DummyNode Node { get; set; } = null!;

            [GetNode("/root/Titled")]
            private DummyNode Field = null!;
        }
        """;

        await VerifyHelper.Verify(source, nameof(GivenCustomSetupMethodName_GeneratesMethodNamedInitThatSetsTheMarkedMembers));
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
            [GetNode("%SomeProp")]
            public DummyNode Node { get; set; } = null!;

            [GetNode("%SomeField")]
            private DummyNode Field = null!;
        }
        """;

        await VerifyHelper.Verify(source, nameof(WhenNamespaceIsNotSpecified_GeneratesClassWithReadyMethodWithoutNamespace));
    }

    [Fact]
    public async Task WhenReadyDeclarationAlreadyExists_GeneratesSetupMethodWithDefaultName()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.Attributes;

        namespace RPGGame;

        [GenerateOnReady]
        public partial class Sword : Node
        {
            [GetNode("%SomeProp")]
            public DummyNode Node { get; set; } = null!;

            [GetNode("%SomeField")]
            private DummyNode Field = null!;

            public override void _Ready()
            {
                OnReadySetup(); //Generated method for nodes initialization
            }
        }
        """;

        await VerifyHelper.Verify(source, nameof(WhenReadyDeclarationAlreadyExists_GeneratesSetupMethodWithDefaultName));
    }

    [Fact]
    public async Task WhenDefaultConstructorExists_GeneratesSetupMethodWithDefaultName()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.Attributes;

        namespace RPGGame;

        [GenerateOnReady]
        public partial class Sword : Node
        {
            [GetNode("%SomeProp")]
            public DummyNode Node { get; set; } = null!;

            [GetNode("%SomeField")]
            private DummyNode Field = null!;

            public Sword()
            {

            }
        }
        """;

        await VerifyHelper.Verify(source, nameof(WhenDefaultConstructorExists_GeneratesSetupMethodWithDefaultName));
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
            [GetNode("%SomeProp")]
            public DummyNode Node { get; set; } = null!;
        
            [GetNode("%SomeField")]
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
            [GetNode("%SomeProp")]
            public DummyNode Node { get; set; } = null!;
        
            [GetNode("%SomeField")]
            private DummyNode Field = null!;
        }
        """;

        await VerifyHelper.Verify(source, nameof(WhenAdditionalUsingsAreDeclared_GeneratesFileWithCopiedUsings));
    }

    [Fact]
    public async Task WhenGetNodeAllowsNull_GeneratesReadyMethodThatSetsMembersWithGetNodeOrNull()
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
            [GetNode("%SomeProp", OrNull = true)]
            public DummyNode Node { get; set; } = null!;

            [GetNode(path: "%SomeProp2", OrNull = true)]
            public DummyNode Node2 { get; set; } = null!;

            [GetNode("%SomeField")]
            private DummyNode Field = null!;

            [GetNode("%SomeField2", OrNull = false)]
            public DummyNode Field2 = null!;
        }
        """;

        await VerifyHelper.Verify(source, nameof(WhenGetNodeAllowsNull_GeneratesReadyMethodThatSetsMembersWithGetNodeOrNull));
    }

    [Fact]
    public async Task GivenMethodsWithOnReadyAttribute_GeneratesReadyMethodThatInvokesThemAfterSettingGetNodeMembers()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.Attributes;
        
        namespace RPGGame;
        
        [GenerateOnReady]
        public partial class Sword : Node
        {
            [GetNode("%SomeProp", OrNull = true)]
            public DummyNode Node { get; set; } = null!;
        
            [GetNode(OrNull = false)]
            public DummyNode Node2 { get; set; } = null!;
        
            [GetNode("%SomeField")]
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

        await VerifyHelper.Verify(source, nameof(GivenMethodsWithOnReadyAttribute_GeneratesReadyMethodThatInvokesThemAfterSettingGetNodeMembers));
    }

    [Fact]
    public async Task GivenMethodsWithOnReadyAttributeWithOrderParam_ReadyMethodInvokesThemAccordingToTheOrder()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.Attributes;
        
        namespace RPGGame;
        
        [GenerateOnReady]
        public partial class Sword : Node
        {
            [GetNode("%SomeProp")]
            public DummyNode Node { get; set; } = null!;
        
            [GetNode("%SomeField")]
            private DummyNode Field = null!;

            [OnReady(Order = 2)]
            private void InvokeInReady2()
            {
                
            }

            [OnReady(Order = 3)]
            private void InvokeInReady3()
            {
                
            }

            [OnReady(Order = 1)]
            private void InvokeInReady1()
            {
                
            }

            [OnReady]
            private void InvokeInReady0()
            {
                
            }
        }
        """;

        await VerifyHelper.Verify(source, nameof(GivenMethodsWithOnReadyAttributeWithOrderParam_ReadyMethodInvokesThemAccordingToTheOrder));
    }

    [Fact]
    public async Task GivenMembersWithGetRes_AssignsPropertiesInReadyUsingGDLoad()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.Attributes;
        
        namespace RPGGame;
        
        [GenerateOnReady]
        public partial class Sword : Node
        {
            [GetRes("res://iconprop.svg")]
            public Texture2D TextProp { get; set; } = null!;

            [GetRes(path: "res://iconprop2.svg")]
            public Texture2D TextProp2 { get; set; } = null!;

            [GetRes("res://iconfield.svg")]
            public Texture2D TextField = null!;
        }
        """;

        await VerifyHelper.Verify(source, nameof(GivenMembersWithGetRes_AssignsPropertiesInReadyUsingGDLoad));
    }

    [Fact]
    public async Task When_ReadyMethodIsMarkedWithOnReadyAttribute_GenerateDiagnosticWithWarning()
    {
        var source = """
        using Godot;
        using GodotAutoOnReady.Attributes;
        
        namespace RPGGame;
        
        [GenerateOnReady]
        public partial class Sword : Node
        {
            [GetNode("%SomeProp")]
            public DummyNode Node { get; set; } = null!;
        
            [GetNode("%SomeField")]
            private DummyNode Field = null!;

            [OnReady]
            private void _Ready() //Don't invoke in setup
            {
                
            }

            [OnReady]
            private void InvokeInReady1()
            {
                
            }
        }
        """;

        await VerifyHelper.Verify(source, nameof(When_ReadyMethodIsMarkedWithOnReadyAttribute_GenerateDiagnosticWithWarning));
    }

    [Fact]
    public void SourceGeneratorShouldUseCacheFromPreviousIterations()
    {
        const string input = """
            using Godot;
            using GodotAutoOnReady.Attributes;
            
            namespace RPGGame;
            
            [GenerateOnReady(SetupMethod = "Init")]
            public partial class Sword : Node
            {
                [GetNode("/root/Title")]
                public DummyNode Node { get; set; } = null!;
            
                [GetNode("/root/Titled")]
                private DummyNode Field = null!;
            }
            """;

        string[] expected = [
            OnReadyAttributeSource.Source,
            GetNodeAttributeSource.Source,
            GetResAttributeSource.Source,
            GenerateOnReadyAttributeSource.Source,
            """
            // <auto-generated />

            using Godot;
            using GodotAutoOnReady.Attributes;

            namespace RPGGame
            {
            	public partial class Sword : Node
            	{
            		public void Init()
            		{
            			Node = GetNode<DummyNode>("/root/Title");
            			Field = GetNode<DummyNode>("/root/Titled");
            		}
            	}
            }
            
            """
            ];

        var (diagnostics, output)
            = SourceGeneratorTestHelpers.GetGeneratedTrees<OnReadySourceGenerator>([input], [TrackingNames.DataExtrction]);

        using var s = new AssertionScope();
        diagnostics.Should().BeEmpty();
        output.Length.Should().Be(expected.Length);
        output.Should().Equal(expected, (s1, s2) => s1.ReplaceLineEndings().Equals(s2.ReplaceLineEndings(), StringComparison.OrdinalIgnoreCase));
    }
}
