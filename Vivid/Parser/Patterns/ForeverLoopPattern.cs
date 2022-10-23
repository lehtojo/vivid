using System.Collections.Generic;

public class ForeverLoopPattern : Pattern
{
	public const int KEYWORD = 0;
	public const int BODY = 2;

	// Pattern: loop [\n] {...}
	public ForeverLoopPattern() : base
	(
		TokenType.KEYWORD, TokenType.END | TokenType.OPTIONAL, TokenType.PARENTHESIS
	)
	{ Priority = 1; IsConsumable = false; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return tokens[KEYWORD].To<KeywordToken>().Keyword == Keywords.LOOP && tokens[BODY].Is(ParenthesisType.CURLY_BRACKETS);
	}

	public override Node Build(Context environment, ParserState state, List<Token> tokens)
	{
		var steps_context = new Context(environment);
		var body_context = new Context(steps_context);

		var body_token = tokens[BODY].To<ParenthesisToken>();
		var body = new ScopeNode(body_context, body_token.Position, body_token.End, false);

		Parser.Parse(body_context, body_token.Tokens, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY).ForEach(i => body.Add(i));

		return new LoopNode(steps_context, null, body, tokens[KEYWORD].Position);
	}
}