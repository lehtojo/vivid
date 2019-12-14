using System;
using System.Collections.Generic;
using System.Text;

public class TypePattern : Pattern
{
	public const int PRIORITY = 21;

	public const int NAME = 0;
	public const int BODY = 2;

	// a-z [\n] (...)
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
		var body = tokens[BODY] as ContentToken;
		return body.Type != ParenthesisType.BRACKETS;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		var name = tokens[NAME] as IdentifierToken;
		var body = tokens[BODY] as ContentToken;

		var type = new Type(context, name.Value, AccessModifier.PUBLIC);

		return new TypeNode(type, body.GetTokens());
	}
}
