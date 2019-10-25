using System.Collections.Generic;
using System;

public class OperatorPattern : Pattern
{
	private const int LEFT = 0;
	private const int OPERATOR = 2;
	private const int RIGHT = 4;

	// Pattern:
	// Function / Variable / Number / (...) [\n] Operator [\n] Function / Variable / Number / (...)
	public OperatorPattern() : base(TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.NUMBER | TokenType.STRING | TokenType.DYNAMIC, /* Function / Variable / Number / (...) */
			  TokenType.END | TokenType.OPTIONAL, /* [\n] */
			  TokenType.OPERATOR, /* Operator */
			  TokenType.END | TokenType.OPTIONAL, /* [\n] */
			  TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.NUMBER | TokenType.STRING | TokenType.DYNAMIC) /* Function / Variable / Number / (...) */
	{}

	public override int GetPriority(List<Token> tokens)
	{
		OperatorToken @operator = (OperatorToken)tokens[OPERATOR];
		return @operator.Operator.Priority;
	}


	public override bool Passes(List<Token> tokens)
	{
		return true;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		OperatorToken type = (OperatorToken)tokens[OPERATOR];
		OperatorNode @operator = new OperatorNode(type.Operator);

		Token left = tokens[LEFT];

		try
		{

			Node node = Singleton.Parse(context, left);
			@operator.Add(node);
		}
		catch (Exception exception)
		{
			throw Errors.Get(left.Position, exception);
		}

		Token right = tokens[RIGHT];

		try
		{
			Node node = Singleton.Parse(context, right);
			@operator.Add(node);
		}
		catch (Exception exception)
		{
			throw Errors.Get(right.Position, exception);
		}

		return @operator;
	}
}
