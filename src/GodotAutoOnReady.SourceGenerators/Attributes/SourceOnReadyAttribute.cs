﻿namespace GodotAutoOnReady.SourceGenerators.Attributes;

internal static class SourceOnReadyAttribute
{
    internal const string Attribute = """
    // <auto-generated />
    #if FEATURE_NULLABLE
    #nullable disable
    #endif
    using System;

    namespace GodotAutoOnReady.SourceGenerators.Attributes
    {
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
        internal class OnReadyAttribute : Attribute
        {
            internal readonly string Path;
            internal readonly bool IsSceneUnique;

            internal OnReadyAttribute(string path, bool isSceneUnique = false)
            {
                Path = path;
                IsSceneUnique = isSceneUnique;
            }
        }
    }
    """;
}
