using System.Collections.Generic;

public class OffsetPattern : Pattern
{
	public const int PRIORITY = 19;

	private const int OBJECT = 0;
	private const int INDEX = 1;

	// Function / Variable / (...) [Function / Variable / Number / (...)]
	public OffsetPattern() : base(TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.DYNAMIC, TokenType.CONTENT) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(List<Token> tokens)
	{
		ContentToken index = (ContentToken)tokens[INDEX];

		if (index.Type != ParenthesisType.BRACKETS)
		{
			return false;
		}

		return !index.IsEmpty;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		Node @object = Singleton.Parse(context, tokens[OBJECT]);
		Node index = Singleton.Parse(context, tokens[INDEX]);

		return new OperatorNode(Operators.EXTENDER).SetOperands(@object, index);
	}
}