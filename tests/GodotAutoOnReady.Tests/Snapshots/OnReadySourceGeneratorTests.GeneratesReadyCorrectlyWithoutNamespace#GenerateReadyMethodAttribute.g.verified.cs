﻿//HintName: GenerateReadyMethodAttribute.g.cs
// <auto-generated />
#if NETCOREAPP3_0_OR_GREATER
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