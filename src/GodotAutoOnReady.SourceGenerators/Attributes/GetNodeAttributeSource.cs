﻿using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("GodotAutoOnReady.Tests")]
namespace GodotAutoOnReady.SourceGenerators.Attributes;

internal static class GetNodeAttributeSource
{
    internal const string Name = "GetNodeAttribute";

    internal const string Source = """
    // <auto-generated />
    #if NETCOREAPP3_0_OR_GREATER
    #nullable disable
    #endif
    using System;

    namespace GodotAutoOnReady.Attributes
    {
        /// <summary>
        /// Initializes a property or a field in the _Ready method or a custom setup method
        /// when _Ready can't be auto-generated.
        /// </summary>
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
        public sealed class GetNodeAttribute : Attribute
        {
            /// <summary>
            /// Path to the node, when the path is empty a property name is used instead.
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// Uses GetNodeOrNull<T> instead of GetNode<T> when assigning value to this member.
            /// </summary>
            public bool OrNull { get; set; } = false;

            public GetNodeAttribute(string path = "")
            {
                Path = path;
            }
        }
    }
    """;
}
