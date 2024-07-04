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
            "OnReadyInit.g.cs", SourceText.From(SourceOnReadyInitAttribute.Attribute, Encoding.UTF8)));

        IncrementalValuesProvider<OnReadyData> dataToGenerate = context.SyntaxProvider.ForAttributeWithMetadataName(
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

    private static OnReadyData? GetOnReadyData(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetNode is not ClassDeclarationSyntax classDeclaration)
        {
            return null;
        }

        var classModifiers = string.Join(" ", classDeclaration.Modifiers);
        var className = classDeclaration.Identifier.ValueText;
        var baseClass = classDeclaration.BaseList?.ToString().Replace(":", "").Trim() ?? "";
        var classNamespace = GetNamespace(classDeclaration);

        string classInitMethodName = "";
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
            string? path = null;

            if(member is MethodDeclarationSyntax methodSyntax)
            {
                var methodName = methodSyntax.Identifier.ValueText;

                if(methodName == OnReadyData.ReadyMethodName && methodSyntax.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)))
                {
                    hasReadyMethod = true;
                }
            }

            if (member is PropertyDeclarationSyntax propSyntax)
            {
                name = propSyntax.Identifier.ValueText;
                type = propSyntax.Type.ToString();
                path = GetOnReadyPath(propSyntax.AttributeLists);
            }

            if (member is FieldDeclarationSyntax fieldSyntax)
            {
                name = fieldSyntax.Declaration.Variables.Select(x => x.Identifier.ValueText).First();
                type = fieldSyntax.Declaration.Type.ToString();
                path = GetOnReadyPath(fieldSyntax.AttributeLists);
            }

            if(member is ConstructorDeclarationSyntax)
            {
                hasConstructor = true;
            }

            if (name is not null && type is not null && path is not null)
            {
                properties.Add(new OnReadyAttributeData(name, type, path));
            }
        }

        var classInitMethodModifiers = "public void";
        if(hasReadyMethod && classInitMethodName == "")
        {
            classInitMethodName = OnReadyData.DefaultInitMethodName;
        }

        if(!hasReadyMethod && classInitMethodName == "")
        {
            classInitMethodName = OnReadyData.ReadyMethodName;
            classInitMethodModifiers = "public override void";
        }

        return new OnReadyData(className, 
            classModifiers, 
            classInitMethodName, 
            classInitMethodModifiers, 
            classNamespace,
            baseClass,
            hasConstructor,
            new EquatableArray<OnReadyAttributeData>(properties));
    }

    private static string? GetOnReadyPath(SyntaxList<AttributeListSyntax> attributeList)
    {
        var attributes = attributeList.SelectMany(x => x.Attributes);
        var onReadyAttribute = attributes.FirstOrDefault(x => x.Name is IdentifierNameSyntax identifier &&
            identifier.Identifier.ValueText == "OnReady");

        if (onReadyAttribute is not null)
        {
            for (int j = 0; j < onReadyAttribute.ArgumentList?.Arguments.Count; j++)
            {
                var argument = onReadyAttribute.ArgumentList.Arguments[j];
                var nameColon = argument.NameColon?.Name.Identifier.ValueText;

                if ((j == 0 && nameColon is null) || nameColon == "path")
                {
                    return argument.Expression.ChildTokens().First().ValueText;
                }
            }
        }

        return null;
    }

    private static string GetNamespace(in BaseTypeDeclarationSyntax syntax)
    {
        // If we don't have a namespace at all we'll return an empty string
        // This accounts for the "default namespace" case
        string nameSpace = string.Empty;

        // Get the containing syntax node for the type declaration
        // (could be a nested type, for example)
        SyntaxNode? potentialNamespaceParent = syntax.Parent;

        // Keep moving "out" of nested classes etc until we get to a namespace
        // or until we run out of parents
        while (potentialNamespaceParent != null &&
                potentialNamespaceParent is not NamespaceDeclarationSyntax
                && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        // Build up the final namespace by looping until we no longer have a namespace declaration
        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            nameSpace = namespaceParent.Name.ToString();

            // Keep moving "out" of the namespace declarations until we run out of nested namespace declarations
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                // Add the outer namespace as a prefix to the final namespace
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }

        return nameSpace;
    }

    private static void Execute(in OnReadyData onReadyData, in SourceProductionContext spc)
    {
        var builder = new SourceBuilder();
        builder.AddLine("// <auto-generated />")
            .AddLine("#nullable disable")
            .AddLine()
            .AddLine("using Godot;")
            .AddLine("using GodotAutoOnReady.SourceGenerators.Attributes;")
            .AddLine();

        if(onReadyData.ClassNamespace != "")
        {
            builder.AddNamespace(onReadyData.ClassNamespace);
        }

        builder.AddClass(onReadyData.ClassModifiers, onReadyData.ClassName, onReadyData.BaseClass);

        GenerateInitializerMethod(onReadyData, builder);

        var code = builder.BuildSource();
        spc.AddSource($"{onReadyData.ClassName}.g.cs", SourceText.From(code, Encoding.UTF8));
    }

    private static void GenerateInitializerMethod(in OnReadyData onReadyData, in SourceBuilder builder)
    {
        var initMethodName = onReadyData.MethodName == OnReadyData.ReadyMethodName && !onReadyData.CanGenerateReadyMethod() ?
                OnReadyData.DefaultInitMethodName : onReadyData.MethodName;

        if(onReadyData.CanGenerateReadyMethod())
        {
            string readySignalHandler = GenerateRandomMethodName(20, "_OnReady");

            //Generate constructor with OnReady handler
            builder.AddMethod("private", onReadyData.ClassName)
                .AddMethodContent("Ready += {readySignalHandler};");

            //Generate OnReady handler, that calls _Ready method
            builder.AddMethod("private void", readySignalHandler)
                .AddMethodContent("_Ready();");
        }

        //Initialize found properties / fields
        builder.AddMethod(onReadyData.MethodModifiers, initMethodName);

        foreach (var data in onReadyData.Attributes)
        {
            builder.AddMethodContent($"{data.VariableName} = GetNode<{data.TypeName}>(\"{data.Path}\");");
        }
    }

    private static string GenerateRandomMethodName(int length, string postfix)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var sb = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            sb.Append(chars[random.Next(chars.Length)]);
        }

        return sb.Append(postfix).ToString();
    }
}
