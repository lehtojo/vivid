using System;
using System.Collections.Generic;
using System.Text;

public class IfPattern : Pattern
{
	public const int PRIORITY = 1;

	public const int CONDITION = 0;
	public const int BODY = 2;

	// $bool [\n] ()
	public IfPattern() : base
	(
		TokenType.DYNAMIC, TokenType.END | TokenType.OPTIONAL, TokenType.CONTENT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		var body = tokens[BODY] as ContentToken;
		return body.Type == ParenthesisType.CURLY_BRACKETS;
	}

	public override Node Build(Context environment, List<Token> tokens)
	{
		var condition = tokens[CONDITION] as DynamicToken;
		var body = tokens[BODY] as ContentToken;

		var context = new Context();
		context.Link(environment);

		return new IfNode(context, condition.Node, Parser.Parse(context, body.GetTokens(), 0, 20));
	}
}
