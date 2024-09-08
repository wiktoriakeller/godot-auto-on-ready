using GodotAutoOnReady.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GodotAutoOnReady.Tests;

public class VerifyHelper
{
    public static Task Verify(string source, string testName, bool disableNullable = false)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        IEnumerable<PortableExecutableReference> references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        ];

        var nullableContextOptions = disableNullable ? NullableContextOptions.Disable : NullableContextOptions.Enable;
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: [ syntaxTree ],
            references: references,
            options: new CSharpCompilationOptions(nullableContextOptions: nullableContextOptions, outputKind: OutputKind.NetModule));

        var generator = new OnReadySourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation);
        return Verifier.Verify(driver)
            .UseDirectory(@$"Snapshots{Path.DirectorySeparatorChar}/{testName}")
            .UseTypeName("Test")
            .UseMethodName("Gen")
            .AutoVerify();
    }
}
