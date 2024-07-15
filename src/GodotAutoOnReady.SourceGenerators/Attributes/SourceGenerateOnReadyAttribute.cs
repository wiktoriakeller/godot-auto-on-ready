﻿namespace GodotAutoOnReady.SourceGenerators.Attributes;

internal static class SourceGenerateOnReadyAttribute
{
    internal const string AttributeName = "GenerateOnReadyAttribute";
    internal const string Attribute = """
    // <auto-generated />
    #if NETCOREAPP3_0_OR_GREATER
    #nullable disable
    #endif
    using System;

    namespace GodotAutoOnReady.Attributes
    {
        /// <summary>
        /// Every class that uses OnReady or OnReadyGet attributes has to be marked with GenerateOnReady,
        /// otherwise the source generator won't find it, and no source code will be generated.
        /// </summary>
        /// <remarks>
        /// When _Ready method is already declared or a default constructor exists, and a name for the custom init method is not provided,
        /// the default name OnReadyInit is used.
        /// </remarks>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        internal class GenerateOnReadyAttribute : Attribute
        {
            private readonly string _name;

            /// <param name="name">
            /// Name used to generate custom init method that can be called in _Ready.
            /// </param>
            internal GenerateOnReadyAttribute(string name)
            {
                _name = name;
            }
        }
    }
    """;
}
