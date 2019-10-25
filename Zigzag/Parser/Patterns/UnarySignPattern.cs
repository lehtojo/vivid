using System.Collections.Generic;

public class UnarySignPattern : Pattern
{
	private const int PRIORITY = 14;

	private const int OPERATOR = 0;
	private const int SIGN = 1;
	private const int OBJECT = 2;

	public UnarySignPattern() : base(TokenType.OPERATOR, TokenType.OPERATOR,
									 TokenType.IDENTIFIER | TokenType.NUMBER | TokenType.DYNAMIC)
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(List<Token> tokens)
	{
		OperatorToken @operator = (OperatorToken)tokens[OPERATOR];
		OperatorToken sign = (OperatorToken)tokens[SIGN];

		return @operator.Operator != Operators.INCREMENT && @operator.Operator != Operators.DECREMENT &&
				(sign.Operator == Operators.ADD || sign.Operator == Operators.SUBTRACT);
	}

	private bool isNegative(OperatorToken sign)
	{
		return sign.Operator == Operators.SUBTRACT;
	}

	private Node getNegativeNode(Node node)
	{
		if (node.GetNodeType() == NodeType.NUMBER_NODE)
		{
			NumberNode number = (NumberNode)node;
			number.Value = -(long)number.Value;

			return number;
		}

		return new NegateNode(node);
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		OperatorToken sign = (OperatorToken)tokens[SIGN];
		Node node = Singleton.Parse(context, tokens[OBJECT]);

		if (isNegative(sign))
		{
			return getNegativeNode(node);
		}

		return node;
	}
	
	public override int GetStart()
	{
		return SIGN;
	}
}
