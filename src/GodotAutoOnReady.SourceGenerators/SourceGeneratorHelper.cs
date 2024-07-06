using System.Text;

namespace GodotAutoOnReady.SourceGenerators;

public static class SourceGeneratorHelper
{
    public static string GenerateRandomMethodName(int length, string postfix)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var sb = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            sb.Append(chars[random.Next(chars.Length)]);
        }

        return sb.Append(postfix).ToString();
    }
}
