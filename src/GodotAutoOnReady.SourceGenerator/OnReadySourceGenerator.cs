using GodotAutoOnReady.SourceGenerator.Attributes;
using GodotAutoOnReady.SourceGenerator.Builders;
using GodotAutoOnReady.SourceGenerator.Common;
using GodotAutoOnReady.SourceGenerator.Diagnostics;
using GodotAutoOnReady.SourceGenerator.Helpers;
using GodotAutoOnReady.SourceGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace GodotAutoOnReady.SourceGenerator;

[Generator]
public class OnReadySourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "OnReadyAttribute.g.cs", SourceText.From(OnReadyAttributeSource.Source, Encoding.UTF8)));

        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource(
            "GetNodeAttribute.g.cs", SourceText.From(GetNodeAttributeSource.Source, Encoding.UTF8)));

        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "GetResAttribute.g.cs", SourceText.From(GetResAttributeSource.Source, Encoding.UTF8)));

        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource(
            "GenerateOnReadyAttribute.g.cs", SourceText.From(GenerateOnReadyAttributeSource.Source, Encoding.UTF8)));

        IncrementalValuesProvider<SourceData> dataToGenerate = context.SyntaxProvider.ForAttributeWithMetadataName(
            "GodotAutoOnReady.Attributes.GenerateOnReadyAttribute",
            predicate: static (node, _) => IsPartialClassSyntax(node),
            transform: static (ctx, _) => GetOnReadyData(ctx))
            .Where(static m => m is not null && m.Value.Members.Count > 0)
            .Select(static (m, _) => m!.Value)
            .WithTrackingName(TrackingNames.DataExtrction);

        context.RegisterSourceOutput(dataToGenerate, static (spc, onReadyData) => Execute(in onReadyData, spc));
    }

    private static bool IsPartialClassSyntax(SyntaxNode node) 
        => node is ClassDeclarationSyntax classDeclaration && classDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword));

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

        string classNamespace = SourceGeneratorHelper.GetNamespace(classDeclaration);
        string className = classDeclaration.Identifier.ValueText;
        string classModifiers = string.Join(" ", classDeclaration.Modifiers);
        string baseClass = classDeclaration.BaseList?.ToString().Replace(":", "").Trim() ?? "";

        string classInitMethodName = SourceData.ReadyMethodName;
        string classInitMethodModifiers = SourceData.ReadyMethodModifiers;
        var diagnostics = new List<Diagnostic>();

        //Extract name for custom initializer method if it is defined
        for (int i = 0; i < context.Attributes.Length; i++)
        {
            var attribute = context.Attributes[i];

            if (attribute.AttributeClass?.Name == GenerateOnReadyAttributeSource.Name &&
                attribute.NamedArguments.Length == 1)
            {
                var name = attribute.NamedArguments.First().Value.Value?.ToString();

                if (!string.IsNullOrEmpty(name))
                {
                    classInitMethodName = name!;
                    classInitMethodModifiers = SourceData.InitMethodMofidiers;
                    break;
                }
            }
        }

        var members = new List<BaseAttributeData>();
        var attributeDataFactory = new AttributeDataFactory();

        bool hasReadyMethod = false;
        bool hasConstructor = classDeclaration.Members.Any(x => x is ConstructorDeclarationSyntax);

        var classSymbols = context.SemanticModel.GetDeclaredSymbol(classDeclaration)?.GetMembers() ?? [];

        for (int i = 0; i < classSymbols.Length; i++)
        {
            var symbol = classSymbols[i];

            if (symbol is IMethodSymbol methodSymbol &&
                methodSymbol.Name == SourceData.ReadyMethodName)
            {
                hasReadyMethod = true;

                if(methodSymbol.GetAttributes().TryGetAttribute(out _, Attributes.OnReadyAttributeSource.Name))
                {
                    diagnostics.Add(CreateDiagnostic(methodSymbol.Locations.First(), 2, ErrorMessages.ReadyMarkedWithOnReady));
                }

                continue;
            }

            var data = attributeDataFactory.GetAttributeData(symbol);

            if (data is not null)
            {
                members.Add(data);
            }
        }

        if((hasReadyMethod || hasConstructor) && classInitMethodName == SourceData.ReadyMethodName)
        {
            classInitMethodName = SourceData.DefaultInitMethodName;
            classInitMethodModifiers = SourceData.InitMethodMofidiers;
            diagnostics.Add(CreateDiagnostic(classDeclaration.GetLocation(), 1, ErrorMessages.ReadyMethodNotGenerated));
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
            members,
            diagnostics);
    }

    private static void Execute(in SourceData onReadyData, in SourceProductionContext spc)
    {
        foreach(var diagnostic in onReadyData.Diagnostics)
        {
            spc.ReportDiagnostic(diagnostic);
        }

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
        var initMethodName = onReadyData.MethodName == SourceData.ReadyMethodName && !onReadyData.GenerateReadyMethod ?
                SourceData.DefaultInitMethodName : onReadyData.MethodName;

        if(onReadyData.GenerateReadyMethod)
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

        if (onReadyData.GenerateReadyMethod)
        {
            builder.AddMethodContent("base._Ready();");
        }

        foreach(var member in onReadyData.Members)
        {
            builder.AddMethodContent(member.GetSourceCode());
        }
    }

    private static Diagnostic CreateDiagnostic(
        Location location, 
        int id, 
        DiagnosticMessage message,
        DiagnosticSeverity severity = DiagnosticSeverity.Warning)
    {
        var descriptor = new DiagnosticDescriptor(
            id: $"GAR00{id}",
            title: message.Title,
            messageFormat: message.Message,
            category: "Design",
            defaultSeverity: severity,
            isEnabledByDefault: true);

        return Diagnostic.Create(descriptor, location);
    }
}
