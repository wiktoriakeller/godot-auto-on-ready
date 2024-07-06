using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using GodotAutoOnReady.SourceGenerators;
using System.Diagnostics;

namespace GodotAutoOnReady.Tests;

public class VerifyHelper
{
    public static Task Verify(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        IEnumerable<PortableExecutableReference> references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        };

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { syntaxTree },
            references: references);

        var generator = new OnReadySourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        var settings = new VerifySettings();

        settings.OnFirstVerify(
            (receivedFile, receivedText, autoVerify) =>
            {
                Debug.WriteLine(receivedFile);
                Debug.WriteLine(receivedText);
                return Task.CompletedTask;
            });
        settings.OnVerifyMismatch(
            (filePair, message, autoVerify) =>
            {
                Debug.WriteLine(filePair.ReceivedPath);
                Debug.WriteLine(filePair.VerifiedPath);
                Debug.WriteLine(message);
                return Task.CompletedTask;
            });

        driver = driver.RunGenerators(compilation);
        return Verifier.Verify(driver, settings)
            .UseDirectory("Snapshots")
            .UseDiffPlex();
    }
}
