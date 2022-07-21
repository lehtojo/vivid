using System.Collections.Generic;
using System.Linq;

class ReturnPattern : Pattern
{
	public const int PRIORITY = 0;

	public ReturnPattern() : base
	(
		TokenType.KEYWORD | TokenType.OPERATOR
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		if (!tokens.First().Is(Keywords.RETURN) && !tokens.First().Is(Operators.HEAVY_ARROW)) return false;

		Consume(state, TokenType.OBJECT); // Optionally consume a return value
		return true;
	}

	public override Node Build(Context context, PatternState state, List<Token> tokens)
	{
		var return_value = (Node?)null;

		if (tokens.Count > 1)
		{
			return_value = Singleton.Parse(context, tokens[1]);
		}

		return new ReturnNode(return_value, tokens.First().Position);
	}
}