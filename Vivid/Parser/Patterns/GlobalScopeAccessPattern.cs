using System.Collections.Generic;
using System.Linq;

public class GlobalScopeAccessPattern : Pattern
{
	public GlobalScopeAccessPattern() : base(TokenType.KEYWORD)
	{
		Priority = 19;
	}

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return tokens.First().To<KeywordToken>().Keyword == Keywords.GLOBAL;
	}

	public override Node Build(Context context, ParserState state, List<Token> tokens)
	{
		// Find the root context (global scope)
		while (context.Parent != null) { context = context.Parent; }

		// Return the context as a node
		return new ContextNode(context, tokens.First().Position);
	}
}