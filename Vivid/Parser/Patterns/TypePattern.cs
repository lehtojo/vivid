using System.Collections.Generic;

public class TypePattern : Pattern
{
	public const int NAME = 0;
	public const int BODY = 2;

	// Pattern: $name [\n] {...}
	public TypePattern() : base
	(
		TokenType.IDENTIFIER, TokenType.END | TokenType.OPTIONAL, TokenType.PARENTHESIS
	)
	{ Priority = 22; IsConsumable = false; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return tokens[BODY].To<ParenthesisToken>().Opening == ParenthesisType.CURLY_BRACKETS;
	}

	public override Node Build(Context context, ParserState state, List<Token> tokens)
	{
		var name = tokens[NAME].To<IdentifierToken>();
		var body = tokens[BODY].To<ParenthesisToken>();

		var type = new Type(context, name.Value, Modifier.DEFAULT, name.Position);

		return new TypeDefinitionNode(type, body.Tokens, name.Position);
	}
}
