using DiffPlex;
using GodotAutoOnReady.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GodotAutoOnReady.Tests;

public class VerifyHelper
{
    public static Task Verify(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var readyCallbackName = "OnReady";

        IEnumerable<PortableExecutableReference> references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        ];

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: [ syntaxTree ],
            references: references);

        var generator = new OnReadySourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        
        var settings = new VerifySettings();
        settings.UseStringComparer((received, verified, dict) =>
        {
            var diff = new Differ().CreateWordDiffs(verified, received, true, [' ', '_']);

            if(diff.PiecesOld.Any(x => x.Contains(readyCallbackName) &&
               diff.DiffBlocks.Count == 1))
            {
                return Task.FromResult(new CompareResult(true));
            }
 
            return Task.FromResult(new CompareResult(diff.DiffBlocks.Count == 0));
        });

        driver = driver.RunGenerators(compilation);
        return Verifier.Verify(driver, settings)
            .UseDirectory("Snapshots");
    }
}
