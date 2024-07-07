using GodotAutoOnReady.SourceGenerators.Attributes;
using GodotAutoOnReady.SourceGenerators.Builders;
using GodotAutoOnReady.SourceGenerators.Common;
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
            "GenerateReadyMethodAttribute.g.cs", SourceText.From(SourceGenerateReadyMethodAttribute.Attribute, Encoding.UTF8)));

        IncrementalValuesProvider<SourceData> dataToGenerate = context.SyntaxProvider.ForAttributeWithMetadataName(
            "GodotAutoOnReady.SourceGenerators.Attributes.GenerateReadyMethodAttribute",
            predicate: static (node, _) => IsPartialClassSyntax(node),
            transform: static (ctx, _) => GetOnReadyData(ctx))
            .Where(static m => m is not null && m.Value.Attributes.Count > 0)
            .Select(static (m, _) => m!.Value);

        context.RegisterImplementationSourceOutput(dataToGenerate, static (spc, onReadyData) => Execute(in onReadyData, spc));
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
        var usingDeclarations = new List<string>();

        foreach(var rootChild in root.ChildNodes())
        {
            if(rootChild is UsingDirectiveSyntax usingSyntax)
            {
                usingDeclarations.Add($"using {usingSyntax.Name!.GetText()};");
            }
        }

        var classModifiers = string.Join(" ", classDeclaration.Modifiers);
        var className = classDeclaration.Identifier.ValueText;
        var baseClass = classDeclaration.BaseList?.ToString().Replace(":", "").Trim() ?? "";
        var classNamespace = GetNamespace(classDeclaration);

        string classInitMethodName = "";

        //Extract name for custom initializer method if it is defined
        for(int i = 0; i < context.Attributes.Length; i++)
        {
            var attribute = context.Attributes[i];

            if(attribute.AttributeClass?.Name == "GenerateReadyMethodAttribute" && attribute.ConstructorArguments.Length == 1)
            {
                var name = attribute.ConstructorArguments.First().Value as string;
                classInitMethodName = string.IsNullOrWhiteSpace(name) ? classInitMethodName : name!;
                break;
            }
        }

        var properties = new List<OnReadyAttributeData>();
        var members = classDeclaration.Members;
        bool hasReadyMethod = false;
        var hasConstructor = false;

        for (int i = 0; i < members.Count; i++)
        {
            var member = members[i];
            string? name = null;
            string? type = null;
            (string Path, bool AllowNull)? arguments = null;

            if(member is MethodDeclarationSyntax methodSyntax)
            {
                var methodName = methodSyntax.Identifier.ValueText;

                if(methodName == SourceData.ReadyMethodName)
                {
                    hasReadyMethod = true;
                }
            }

            if (member is PropertyDeclarationSyntax propSyntax)
            {
                name = propSyntax.Identifier.ValueText;
                type = propSyntax.Type.ToString();
                arguments = GetAttributeArguments(propSyntax.AttributeLists);
            }

            if (member is FieldDeclarationSyntax fieldSyntax)
            {
                name = fieldSyntax.Declaration.Variables.Select(x => x.Identifier.ValueText).First();
                type = fieldSyntax.Declaration.Type.ToString();
                arguments = GetAttributeArguments(fieldSyntax.AttributeLists);
            }

            if (member is ConstructorDeclarationSyntax)
            {
                hasConstructor = true;
            }

            if (name is not null && type is not null && arguments.HasValue)
            {
                properties.Add(new OnReadyAttributeData(name, type, arguments.Value.Path, arguments.Value.AllowNull));
            }
        }

        var classInitMethodModifiers = "public void";
        if(hasReadyMethod && classInitMethodName == "")
        {
            classInitMethodName = SourceData.DefaultInitMethodName;
        }

        if(!hasReadyMethod && classInitMethodName == "")
        {
            classInitMethodName = SourceData.ReadyMethodName;
            classInitMethodModifiers = "public override void";
        }

        return new SourceData(className, 
            classModifiers, 
            classInitMethodName, 
            classInitMethodModifiers, 
            classNamespace,
            baseClass,
            hasConstructor,
            disableNullable,
            assemblyName,
            new EquatableArray<string>(usingDeclarations),
            new EquatableArray<OnReadyAttributeData>(properties));
    }

    private static (string Path, bool AllowNull)? GetAttributeArguments(SyntaxList<AttributeListSyntax> attributeList)
    {
        var attributes = attributeList.SelectMany(x => x.Attributes);
        var onReadyAttribute = attributes.FirstOrDefault(x => x.Name is IdentifierNameSyntax identifier &&
            identifier.Identifier.ValueText == "OnReady");
        string path = "";
        bool allowNull = false;

        if (onReadyAttribute is not null)
        {
            for (int i = 0; i < onReadyAttribute.ArgumentList?.Arguments.Count; i++)
            {
                var argument = onReadyAttribute.ArgumentList.Arguments[i];
                var nameColon = argument.NameColon?.Name.Identifier.ValueText;

                if ((i == 0 && nameColon is null) || nameColon == "path")
                {
                    path = argument.Expression.ChildTokens().First().ValueText;
                }

                if((i == 1 && nameColon is null) || nameColon == "allowNull")
                {
                    var success = bool.TryParse(argument.Expression.ChildTokens().First().ValueText, out bool canBeNull);

                    if (success)
                    {
                        allowNull = canBeNull;
                    }
                }
            }

            return (path, allowNull);
        }

        return null;
    }

    private static string GetNamespace(in BaseTypeDeclarationSyntax syntax)
    {
        //Default namespace case
        string nameSpace = string.Empty;

        //Get the containing syntax node for the type declaration (could be a nested type)
        SyntaxNode? potentialNamespaceParent = syntax.Parent;

        //Keep moving out of nested classes
        while (potentialNamespaceParent != null &&
                potentialNamespaceParent is not NamespaceDeclarationSyntax
                && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        //Build up the final namespace by looping until we no longer have a namespace declaration
        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            nameSpace = namespaceParent.Name.ToString();

            //Keep moving out of the namespace declarations until we run out of nested namespace declarations
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }

        return nameSpace;
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
        var initMethodName = onReadyData.MethodName == SourceData.ReadyMethodName && !onReadyData.CanGenerateReadyMethod() ?
                SourceData.DefaultInitMethodName : onReadyData.MethodName;

        if(onReadyData.CanGenerateReadyMethod())
        {
            string readySignalHandler = SourceGeneratorHelper.ComputeHash(onReadyData.AssemblyName) + "_OnReady";

            //Generate constructor with OnReady handler
            builder.AddMethod("private", onReadyData.ClassName)
                .AddMethodContent($"Ready += {readySignalHandler};");

            //Generate OnReady handler, that calls _Ready method
            builder.AddMethod("private void", readySignalHandler)
                .AddMethodContent("_Ready();");
        }

        //Initialize found properties / fields
        builder.AddMethod(onReadyData.MethodModifiers, initMethodName);

        foreach (var data in onReadyData.Attributes)
        {
            var getNodeSyntax = data.AllowNull ? "GetNodeOrNull" : "GetNode";
            builder.AddMethodContent($"{data.VariableName} = {getNodeSyntax}<{data.TypeName}>(\"{data.Path}\");");
        }
    }
}
