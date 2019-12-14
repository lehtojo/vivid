using System;
using System.Collections.Generic;
using System.Text;

public class ElseIfPattern : Pattern
{
	public const int PRIORITY = 1;

	public const int IF = 0;
	public const int CONDITION = 2;
	public const int BODY = 4;

	// $if [\n] $bool [\n] (...) 
	public ElseIfPattern() : base
	(
		TokenType.DYNAMIC, TokenType.END | TokenType.OPTIONAL, TokenType.DYNAMIC, TokenType.END | TokenType.OPTIONAL, TokenType.CONTENT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		var previous = tokens[IF] as DynamicToken;
		
		if (previous.Node.GetNodeType() != NodeType.IF_NODE)
		{
			return false;
		}

		var condition = tokens[CONDITION] as DynamicToken;
		return condition.Node is IType type && type.GetType() == Types.BOOL;
	}

	public override Node Build(Context environment, List<Token> tokens)
	{
		var former = (tokens[IF] as DynamicToken).Node as IfNode;
		var condition = tokens[CONDITION] as DynamicToken;
		var body = tokens[BODY] as ContentToken;

		var context = new Context();
		context.Link(environment);

		var node = new ElseIfNode(context, condition.Node, Parser.Parse(context, body.GetTokens()));
		former.AddSuccessor(node);

		return null;
	}

	public override int GetStart()
	{
		return 1;
	}
}

// #error Tarkista, ettei tokenit korruptoidu ensimmäisen käytön jälkeen