using System.Text;

namespace GodotSourceGenerators.SourceGenerators.Builders;

public static class StringBuilderExtensions
{
    public static StringBuilder Add(this StringBuilder sb, string content, int indentation = 0)
    {
        AddIndentation(sb, indentation);
        sb.Append(content);
        return sb;
    }

    public static StringBuilder AddLine(this StringBuilder sb, string content, int indentation = 0)
    {
        AddIndentation(sb, indentation);
        sb.AppendLine(content);
        return sb;
    }

    public static StringBuilder AddEmptyLine(this StringBuilder sb)
    {
        sb.AppendLine();
        return sb;
    }

    public static StringBuilder AddIndentation(this StringBuilder sb, int indentation)
    {
        for (int i = 0; i < indentation; i++)
        {
            sb.Append("\t");
        }

        return sb;
    }
}
