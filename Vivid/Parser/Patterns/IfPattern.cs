using System.Collections.Generic;
using System.Linq;

public class IfPattern : Pattern
{
	public const int KEYWORD = 0;
	public const int CONDITION = 1;
	public const int BODY = 3;

	// Pattern: if $condition [\n] {...}/...
	public IfPattern() : base
	(
		TokenType.KEYWORD, TokenType.OBJECT, TokenType.END | TokenType.OPTIONAL
	)
	{ Priority = 1; IsConsumable = false; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		var keyword = tokens[KEYWORD].To<KeywordToken>().Keyword;
		if (keyword != Keywords.IF && keyword != Keywords.ELSE) return false;

		// Prevents else-if from thinking that a body is a condition
		if (tokens[CONDITION].Is(ParenthesisType.CURLY_BRACKETS)) return false;

		// Try to consume curly brackets
		var next = state.Peek();
		if (next == null) return false;
		if (next.Is(ParenthesisType.CURLY_BRACKETS)) state.Consume();

		return true;
	}

	public override Node Build(Context environment, ParserState state, List<Token> tokens)
	{
		var condition = Singleton.Parse(environment, tokens[CONDITION]);
		var start = tokens[KEYWORD].Position;
		var end = (Position?)null;

		var body = (List<Token>?)null;
		var last = tokens.Last();

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

		if (tokens[KEYWORD].To<KeywordToken>().Keyword == Keywords.IF) return new IfNode(context, condition, node, start, end);
		return new ElseIfNode(context, condition, node, start, end);
	}
}