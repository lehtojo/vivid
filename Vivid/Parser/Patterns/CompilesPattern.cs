using System.Collections.Generic;

public class CompilesPattern : Pattern
{
    public const int PRIORITY = 5;

    private const int EXISTS = 0;
    private const int CONDITION = 2;

    public CompilesPattern() : base
    (
        TokenType.KEYWORD,
        TokenType.END | TokenType.OPTIONAL,
        TokenType.CONTENT
    ) {}

    public override int GetPriority(List<Token> tokens)
    {
        return PRIORITY;
    }

    public override bool Passes(Context context, PatternState state, List<Token> tokens)
    {
        return tokens[EXISTS].Is(Keywords.COMPILES) && tokens[CONDITION].Is(ParenthesisType.CURLY_BRACKETS);
    }

    public override Node? Build(Context context, List<Token> tokens)
    {
        var conditions = Singleton.Parse(context, tokens[CONDITION].To<ContentToken>());
        var result = new CompilesNode();

        conditions.ForEach(i => result.Add(i));

        return result;
    }
}