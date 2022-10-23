using System.Collections.Generic;
using System.Linq;

class ReturnPattern : Pattern
{
	public ReturnPattern() : base
	(
		TokenType.KEYWORD
	)
	{ Priority = 0; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		if (!tokens.First().Is(Keywords.RETURN)) return false;

		state.Consume(TokenType.OBJECT); // Optionally consume a return value
		return true;
	}

	public override Node Build(Context context, ParserState state, List<Token> tokens)
	{
		var return_value = (Node?)null;

		if (tokens.Count > 1)
		{
			return_value = Singleton.Parse(context, tokens[1]);
		}

		return new ReturnNode(return_value, tokens.First().Position);
	}
}