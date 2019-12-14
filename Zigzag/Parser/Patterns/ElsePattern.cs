using System;
using System.Collections.Generic;
using System.Text;

public class ElsePattern : Pattern
{
	public const int PRIORITY = 1;

	public const int FORMER = 0;
	public const int BODY = 2;

	// $([else] if) [\n] (...)
	public ElsePattern() : base
	(
		TokenType.DYNAMIC, TokenType.END | TokenType.OPTIONAL, TokenType.CONTENT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		var former = tokens[FORMER] as DynamicToken;

		return former.Node.GetNodeType() == NodeType.IF_NODE ||
				former.Node.GetNodeType() == NodeType.ELSE_IF_NODE;
	}

	public override Node Build(Context environment, List<Token> tokens)
	{
		var former = (tokens[FORMER] as DynamicToken).Node as IfNode;
		var body = tokens[BODY] as ContentToken;

		var context = new Context();
		context.Link(environment);

		var node = new ElseNode(context, Parser.Parse(context, body.GetTokens()));
		former.AddSuccessor(node);

		return null;
	}

	public override int GetStart()
	{
		return 1;
	}
}
