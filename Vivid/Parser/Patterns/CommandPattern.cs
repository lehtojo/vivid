using System.Collections.Generic;

public class CommandPattern : Pattern
{
	private const int PRIORITY = 2;

	private const int KEYWORD = 0;

	// Pattern: stop/continue
	public CommandPattern() : base
	(
		TokenType.KEYWORD
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		var keyword = tokens[KEYWORD].To<KeywordToken>().Keyword;
		return keyword == Keywords.STOP || keyword == Keywords.CONTINUE;
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		return new LoopControlNode(tokens[KEYWORD].To<KeywordToken>().Keyword, tokens[KEYWORD].Position);
	}
}