using System.Collections.Generic;

public class CommandPattern : Pattern
{
	private const int PRIORITY = 2;

	private const int INSTRUCTION = 0;

	// Examples: stop, continue
	public CommandPattern() : base
	(
		TokenType.KEYWORD
	)
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		var instruction = tokens[INSTRUCTION].To<KeywordToken>();

		return instruction.Keyword == Keywords.STOP || instruction.Keyword == Keywords.CONTINUE || instruction.Keyword == Keywords.RETURN;
	}

	public override Node? Build(Context context, List<Token> tokens)
	{
		var keyword = tokens[INSTRUCTION].To<KeywordToken>().Keyword;

		if (keyword == Keywords.RETURN)
		{
			return new ReturnNode(null, tokens[INSTRUCTION].Position);
		}

		return new LoopControlNode(keyword, tokens[INSTRUCTION].Position);
	}
}