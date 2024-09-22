using Microsoft.CodeAnalysis;

namespace GodotAutoOnReady.SourceGenerator.Diagnostics;

internal static class DiagnosticsHelper
{
    internal static DiagnosticMessage ReadyMarkedWithOnReady = 
        new("_Ready method can't be marked with OnReady attribute", "_Ready method can't be marked with OnReady attribute", 1, DiagnosticSeverity.Warning);

    internal static DiagnosticMessage ReadyMethodNotGenerated = new("Default constructor or _Ready method is already declared",
        "_Ready method was not generated because a default constructor or a _Ready method is already declared and no custom setup method name was provided.\r\nFields setup was generated in OnReadySetup method.",
        2,
        DiagnosticSeverity.Warning);

    internal static DiagnosticMessage NoNodeType = new("NodeType was not provided",
        "Path points to a property but no NodeType was provided.",
        3,
        DiagnosticSeverity.Warning);

    public static Diagnostic CreateDiagnostic(
        Location location,
        DiagnosticMessage message)
    {
        var descriptor = new DiagnosticDescriptor(
            id: $"GAOR00{message.Id}",
            title: message.Title,
            messageFormat: message.Message,
            category: "Design",
            defaultSeverity: message.Severity,
            isEnabledByDefault: true);

        return Diagnostic.Create(descriptor, location);
    }
}
