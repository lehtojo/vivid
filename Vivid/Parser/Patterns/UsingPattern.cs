using System.Collections.Generic;

public class UsingPattern : Pattern
{
	public UsingPattern() : base
	(
		TokenType.ANY, TokenType.IDENTIFIER, TokenType.ANY
	)
	{ Priority = 5; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return tokens[1].To<IdentifierToken>().Value == Keywords.USING.Identifier;
	}

	public override Node? Build(Context context, ParserState state, List<Token> tokens)
	{
		var allocated = Singleton.Parse(context, tokens[0]);
		var allocator = Singleton.Parse(context, tokens[2]);
		return new UsingNode(allocated, allocator, tokens[1].Position);
	}
}