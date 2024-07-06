using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace GodotAutoOnReady.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifierSettings.OnFirstVerify(
            (receivedFile, receivedText, autoVerify) =>
            {
                Debug.WriteLine(receivedFile);
                Debug.WriteLine(receivedText);
                return Task.CompletedTask;
            });
        VerifierSettings.OnVerifyMismatch(
            (filePair, message, autoVerify) =>
            {
                Debug.WriteLine(filePair.ReceivedPath);
                Debug.WriteLine(filePair.VerifiedPath);
                Debug.WriteLine(message);
                return Task.CompletedTask;
            });
        //VerifierSettings.RegisterStringComparer("cs", (received, verified, dict) =>
        //{
        //    return Task.FromResult(new CompareResult(true));
        //});
        VerifySourceGenerators.Initialize();
    }
}
