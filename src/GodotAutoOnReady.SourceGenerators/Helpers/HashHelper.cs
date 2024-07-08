using System.Security.Cryptography;
using System.Text;

namespace GodotAutoOnReady.SourceGenerators.Helpers;

internal static class HashHelper
{
    internal static string ComputeHash(string sourceData)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sourceData));

        var sb = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            sb.Append(bytes[i].ToString("x2"));
        }

        return sb.ToString();
    }
}
