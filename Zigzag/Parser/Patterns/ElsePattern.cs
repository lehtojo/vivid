using System;
using System.Collections.Generic;
using System.Text;

public class ElsePattern : Pattern
{
	public const int PRIORITY = 1;

	public const int FORMER = 0;
	public const int ELSE = 2;
	public const int BODY = 4;

	// $([else] if) [\n] else [\n] (...)
	public ElsePattern() : base
	(
		TokenType.DYNAMIC, 
		TokenType.END | TokenType.OPTIONAL, 
		TokenType.KEYWORD,
		TokenType.END | TokenType.OPTIONAL, 
		TokenType.CONTENT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		if (((KeywordToken)tokens[ELSE]).Keyword != Keywords.ELSE)
		{
			return false;
		}
		
		var former = (DynamicToken)tokens[FORMER];

		return former.Node.GetNodeType() == NodeType.IF_NODE ||
				former.Node.GetNodeType() == NodeType.ELSE_IF_NODE;
	}

	public override Node Build(Context environment, List<Token> tokens)
	{
		var former = (IfNode)((DynamicToken)tokens[FORMER]).Node;
		var body = (ContentToken)tokens[BODY];

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
