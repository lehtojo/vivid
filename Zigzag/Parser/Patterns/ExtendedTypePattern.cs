using System.Collections.Generic;

public class ExtendedTypePattern : Pattern
{
	public const int PRIORITY = 21;

	private const int MODIFIER = 0;
	private const int TYPE = 1;
	private const int NAME = 2;
	private const int EXTENDER = 3;
	private const int SUPERTYPES = 4;
	private const int BODY = 6;

	// Pattern:
	// [private / protected / public] type ... : ... / (...) [\n] {...}
	public ExtendedTypePattern() : base(TokenType.KEYWORD | TokenType.OPTIONAL, /* [private / protected / public] */
										TokenType.KEYWORD, /* type */
										TokenType.IDENTIFIER, /* ... */
										TokenType.OPERATOR, /* : */
										TokenType.IDENTIFIER | TokenType.CONTENT, /* ... / (...) */
										TokenType.END | TokenType.OPTIONAL, /* [\n] */
										TokenType.CONTENT) /* {..} */
	{}

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

		if (type.Keyword != Keywords.TYPE)
		{
			return false;
		}

		OperatorToken extender = (OperatorToken)tokens[EXTENDER];

		if (extender.Operator != Operators.EXTENDER)
		{
			return false;
		}

		Token token = tokens[SUPERTYPES];

		if (token.Type == TokenType.CONTENT)
		{
			ContentToken supertypes = (ContentToken)token;

			if (supertypes.Type != ParenthesisType.BRACKETS)
			{
				return false;
			}
		}

		ContentToken body = (ContentToken)tokens[BODY];
		return body.Type == ParenthesisType.CURLY_BRACKETS;
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

	private Type GetType(Context environment, Node node)
	{
		if (node is Contextable contextable)
		{
			Context context = contextable.GetContext();

			return (Type)context;
		}
		else if (node is Resolvable resolvable)
		{
			return new UnresolvedType(environment, resolvable);
		}

		return null;
	}

	private List<Type> GetSupertypes(Context environment, List<Token> tokens)
	{
		List<Type> types = new List<Type>();
		Node node = Singleton.Parse(environment, tokens[SUPERTYPES]);

		if (node.GetNodeType() == NodeType.CONTENT_NODE)
		{
			Node iterator = node.First;

			while (iterator != null)
			{
				Type type = GetType(environment, iterator);
				types.Add(type);

				iterator = iterator.Next;
			}
		}
		else
		{
			Type type = GetType(environment, node);
			types.Add(type);
		}

		return types;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		int modifiers = GetModifiers(tokens);

		// Get type name and its body
		string name = GetName(tokens);
		List<Token> body = GetBody(tokens);

		// Get all supertypes declared for this type
		List<Type> supertypes = GetSupertypes(context, tokens);

		// Create this type and parse its possible subtypes
		Type type = new Type(context, name, modifiers, supertypes);
		Parser.Parse(type, body, PRIORITY);

		return new TypeNode(type, body);
	}
}