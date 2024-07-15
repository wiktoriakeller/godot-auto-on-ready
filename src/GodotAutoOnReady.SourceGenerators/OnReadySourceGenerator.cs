using GodotAutoOnReady.SourceGenerators.Attributes;
using GodotAutoOnReady.SourceGenerators.Builders;
using GodotAutoOnReady.SourceGenerators.Common;
using GodotAutoOnReady.SourceGenerators.Helpers;
using GodotAutoOnReady.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace GodotAutoOnReady.SourceGenerators;

[Generator]
public class OnReadySourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource(
            "OnReadyAttribute.g.cs", SourceText.From(SourceOnReadyAttribute.Attribute, Encoding.UTF8)));

        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource(
            "OnReadyGetAttribute.g.cs", SourceText.From(SourceOnReadyGetAttribute.Attribute, Encoding.UTF8)));

        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource(
            "GenerateOnReadyAttribute.g.cs", SourceText.From(SourceGenerateOnReadyAttribute.Attribute, Encoding.UTF8)));

        IncrementalValuesProvider<SourceData> dataToGenerate = context.SyntaxProvider.ForAttributeWithMetadataName(
            "GodotAutoOnReady.Attributes.GenerateOnReadyAttribute",
            predicate: static (node, _) => IsPartialClassSyntax(node),
            transform: static (ctx, _) => GetOnReadyData(ctx))
            .Where(static m => m is not null && m.Value.Members.Count > 0)
            .Select(static (m, _) => m!.Value);

        context.RegisterSourceOutput(dataToGenerate, static (spc, onReadyData) => Execute(in onReadyData, spc));
    }

    private static bool IsPartialClassSyntax(SyntaxNode node)
    {
        if (node is ClassDeclarationSyntax classDeclaration &&
            classDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)))
        {
            return true;
        }

        return false;
    }

    private static SourceData? GetOnReadyData(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetNode is not ClassDeclarationSyntax classDeclaration)
        {
            return null;
        }

        bool disableNullable = context.SemanticModel.Compilation.Options.NullableContextOptions != NullableContextOptions.Disable;
        var assemblyName = context.SemanticModel.Compilation.AssemblyName ?? classDeclaration.Identifier.Text;
        var root = context.SemanticModel.SyntaxTree.GetRoot();
        var usingDeclarations = SourceGeneratorHelper.GetUsingDeclarations(root);

        string className = classDeclaration.Identifier.ValueText;
        string classModifiers = string.Join(" ", classDeclaration.Modifiers);
        string baseClass = classDeclaration.BaseList?.ToString().Replace(":", "").Trim() ?? "";
        string classNamespace = SourceGeneratorHelper.GetNamespace(classDeclaration);

        string classInitMethodName = SourceData.ReadyMethodName;
        string classInitMethodModifiers = SourceData.ReadyMethodModifiers;

        //Extract name for custom initializer method if it is defined
        for (int i = 0; i < context.Attributes.Length; i++)
        {
            var attribute = context.Attributes[i];

            if(attribute.AttributeClass?.Name == SourceGenerateOnReadyAttribute.AttributeName && 
                attribute.ConstructorArguments.Length == 1)
            {
                var name = attribute.ConstructorArguments.First().Value as string;

                if (!string.IsNullOrEmpty(name))
                {
                    classInitMethodName = name!;
                    classInitMethodModifiers = SourceData.InitMethodMofidiers;
                    break;
                }
            }
        }

        var properties = new List<OnReadyGetAttributeData>();
        var onReadyMethods = new List<string>();
        bool hasReadyMethod = false;
        bool hasConstructor = false;

        for (int i = 0; i < classDeclaration.Members.Count; i++)
        {
            var member = classDeclaration.Members[i];
            string? name = null;
            string? type = null;
            (string Path, bool OrNull)? arguments = null;

            if (member is ConstructorDeclarationSyntax)
            {
                hasConstructor = true;
            }

            if (member is MethodDeclarationSyntax methodSyntax)
            {
                var methodName = methodSyntax.Identifier.ValueText;

                if(methodName == SourceData.ReadyMethodName)
                {
                    hasReadyMethod = true;
                }

                //Add Action type methods with OnReady attribute
                if(SourceGeneratorHelper.TryGetAttribute(methodSyntax.AttributeLists, SourceOnReadyAttribute.AttributeName, out var methodAttribute) &&
                    methodSyntax.ParameterList.Parameters.Count == 0 &&
                    methodSyntax.ReturnType is PredefinedTypeSyntax predefined && predefined.Keyword.IsKind(SyntaxKind.VoidKeyword))
                {
                    onReadyMethods.Add(methodName);
                }
            }

            if (member is PropertyDeclarationSyntax propSyntax &&
                SourceGeneratorHelper.TryGetAttribute(propSyntax.AttributeLists, SourceOnReadyGetAttribute.AttributeName, out var propAttribute))
            {
                name = propSyntax.Identifier.ValueText;
                type = propSyntax.Type.ToString();
                arguments = GetOnReadyArguments(propAttribute!);
            }

            if (member is FieldDeclarationSyntax fieldSyntax &&
                SourceGeneratorHelper.TryGetAttribute(fieldSyntax.AttributeLists, SourceOnReadyGetAttribute.AttributeName, out var fieldAttribute))
            {
                name = fieldSyntax.Declaration.Variables.Select(x => x.Identifier.ValueText).First();
                type = fieldSyntax.Declaration.Type.ToString();
                arguments = GetOnReadyArguments(fieldAttribute!);
            }

            if (name is not null && type is not null && arguments.HasValue)
            {
                properties.Add(new OnReadyGetAttributeData(name, type, arguments.Value.Path, arguments.Value.OrNull));
            }
        }

        if((hasReadyMethod || hasConstructor) && classInitMethodName == SourceData.ReadyMethodName)
        {
            classInitMethodName = SourceData.DefaultInitMethodName;
            classInitMethodModifiers = SourceData.InitMethodMofidiers;
        }

        return new SourceData(
            className, 
            classModifiers, 
            classInitMethodName, 
            classInitMethodModifiers, 
            classNamespace,
            baseClass,
            disableNullable,
            assemblyName,
            usingDeclarations,
            new EquatableArray<string>(onReadyMethods),
            new EquatableArray<OnReadyGetAttributeData>(properties));
    }

    private static (string Path, bool OrNull)? GetOnReadyArguments(in AttributeSyntax onReadyAttribute)
    {
        string path = "";
        bool orNull = false;

        for (int i = 0; i < onReadyAttribute.ArgumentList?.Arguments.Count; i++)
        {
            var argument = onReadyAttribute.ArgumentList.Arguments[i];
            var nameColon = argument.NameColon?.Name.Identifier.ValueText;

            if (argument.NameEquals?.Name.Identifier.ValueText == "OrNull")
            {
                var success = bool.TryParse(argument.Expression.ChildTokens().First().ValueText, out bool canBeNull);

                if (success)
                {
                    orNull = canBeNull;
                }
            }
            else if(i == 0 || nameColon == "path")
            {
                path = argument.Expression.ChildTokens().First().ValueText;
            }
        }

        return (path, orNull);
    }

    private static void Execute(in SourceData onReadyData, in SourceProductionContext spc)
    {
        var builder = new SourceBuilder();
        builder.AddLine("// <auto-generated />")
            .AddNullableDisable(onReadyData.NullableDisable)
            .AddLine();

        foreach(var usingDeclaration in onReadyData.UsingDeclarations)
        {
            builder.AddLine(usingDeclaration);
        }
        builder.AddLine();

        if(onReadyData.ClassNamespace != "")
        {
            builder.AddNamespace(onReadyData.ClassNamespace);
        }

        builder.AddClass(onReadyData.ClassModifiers, onReadyData.ClassName, onReadyData.BaseClass);

        GenerateInitializerMethod(onReadyData, builder);

        var code = builder.BuildSource();
        
        spc.AddSource($"{onReadyData.ClassName}.g.cs", SourceText.From(code, Encoding.UTF8));
    }

    private static void GenerateInitializerMethod(in SourceData onReadyData, in SourceBuilder builder)
    {
        var initMethodName = onReadyData.MethodName == SourceData.ReadyMethodName && !onReadyData.GenerateReadyMethod() ?
                SourceData.DefaultInitMethodName : onReadyData.MethodName;

        if(onReadyData.GenerateReadyMethod())
        {
            string readySignalHandler = HashHelper.ComputeHash(onReadyData.AssemblyName) + "_OnReady";

            //Generate constructor with OnReady handler
            builder.AddMethod("private", onReadyData.ClassName)
                .AddMethodContent($"Ready += {readySignalHandler};");

            //Generate OnReady handler, that calls _Ready method
            builder.AddMethod("private void", readySignalHandler)
                .AddMethodContent("_Ready();");
        }

        //Initialize found properties / fields
        builder.AddMethod(onReadyData.MethodModifiers, initMethodName);

        if (onReadyData.GenerateReadyMethod())
        {
            builder.AddMethodContent("base._Ready();");
        }

        foreach (var data in onReadyData.Members)
        {
            var isResource = data.Path.StartsWith("res://");
            var getSyntax = "GD.Load";

            if (!isResource)
            {
                getSyntax = data.OrNull ? "GetNodeOrNull" : "GetNode";
            }

            builder.AddMethodContent($"{data.VariableName} = {getSyntax}<{data.TypeName}>(\"{data.Path}\");");
        }

        //Add OnReady action methods invocations
        foreach(var method in onReadyData.OnReadyMethods)
        {
            builder.AddMethodContent($"{method}();");
        }
    }
}
