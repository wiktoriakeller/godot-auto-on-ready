﻿namespace GodotAutoOnReady.SourceGenerators.Attributes;

internal static class SourceGenerateReadyMethodAttribute
{
    internal const string Attribute = """
    // <auto-generated />
    #if FEATURE_NULLABLE
    #nullable disable
    #endif
    using System;

    namespace GodotAutoOnReady.SourceGenerators.Attributes
    {
        [AttributeUsage(AttributeTargets.Class)]
        internal class GenerateReadyMethodAttribute : Attribute
        {
            internal readonly string InitMethodName;

            internal GenerateReadyMethodAttribute(string initMethodName = "")
            {
                InitMethodName = initMethodName;
            }
        }
    }
    """;
}
