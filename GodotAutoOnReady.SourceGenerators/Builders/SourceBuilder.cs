using System.Text;

namespace GodotAutoOnReady.SourceGenerators.Builders;

public class SourceBuilder
{
    private StringBuilder _sb = new();
    private Stack<BracketToken> _tokens = [];
    private int _maxIndentation = 0;

    public SourceBuilder AddNamespace(string namespcaeName)
    {
        _sb.AddLine($"namespace {namespcaeName}")
           .AddLine("{");

        _tokens.Push(new BracketToken("}", _maxIndentation++, TokenType.Namespace));
        return this;
    }

    public SourceBuilder AddClass(string modifiers, string className, string baseClass = "")
    {
        _sb.Add($"{modifiers} {className}", _maxIndentation)
           .Add(baseClass == "" ? "\n" : $" : {baseClass}\n")
           .AddLine("{", _maxIndentation);

        _tokens.Push(new BracketToken("}", _maxIndentation++, TokenType.Class));
        return this;
    }

    public SourceBuilder AddMethod(string modifiers, string methodName)
    {
        CheckShouldAddClosingBracket();

        _sb.AddLine($"{modifiers} {methodName}()", _maxIndentation)
           .AddLine("{", _maxIndentation);

        _tokens.Push(new BracketToken("}", _maxIndentation, TokenType.Method));
        return this;
    }

    public SourceBuilder AddMethodContent(string content = "")
    {
        _sb.AddLine(content, _maxIndentation + 1);
        return this;
    }

    public SourceBuilder AddLine(string line = "")
    {
        _sb.AddLine(line, _maxIndentation);
        return this;
    }

    public void Reset()
    {
        _sb = new();
        _tokens = [];
        _maxIndentation = 0;
    }

    public string BuildSource()
    {
        while (_tokens.Count > 0)
        {
            var token = _tokens.Pop();
            _sb.AddIndentation(token.Indentation)
               .AddLine(token.Bracket);
        }

        return _sb.ToString();
    }

    private void CheckShouldAddClosingBracket()
    {
        if (_tokens.Peek().TokenType == TokenType.Method)
        {
            var token = _tokens.Pop();

            _sb.AddLine(token.Bracket, token.Indentation)
               .AddEmptyLine();
        }
    }
}
