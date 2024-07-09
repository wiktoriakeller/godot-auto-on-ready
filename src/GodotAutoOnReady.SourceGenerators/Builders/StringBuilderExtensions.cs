using System.Text;

namespace GodotAutoOnReady.SourceGenerators.Builders;

internal static class StringBuilderExtensions
{
    internal static StringBuilder Add(this StringBuilder sb, string content, int indentation = 0)
    {
        AddIndentation(sb, indentation);
        sb.Append(content);
        return sb;
    }

    internal static StringBuilder AddLine(this StringBuilder sb, string content, int indentation = 0)
    {
        AddIndentation(sb, indentation);
        sb.AppendLine(content);
        return sb;
    }

    internal static StringBuilder AddEmptyLine(this StringBuilder sb)
    {
        sb.AppendLine();
        return sb;
    }

    internal static StringBuilder AddIndentation(this StringBuilder sb, int indentation)
    {
        for (int i = 0; i < indentation; i++)
        {
            sb.Append("\t");
        }

        return sb;
    }
}
