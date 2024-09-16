namespace GodotAutoOnReady.SourceGenerators.Diagnostics;

internal static class ErrorMessages
{
    public static DiagnosticMessage ReadyMarkedWithOnReady = 
        new("_Ready method can't be marked with OnReady attribute", "_Ready method can't be marked with OnReady attribute");

    public static DiagnosticMessage ReadyMethodNotGenerated = new("Default constructor or _Ready method is already declared",
        "_Ready method was not generated because a default constructor or a _Ready method is already declared and no custom setup method name was provided.\r\nFields setup was generated in OnReadySetup method.");
}
