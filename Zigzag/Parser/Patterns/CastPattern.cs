using System.Collections.Generic;
public class CastPattern : Pattern
{
	public const int PRIORITY = 19;

	private const int OBJECT = 0;
	private const int CAST = 1;
	private const int TYPE = 2;

	// Pattern:
	// ... -> Type / Type.Subtype
	public CastPattern() : base(TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.NUMBER | TokenType.DYNAMIC, /* ... */
								TokenType.OPERATOR, /* -> */
								TokenType.IDENTIFIER | TokenType.DYNAMIC) /* Type / Type.Subtype */
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(List<Token> tokens)
	{
		OperatorToken cast = (OperatorToken)tokens[CAST];
		return cast.Operator == Operators.CAST;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		Node @object = Singleton.Parse(context, tokens[OBJECT]);
		Node type = Singleton.Parse(context, tokens[TYPE]);

		return new CastNode(@object, type);
	}
}