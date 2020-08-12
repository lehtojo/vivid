using System.Collections.Generic;

public class LoopControlPattern : Pattern
{
	private const int PRIORITY = 2;

	private const int INSTRUCTION = 0;

	// Examples: stop, continue
	public LoopControlPattern() : base
	(
		TokenType.KEYWORD
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		var instruction = tokens[INSTRUCTION].To<KeywordToken>();

		return instruction.Keyword == Keywords.STOP || instruction.Keyword == Keywords.CONTINUE;
	}

	public override Node? Build(Context context, List<Token> tokens)
	{
		return new LoopControlNode(tokens[INSTRUCTION].To<KeywordToken>().Keyword);
	}
}