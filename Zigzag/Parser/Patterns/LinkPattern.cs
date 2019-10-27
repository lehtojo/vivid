using System.Collections.Generic;
public class LinkPattern : Pattern
{
	public const int PRIORITY = 19;

	private const int LEFT = 0;
	private const int OPERATOR = 2;
	private const int RIGHT = 4;

	// Pattern:
	// ... [\n] . [\n] ...
	public LinkPattern() : base(TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.DYNAMIC,  /* ... */
								TokenType.END | TokenType.OPTIONAL, /* [\n] */
								TokenType.OPERATOR, /* . */
								TokenType.END | TokenType.OPTIONAL, /* [\n] */
								TokenType.FUNCTION | TokenType.IDENTIFIER) /* ... */
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(List<Token> tokens)
	{
		OperatorToken @operator = (OperatorToken)tokens[OPERATOR];

		// The operator between left and right token must be dot
		if (@operator.Operator != Operators.DOT)
		{
			return false;
		}

		// When left token is a processed, it must be contextable
		if (tokens[LEFT].Type == TokenType.DYNAMIC)
		{
			DynamicToken token = (DynamicToken)tokens[LEFT];
			return (token.Node is Contextable);
		}

		return true;
	}

	public override Node Build(Context environment, List<Token> tokens)
	{
		Node left = Singleton.Parse(environment, tokens[LEFT]);
		Node right;

		if (left is Contextable contextable)
		{
			Context primary = contextable.GetContext();

			// Creates an unresolved node from right token if the primary context is unresolved
			if (primary == null || primary is IResolvable)
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