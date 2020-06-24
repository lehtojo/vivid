using System.Collections.Generic;

public class OffsetPattern : Pattern
{
	private const int PRIORITY = 18;

	private const int OBJECT = 0;
	private const int INDICES = 1;

	// ... [...]
	public OffsetPattern() : base
	(
		TokenType.OBJECT, TokenType.CONTENT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		var index = tokens[INDICES].To<ContentToken>();

		if (!Equals(index.Type, ParenthesisType.BRACKETS))
		{
			return false;
		}

		return !index.IsEmpty;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		var source = Singleton.Parse(context, tokens[OBJECT]);
		var indices = Singleton.Parse(context, tokens[INDICES]);

		return new OperatorNode(Operators.COLON).SetOperands(source, indices);
	}
}