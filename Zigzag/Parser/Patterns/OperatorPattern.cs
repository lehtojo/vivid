using System;
using System.Collections.Generic;

public class OperatorPattern : Pattern
{
	private const int LEFT = 0;
	private const int OPERATOR = 2;
	private const int RIGHT = 4;

	// Pattern:
	// ... [\n] Operator [\n] ...
	public OperatorPattern() : base
	(
		  TokenType.OBJECT,
		  TokenType.END | TokenType.OPTIONAL,
		  TokenType.OPERATOR,
		  TokenType.END | TokenType.OPTIONAL,
		  TokenType.OBJECT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		OperatorToken @operator = (OperatorToken)tokens[OPERATOR];
		return @operator.Operator.Priority;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		return true;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		var operation = (OperatorToken)tokens[OPERATOR];

		var left = tokens[LEFT];
		var right = tokens[RIGHT];

		var node = new OperatorNode(operation.Operator);

		try
		{
			node.Add(Singleton.Parse(context, left));
		}
		catch (Exception exception)
		{
			throw Errors.Get(left.Position, exception);
		}

		try
		{
			node.Add(Singleton.Parse(context, right));
		}
		catch (Exception exception)
		{
			throw Errors.Get(right.Position, exception);
		}

		return node;
	}
}