using System.Collections.Generic;

public class IfPattern : Pattern
{
	public const int PRIORITY = 15;

	private const int ELSE = 0;
	private const int IF = 1;
	private const int CONDITION = 2;
	private const int BODY = 4;

	// Pattern:
	// if (...) [\n] {...}
	public IfPattern() : base(TokenType.KEYWORD | TokenType.OPTIONAL, /* else */
							  TokenType.KEYWORD, /* if */
							  TokenType.DYNAMIC, /* (...) */
							  TokenType.END | TokenType.OPTIONAL, /* [\n] */
							  TokenType.CONTENT) /* {...} */
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(List<Token> tokens)
	{
		KeywordToken keyword = (KeywordToken)tokens[ELSE];

		if (keyword != null && keyword.Keyword != Keywords.ELSE)
		{
			return false;
		}

		keyword = (KeywordToken)tokens[IF];

		// Keyword at the start must be 'if' since this pattern represents if statement
		if (keyword.Keyword != Keywords.IF)
		{
			return false;
		}

		ContentToken body = (ContentToken)tokens[BODY];

		return body.Type == ParenthesisType.CURLY_BRACKETS;
	}

	/**
     * Returns the if statement's condition in node tree form
     * @param tokens If statement pattern represented in tokens
     * @return If statement's condition in node tree form
     */
	private Node GetCondition(List<Token> tokens)
	{
		DynamicToken token = (DynamicToken)tokens[CONDITION];
		ContentNode node = (ContentNode)token.Node;

		Node condition = node.First;

		if (condition == null)
		{
			throw Errors.Get(tokens[IF].Position, "Condition cannot be empty");
		}

		return condition;
	}

	/**
     * Tries to parse the body of the if statement
     * @param context Context to use while parsing
     * @param tokens If statement pattern represented in tokens
     * @return Parsed body in node tree form
     */
	private Node GetBody(Context context, List<Token> tokens)
	{
		ContentToken content = (ContentToken)tokens[BODY];
		return Parser.Parse(context, content.GetTokens(), Parser.MIN_PRIORITY, Parser.MEMBERS - 1);
	}

	/**
     * Returns whether this is a successor to a if statament
     * @return True if this is a successor to a if statement
     */
	private bool IsSuccessor(List<Token> tokens)
	{
		return tokens[ELSE] != null;
	}


	public override Node Build(Context environment, List<Token> tokens)
	{
		// Create new context for if statement's body, which is linked to its environment
		Context context = new Context();
		context.Link(environment);

		// Collect the components of this if statement
		Node condition = GetCondition(tokens);
		Node body = GetBody(context, tokens);

		// Build the components into a node
		if (IsSuccessor(tokens))
		{
			return new ElseIfNode(context, condition, body);
		}
		else
		{
			return new IfNode(context, condition, body);
		}
	}
}