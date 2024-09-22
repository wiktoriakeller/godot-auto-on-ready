namespace GodotAutoOnReady.SourceGenerator.Builders;

internal enum TokenType
{
    Method,
    Class,
    Namespace
}

internal readonly record struct BracketToken
{
    public readonly string Bracket;
    public readonly int Indentation;
    public readonly TokenType TokenType;

    public BracketToken(string bracket, int indentation, TokenType tokenType)
    {
        Bracket = bracket;
        Indentation = indentation;
        TokenType = tokenType;
    }
}
