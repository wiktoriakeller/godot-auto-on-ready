using System.Runtime.CompilerServices;

namespace GodotAutoOnReady.Tests.Helpers;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();
    }
}
