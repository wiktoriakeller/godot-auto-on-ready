using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections;
using System.Collections.Immutable;
using System.Reflection;

namespace GodotAutoOnReady.Tests.Helpers;

internal class SourceGeneratorTestHelpers
{
    public static (ImmutableArray<Diagnostic> Diagnostics, string[] Output) GetGeneratedTrees<T>(
        string[] sources,
        string[] trackingStages,
        bool assertOutputs = true)
        where T : IIncrementalGenerator, new()
    {
        // Convert the source files to SyntaxTrees
        IEnumerable<SyntaxTree> syntaxTrees = sources.Select(static x => CSharpSyntaxTree.ParseText(x));

        // Configure the assembly references
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location))
            .Select(x => MetadataReference.CreateFromFile(x.Location))
            .Concat([MetadataReference.CreateFromFile(typeof(T).Assembly.Location)]);

        // Create a Compilation object
        CSharpCompilation compilation = CSharpCompilation.Create(
            "EnumExtensions.Generated",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Run the generator, get the results, and assert cacheability if applicable
        GeneratorDriverRunResult runResult = RunGeneratorAndAssertOutput<T>(
            compilation, trackingStages, assertOutputs);

        return (runResult.Diagnostics, runResult.GeneratedTrees.Select(x => x.ToString()).ToArray());
    }

    private static GeneratorDriverRunResult RunGeneratorAndAssertOutput<T>(
        CSharpCompilation compilation, 
        string[] trackingNames, 
        bool assertOutput = true)
        where T : IIncrementalGenerator, new()
    {
        ISourceGenerator generator = new T().AsSourceGenerator();

        // Track all the incremental generator outputs
        var options = new GeneratorDriverOptions(
            disabledOutputs: IncrementalGeneratorOutputKind.None,
            trackIncrementalGeneratorSteps: true);

        GeneratorDriver driver = CSharpGeneratorDriver.Create([generator], driverOptions: options);

        var clone = compilation.Clone();

        // First run
        driver = driver.RunGenerators(compilation);
        GeneratorDriverRunResult runResult = driver.GetRunResult();

        if (assertOutput)
        {
            // Second run
            GeneratorDriverRunResult runResult2 = driver
                                                  .RunGenerators(clone)
                                                  .GetRunResult();

            // Compare all the tracked outputs, throw if there's a failure
            AssertRunsEqual(runResult, runResult2, trackingNames);

            // Verify the second run only generated cached source outputs
            runResult2.Results[0]
                        .TrackedOutputSteps
                        .SelectMany(x => x.Value) // step executions
                        .SelectMany(x => x.Outputs) // execution results
                        .Should()
                        .OnlyContain(x => x.Reason == IncrementalStepRunReason.Cached);
        }

        return runResult;
    }

    private static void AssertRunsEqual(
        GeneratorDriverRunResult runResult1,
        GeneratorDriverRunResult runResult2,
        string[] trackingNames)
    {
        var trackedSteps1 = GetTrackedSteps(runResult1, trackingNames);
        var trackedSteps2 = GetTrackedSteps(runResult2, trackingNames);

        // Assert that number of named tracked steps is equal in both runs
        trackedSteps1.Should()
                     .NotBeEmpty()
                     .And.HaveSameCount(trackedSteps2)
                     .And.ContainKeys(trackedSteps2.Keys);

        // Get the IncrementalGeneratorRunStep collection for each run
        foreach (var (trackingName, runSteps1) in trackedSteps1)
        {
            // Assert that both runs produced the same outputs
            var runSteps2 = trackedSteps2[trackingName];
            AssertEqual(runSteps1, runSteps2, trackingName);
        }

        static Dictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> GetTrackedSteps(
            GeneratorDriverRunResult runResult, string[] trackingNames)
            => runResult
                    .Results[0] // We're only running a single generator, so this is safe
                    .TrackedSteps // Get the pipeline outputs
                    .Where(step => trackingNames.Contains(step.Key)) // filter to known steps
                    .ToDictionary(x => x.Key, x => x.Value);
    }

    private static void AssertEqual(
        ImmutableArray<IncrementalGeneratorRunStep> runSteps1,
        ImmutableArray<IncrementalGeneratorRunStep> runSteps2,
        string stepName)
    {
        runSteps1.Should().HaveSameCount(runSteps2);

        for (var i = 0; i < runSteps1.Length; i++)
        {
            var runStep1 = runSteps1[i];
            var runStep2 = runSteps2[i];

            // The outputs should be equal between different runs
            IEnumerable<object> outputs1 = runStep1.Outputs.Select(x => x.Value);
            IEnumerable<object> outputs2 = runStep2.Outputs.Select(x => x.Value);

            outputs1.Should()
                    .Equal(outputs2, $"because {stepName} should produce cacheable outputs");

            // On the second run the results should always be cached or unchanged
            // - Unchanged is when the _input_ has changed, but the output hasn't
            // - Cached is when the the input has not changed, so the cached output is used 
            runStep2.Outputs.Should()
                .OnlyContain(
                    x => x.Reason == IncrementalStepRunReason.Cached || x.Reason == IncrementalStepRunReason.Unchanged,
                    $"{stepName} expected to have reason {IncrementalStepRunReason.Cached} or {IncrementalStepRunReason.Unchanged}");

            AssertObjectGraph(runStep1, stepName);
        }
    }
    
    private static void AssertObjectGraph(IncrementalGeneratorRunStep runStep, string stepName)
    {
        // Including the stepName in error messages to make it easy to isolate issues
        var because = $"{stepName} shouldn't contain banned symbols";
        var visited = new HashSet<object>();

        // Check all of the outputs
        foreach (var (obj, _) in runStep.Outputs)
        {
            Visit(obj);
        }

        void Visit(object node)
        {
            if (node is null || !visited.Add(node))
            {
                return;
            }

            // Make sure it's not a banned type
            node.Should()
                .NotBeOfType<Compilation>(because)
                .And.NotBeOfType<ISymbol>(because)
                .And.NotBeOfType<SyntaxNode>(because);

            // Examine the object
            Type type = node.GetType();
            if (type.IsPrimitive || type.IsEnum || type == typeof(string))
            {
                return;
            }

            // If the object is a collection, check each of the values
            if (node is IEnumerable collection and not string)
            {
                foreach (object element in collection)
                {
                    // recursively check each element in the collection
                    Visit(element);
                }

                return;
            }

            // Recursively check each field in the object
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var fieldValue = field.GetValue(node);

                if(fieldValue is not null)
                {
                    Visit(fieldValue);
                }
            }
        }
    }
}
