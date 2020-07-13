using System.Collections.Generic;

public class LinkPattern : Pattern
{
	public const int PRIORITY = 19;

	private const int LEFT = 0;
	private const int OPERATOR = 2;
	private const int RIGHT = 4;

	// ... [\n] . [\n] ...
	public LinkPattern() : base
	(
		TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.CONTENT | TokenType.DYNAMIC,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.OPERATOR,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.FUNCTION | TokenType.IDENTIFIER
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		var operation = (OperatorToken)tokens[OPERATOR];

		// The operator between left and right token must be dot
		if (operation.Operator != Operators.DOT)
		{
			return false;
		}

		// When left token is dynamic, it must be contextable
		if (tokens[LEFT].Type == TokenType.DYNAMIC)
		{
			var token = (DynamicToken)tokens[LEFT];
			return token.Node is IType;
		}

		return true;
	}

	public override Node Build(Context environment, List<Token> tokens)
	{
		var left = Singleton.Parse(environment, tokens[LEFT]);
		
		Node? right;

		if (left is IType type)
		{
			var primary = type.GetType();

			// Creates an unresolved node from right token if the primary context is unresolved
			if (primary == Types.UNKNOWN || primary is IResolvable)
			{
				right = Singleton.GetUnresolved(environment, tokens[RIGHT]);
			}
			else
			{
				right = Singleton.Parse(environment, primary, tokens[RIGHT]);
			}
		}
		else
		{
			right = Singleton.GetUnresolved(environment, tokens[RIGHT]);
		}

		return new LinkNode(left, right);
	}
}