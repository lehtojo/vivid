using System.Collections.Generic;
using System.Linq;

public class ElsePattern : Pattern
{
	// Pattern: $if/$else-if [\n] else [\n] {...}/...
	public ElsePattern() : base
	(
		TokenType.KEYWORD,
		TokenType.END | TokenType.OPTIONAL
	)
	{ Priority = 1; IsConsumable = false; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		// Ensure there is an (else) if-statement before this else-statement
		if (state.Start == 0) return false;
		var token = state.All[state.Start - 1];

		// If the previous token represents an (else) if-statement, just continue
		if (token.Type != TokenType.DYNAMIC || !token.To<DynamicToken>().Node.Is(NodeType.IF, NodeType.ELSE_IF))
		{
			// The previous token must be a line ending in order for this pass function to succeed
			if (token.Type != TokenType.END || state.Start == 1) return false;

			// Now, the token before the line ending must be an (else) if-statement in order for this pass function to succeed
			token = state.All[state.Start - 2];
			if (token.Type != TokenType.DYNAMIC || !token.To<DynamicToken>().Node.Is(NodeType.IF, NodeType.ELSE_IF)) return false;
		}

		// Ensure the keyword is the else-keyword
		if (tokens.First().To<KeywordToken>().Keyword != Keywords.ELSE) return false;

		var next = state.Peek();
		if (next == null) return false;
		if (next.Is(ParenthesisType.CURLY_BRACKETS)) state.Consume();
		return true;
	}

	public override Node? Build(Context environment, ParserState state, List<Token> tokens)
	{
		var start = tokens.First().Position;
		var end = (Position?)null;

		var body = (List<Token>?)null;
		var last = tokens[tokens.Count - 1];

		var context = new Context(environment);

		if (last.Is(ParenthesisType.CURLY_BRACKETS))
		{
			body = last.To<ParenthesisToken>().Tokens;
			end = last.To<ParenthesisToken>().End;
		}
		else
		{
			body = new List<Token>();
			Common.ConsumeBlock(state, body);
		}

		var node = Parser.Parse(context, body, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY);

		return new ElseNode(context, node, start, end);
	}
}
