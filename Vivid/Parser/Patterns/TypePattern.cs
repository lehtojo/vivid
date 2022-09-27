using System.Collections.Generic;

public class TypePattern : Pattern
{
	public const int PRIORITY = 22;

	public const int NAME = 0;
	public const int BODY = 2;

	// Pattern: $name [\n] {...}
	public TypePattern() : base
	(
		TokenType.IDENTIFIER, TokenType.END | TokenType.OPTIONAL, TokenType.PARENTHESIS
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return tokens[BODY].To<ParenthesisToken>().Opening == ParenthesisType.CURLY_BRACKETS;
	}

	public override Node Build(Context context, PatternState state, List<Token> tokens)
	{
		var name = tokens[NAME].To<IdentifierToken>();
		var body = tokens[BODY].To<ParenthesisToken>();

		var type = new Type(context, name.Value, Modifier.DEFAULT, name.Position);

		return new TypeNode(type, body.Tokens, name.Position);
	}
}
