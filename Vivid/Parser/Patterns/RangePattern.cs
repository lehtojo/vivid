
using System.Collections.Generic;

public class RangePattern : Pattern
{
	public const int PRIORITY = 5;

	public const int LEFT = 0;
	public const int OPERATOR = 2;
	public const int RIGHT = 4;

	public const string RANGE_TYPE_NAME = "Range";

	// Pattern: $start [\n] .. [\n] $end
	public RangePattern() : base
	(
		TokenType.OBJECT,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.OPERATOR,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.OBJECT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return tokens[OPERATOR].Is(Operators.RANGE);
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		var left = Singleton.Parse(context, tokens[LEFT]);
		var right = Singleton.Parse(context, tokens[RIGHT]);

		return new UnresolvedFunction(RANGE_TYPE_NAME, tokens[OPERATOR].Position).SetParameters(new Node { left, right });
	}
}