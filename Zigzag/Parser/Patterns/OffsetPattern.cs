using System.Collections.Generic;

public class OffsetPattern : Pattern
{
	public const int PRIORITY = 18;

	private const int OBJECT = 0;
	private const int INDEX = 1;

	// ... [...]
	public OffsetPattern() : base
	(
		TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.DYNAMIC, TokenType.CONTENT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		var index = (ContentToken)tokens[INDEX];

		if (index.Type != ParenthesisType.BRACKETS)
		{
			return false;
		}

		return !index.IsEmpty;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		var source = Singleton.Parse(context, tokens[OBJECT]);
		var index = Singleton.Parse(context, tokens[INDEX]);

		return new OperatorNode(Operators.COLON).SetOperands(source, index);
	}
}