using System.Collections.Generic;
public class ConstructorPattern : Pattern
{
	public const int PRIORITY = 20;

	private const int MODIFIER = 0;
	private const int INIT = 1;
	private const int PARAMETERS = 2;

	private const int BODY = 4;

	// Pattern:
	// [private / protected / public] init (...) [\n] {...}
	public ConstructorPattern() : base(TokenType.KEYWORD | TokenType.OPTIONAL, /* [private / protected / public] */
									   TokenType.KEYWORD, /* init */
									   TokenType.CONTENT | TokenType.OPTIONAL, /* (...) */
									   TokenType.END | TokenType.OPTIONAL, /* [\n] */
									   TokenType.CONTENT) /*  {...} */
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	private KeywordToken GetInitializeKeyword(List<Token> tokens)
	{
		return (KeywordToken)tokens[INIT];
	}

	private ContentToken GetParameters(List<Token> tokens)
	{
		return (ContentToken)tokens[PARAMETERS];
	}

	private ContentToken GetBody(List<Token> tokens)
	{
		return (ContentToken)tokens[BODY];
	}

	public override bool Passes(List<Token> tokens)
	{
		KeywordToken modifier = (KeywordToken)tokens[MODIFIER];

		if (modifier != null && modifier.Keyword.Type != KeywordType.ACCESS_MODIFIER)
		{
			return false;
		}

		KeywordToken init = GetInitializeKeyword(tokens);

		if (init.Keyword != Keywords.INIT)
		{
			return false;
		}

		ContentToken parameters = GetParameters(tokens);

		if (parameters != null && parameters.Type != ParenthesisType.PARENTHESIS)
		{
			return false;
		}

		ContentToken body = GetBody(tokens);

		return body.Type == ParenthesisType.CURLY_BRACKETS;
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

		ContentToken parameters = GetParameters(tokens);
		List<Token> body = GetBody(tokens).GetTokens();

		if (!context.IsType)
		{
			throw Errors.Get(tokens[INIT].Position, "Constructor must be inside of a type");
		}

		Type type = (Type)context;

		Constructor constructor = new Constructor(context, modifiers);

		if (parameters != null)
		{
			constructor.SetParameters(Singleton.GetContent(constructor, parameters));
		}

		type.AddConstructor(constructor);

		return new FunctionNode(constructor, body);
	}
}