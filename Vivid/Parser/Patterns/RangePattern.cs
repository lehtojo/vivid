
using System.Collections.Generic;

public class RangePattern : Pattern
{
	public const int LEFT = 0;
	public const int OPERATOR = 2;
	public const int RIGHT = 4;

	// Pattern: $start [\n] .. [\n] $end
	public RangePattern() : base
	(
		TokenType.OBJECT,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.OPERATOR,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.OBJECT
	)
	{ Priority = 5; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return tokens[OPERATOR].Is(Operators.RANGE);
	}

	public override Node? Build(Context context, ParserState state, List<Token> tokens)
	{
		var left = Singleton.Parse(context, tokens[LEFT]);
		var right = Singleton.Parse(context, tokens[RIGHT]);

		return new UnresolvedFunction(Parser.STANDARD_RANGE_TYPE, tokens[OPERATOR].Position).SetArguments(new Node { left, right });
	}
}