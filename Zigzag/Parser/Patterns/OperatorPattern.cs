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
		  TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.NUMBER | TokenType.STRING | TokenType.DYNAMIC,
		  TokenType.END | TokenType.OPTIONAL,
		  TokenType.OPERATOR,
		  TokenType.END | TokenType.OPTIONAL,
		  TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.NUMBER | TokenType.STRING | TokenType.DYNAMIC
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
		var type = (OperatorToken)tokens[OPERATOR];
		var @operator = new OperatorNode(type.Operator);

		var left = tokens[LEFT];

		try
		{
			var node = Singleton.Parse(context, left);
			@operator.Add(node);
		}
		catch (Exception exception)
		{
			throw Errors.Get(left.Position, exception);
		}

		var right = tokens[RIGHT];

		try
		{
			var node = Singleton.Parse(context, right);
			@operator.Add(node);
		}
		catch (Exception exception)
		{
			throw Errors.Get(right.Position, exception);
		}

		return @operator;
	}
}