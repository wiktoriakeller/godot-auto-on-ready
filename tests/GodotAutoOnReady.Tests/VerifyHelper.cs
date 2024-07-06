using DiffPlex;
using GodotAutoOnReady.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GodotAutoOnReady.Tests;

public class VerifyHelper
{
    public static Task Verify(string source, bool disableNullable = false)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var readyCallbackName = "OnReady";

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
        
        var settings = new VerifySettings();
        settings.UseStringComparer((received, verified, dict) =>
        {
            var diff = new Differ().CreateWordDiffs(verified, received, true, [' ', '_']);

            if(diff.DiffBlocks.Count == 2)
            {
                foreach(var block in diff.DiffBlocks)
                {
                    var startA = diff.DiffBlocks[0].DeleteStartA;

                    if (startA + 2 < diff.PiecesNew.Length &&
                        (diff.PiecesNew[startA + 1] != "_" ||
                        !diff.PiecesNew[startA + 2].Contains(readyCallbackName)))
                    {
                        return Task.FromResult(new CompareResult(false));
                    }
                }

                return Task.FromResult(new CompareResult(true));
            }

            return Task.FromResult(new CompareResult(diff.DiffBlocks.Count == 0));
        });

        driver = driver.RunGenerators(compilation);
        return Verifier.Verify(driver, settings)
            .UseDirectory("Snapshots");
    }
}
