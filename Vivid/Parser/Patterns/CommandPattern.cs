using System.Collections.Generic;

public class CommandPattern : Pattern
{
	private const int KEYWORD = 0;

	// Pattern: stop/continue
	public CommandPattern() : base
	(
		TokenType.KEYWORD
	)
	{ Priority = 2; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		var keyword = tokens[KEYWORD].To<KeywordToken>().Keyword;
		return keyword == Keywords.STOP || keyword == Keywords.CONTINUE;
	}

	public override Node? Build(Context context, ParserState state, List<Token> tokens)
	{
		return new CommandNode(tokens[KEYWORD].To<KeywordToken>().Keyword, tokens[KEYWORD].Position);
	}
}