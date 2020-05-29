using System.Collections.Generic;

public class TypePattern : Pattern
{
	public const int PRIORITY = 22;

	public const int NAME = 0;
	public const int BODY = 2;

	// a-z [\n] {...}
	public TypePattern() : base
	(
		TokenType.IDENTIFIER, TokenType.END | TokenType.OPTIONAL, TokenType.CONTENT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		return tokens[BODY].To<ContentToken>().Type == ParenthesisType.CURLY_BRACKETS;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		var name = tokens[NAME].To<IdentifierToken>();
		var body = tokens[BODY].To<ContentToken>();

		var type = new Type(context, name.Value, AccessModifier.PUBLIC);

		return new TypeNode(type, body.GetTokens());
	}
}
