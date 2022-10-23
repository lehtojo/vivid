using System.Collections.Generic;

public class ListPattern : Pattern
{
	public const int ID = 1;

	public const int LEFT = 0;
	public const int COMMA = 2;
	public const int RIGHT = 4;

	// Pattern: ... , ...
	public ListPattern() : base
	(
		TokenType.OBJECT,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.OPERATOR,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.OBJECT
	)
	{ Priority = 0; Id = ID; IsConsumable = false; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return tokens[COMMA].To<OperatorToken>().Operator == Operators.COMMA;
	}

	public override Node? Build(Context context, ParserState state, List<Token> tokens)
	{
		var left = tokens[LEFT];
		var right = tokens[RIGHT];

		// If the left token represents a list node, add the right operand to it and return the list
		if (left.Is(TokenType.DYNAMIC))
		{
			var node = left.To<DynamicToken>().Node;

			if (node.Is(NodeType.LIST))
			{
				node.Add(Singleton.Parse(context, right));
				return node;
			}
		}

		return new ListNode(tokens[COMMA].Position, Singleton.Parse(context, left), Singleton.Parse(context, right));
	}
}