using System.Collections.Generic;
using System.Linq;

public class IfPattern : Pattern
{
	public const int PRIORITY = 1;

	public const int IF = 0;
	public const int CONDITION = 1;
	public const int BODY = 3;

	// Pattern: if $bool [\n] [{...}]
	public IfPattern() : base
	(
		TokenType.KEYWORD, TokenType.OBJECT, TokenType.END | TokenType.OPTIONAL
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		if (tokens[IF].To<KeywordToken>().Keyword != Keywords.IF) return false;

		Try(state, () => Consume(state, out Token? body, TokenType.PARENTHESIS) && body!.To<ParenthesisToken>().Opening == ParenthesisType.CURLY_BRACKETS);
		return true;
	}

	public override Node Build(Context environment, PatternState state, List<Token> tokens)
	{
		var condition = Singleton.Parse(environment, tokens[CONDITION]);
		var body = tokens.Last().Is(ParenthesisType.CURLY_BRACKETS) ? tokens.Last().To<ParenthesisToken>().Tokens : null;
		
		var start = tokens[IF].Position;
		var end = tokens.Last().Is(ParenthesisType.CURLY_BRACKETS) ? tokens.Last().To<ParenthesisToken>().End : null;

		var context = new Context(environment);

		if (body == null)
		{
			body = new List<Token>();
			if (!Common.ConsumeBlock(context, state, body)) throw Errors.Get(tokens[IF].Position, "If-statement has an empty body");
		}

		var node = Parser.Parse(context, body, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY);

		return new IfNode(context, condition, node, start, end);
	}
}