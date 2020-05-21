using System.Collections.Generic;

public class IfPattern : Pattern
{
	public const int PRIORITY = 1;

	public const int KEYWORD = 0;
	public const int CONDITION = 1;
	public const int BODY = 3;

	// if $bool [\n] {}
	public IfPattern() : base
	(
		TokenType.KEYWORD, TokenType.DYNAMIC, TokenType.END | TokenType.OPTIONAL, TokenType.CONTENT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		var keyword = tokens[KEYWORD].To<KeywordToken>();

		if (keyword.Keyword != Keywords.IF)
		{
			return false;
		}

		return tokens[BODY].To<ContentToken>().Type == ParenthesisType.CURLY_BRACKETS;
	}

	public override Node Build(Context environment, List<Token> tokens)
	{
		var condition = tokens[CONDITION].To<DynamicToken>();
		var body = tokens[BODY].To<ContentToken>();

		var context = new Context();
		context.Link(environment);

		return new IfNode(context, condition.Node, Parser.Parse(context, body.GetTokens(), 0, 20));
	}
}
