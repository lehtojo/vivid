using System.Collections.Generic;

public class ListConstructionPattern : Pattern
{
	private const int LIST = 0;

	// Pattern: [ $element-1, $element-2, ... ]

	public ListConstructionPattern() : base
	(
		TokenType.PARENTHESIS
	)
	{ Priority = 2; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return tokens[LIST].To<ParenthesisToken>().Opening == ParenthesisType.BRACKETS;
	}

	public override Node Build(Context context, ParserState state, List<Token> tokens)
	{
		var elements = Singleton.Parse(context, tokens[LIST]);
		var position = tokens[LIST].Position;

		return new ListConstructionNode(elements, position);
	}
}