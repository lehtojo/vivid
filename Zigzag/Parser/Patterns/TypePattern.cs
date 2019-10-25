using System.Collections.Generic;

public class TypePattern : Pattern
{
	public const int PRIORITY = 21;

	private const int MODIFIER = 0;
	private const int TYPE = 1;
	private const int NAME = 2;
	private const int BODY = 4;

	// [private / protected / public] type ... [\n] {...}
	public TypePattern() : base(TokenType.KEYWORD | TokenType.OPTIONAL, /* [private / protected / public] */
								TokenType.KEYWORD, /* type */
								TokenType.IDENTIFIER, /* ... */
								TokenType.END | TokenType.OPTIONAL, /* [\n] */
								TokenType.CONTENT) /* {...} */ {}

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(List<Token> tokens)
	{
		KeywordToken modifier = (KeywordToken)tokens[MODIFIER];

		if (modifier != null && modifier.Keyword.Type != KeywordType.ACCESS_MODIFIER)
		{
			return false;
		}

		KeywordToken type = (KeywordToken)tokens[TYPE];
		return type.Keyword == Keywords.TYPE;
	}

	private string GetName(List<Token> tokens)
	{
		return ((IdentifierToken)tokens[NAME]).Value;
	}

	private List<Token> GetBody(List<Token> tokens)
	{
		return ((ContentToken)tokens[BODY]).GetTokens();
	}

	private int GetModifiers(List<Token> tokens)
	{
		int modifiers = AccessModifier.PUBLIC;

		KeywordToken token = (KeywordToken)tokens[MODIFIER];

		if (token != null)
		{
			AccessModifierKeyword modifier = (AccessModifierKeyword)token.Keyword;
			modifiers = modifier.Modifier;
		}

		return modifiers;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		int modifiers = GetModifiers(tokens);

		// Get type name and its body
		string name = GetName(tokens);
		List<Token> body = GetBody(tokens);

		// Create this type and parse its possible subtypes
		Type type = new Type(context, name, modifiers);
		Parser.Parse(type, body, PRIORITY);

		return new TypeNode(type, body);
	}
}
