using System.Collections.Generic;

public class ListConstructionPattern : Pattern
{
	private const int PRIORITY = 2;

	private const int LIST = 0;

	// Pattern: [ $element-1, $element-2, ... ]

	public ListConstructionPattern() : base
	(
		TokenType.PARENTHESIS
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return tokens[LIST].To<ParenthesisToken>().Opening == ParenthesisType.BRACKETS;
	}

	public override Node Build(Context context, PatternState state, List<Token> tokens)
	{
		var elements = Singleton.Parse(context, tokens[LIST]);
		var position = tokens[LIST].Position;

		return new ListConstructionNode(elements, position);
	}
}