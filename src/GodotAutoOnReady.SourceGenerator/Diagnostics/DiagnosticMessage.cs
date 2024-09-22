using Microsoft.CodeAnalysis;

namespace GodotAutoOnReady.SourceGenerator.Diagnostics;

internal record struct DiagnosticMessage(string Title, string Message, int Id, DiagnosticSeverity Severity);