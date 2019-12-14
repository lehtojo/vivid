using System.Collections.Generic;

public class CastPattern : Pattern
{
	public const int PRIORITY = 19;

	private const int OBJECT = 0;
	private const int CAST = 1;
	private const int TYPE = 2;

	// ... -> Type
	public CastPattern() : base
	(
		TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.NUMBER | TokenType.DYNAMIC,
		TokenType.OPERATOR,
		TokenType.IDENTIFIER | TokenType.DYNAMIC
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		var cast = tokens[CAST] as OperatorToken;
		return cast.Operator == Operators.CAST;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		var source = Singleton.Parse(context, tokens[OBJECT]);
		var type = Singleton.Parse(context, tokens[TYPE]);

		return new CastNode(source, type);
	}
}