using System.Collections.Generic;

public class ElsePattern : Pattern
{
	public const int PRIORITY = 15;

	private const int ELSE = 0;
	private const int BODY = 2;

	// Pattern:
	// else [\n] {...}
	public ElsePattern() : base(TokenType.KEYWORD, /* else */
								TokenType.END | TokenType.OPTIONAL, /* [\n] */
								TokenType.CONTENT)  /* {...} */
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(List<Token> tokens)
	{
		KeywordToken keyword = (KeywordToken)tokens[ELSE];

		if (keyword.Keyword != Keywords.ELSE)
		{
			return false;
		}

		ContentToken body = (ContentToken)tokens[BODY];
		return body.Type == ParenthesisType.CURLY_BRACKETS;
	}

	private List<Token> GetBody(List<Token> tokens)
	{
		ContentToken body = (ContentToken)tokens[BODY];
		return body.GetTokens();
	}

	public override Node Build(Context environment, List<Token> tokens)
	{
		Context context = new Context();
		context.Link(environment);

		List<Token> body = GetBody(tokens);
		Node node = Parser.Parse(context, body, Parser.MIN_PRIORITY, Parser.MEMBERS - 1);

		return new ElseNode(context, node);
	}
}