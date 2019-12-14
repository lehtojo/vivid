using System;
using System.Collections.Generic;
using System.Text;

class IncrementPattern : Pattern
{
	public const int PRIORITY = 18;

	public const int OPERATOR = 0;
	public const int OBJECT = 1;

	// ++ a-z
	public IncrementPattern() : base
	(
		TokenType.OPERATOR, TokenType.DYNAMIC | TokenType.IDENTIFIER 
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		var destination = tokens[OBJECT];

		if (destination is DynamicToken dynamic && dynamic.Node.GetNodeType() != NodeType.LINK_NODE)
		{
			return false;
		}

		var @operator = tokens[OPERATOR] as OperatorToken;
		return @operator.Operator == Operators.INCREMENT;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		return new IncrementNode(Singleton.Parse(context, tokens[OBJECT]));
	}
}