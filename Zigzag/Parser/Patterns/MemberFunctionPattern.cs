using System.Collections.Generic;
public class MemberFunctionPattern : Pattern
{
	public const int PRIORITY = 20;

	private const int MODIFIER = 0;
	private const int RETURN_TYPE = 2;
	private const int HEAD = 3;
	private const int BODY = 5;

	// Pattern:
	// [private / protected / public] [static] Type / Type.Subtype / func ... (...) [\n] {...}
	public MemberFunctionPattern() : base(TokenType.KEYWORD | TokenType.OPTIONAL, /* [private / protected / public] */
										  TokenType.KEYWORD | TokenType.OPTIONAL, /* [static] */
										  TokenType.KEYWORD | TokenType.IDENTIFIER | TokenType.DYNAMIC, /* Type / Type.Subtype / func */
										  TokenType.FUNCTION | TokenType.IDENTIFIER, /* ... [(...)] */
										  TokenType.END | TokenType.OPTIONAL, /* [\n] */
										  TokenType.CONTENT) /* {...} */
	{ }


	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(List<Token> tokens)
	{
		KeywordToken first = (KeywordToken)tokens[MODIFIER];
		KeywordToken second = (KeywordToken)tokens[MODIFIER + 1];

		if ((first != null && first.Keyword.Type != KeywordType.ACCESS_MODIFIER) ||
			(second != null && second.Keyword.Type != KeywordType.ACCESS_MODIFIER))
		{
			return false;
		}

		Token token = tokens[RETURN_TYPE];

		switch (token.Type)
		{

			case TokenType.KEYWORD:
			{
				return ((KeywordToken)token).Keyword == Keywords.FUNC;
			}

			case TokenType.IDENTIFIER:
			{
				return true;
			}

			case TokenType.DYNAMIC:
			{
				Node node = ((DynamicToken)token).Node;
				return node.GetNodeType() == NodeType.TYPE_NODE || node.GetNodeType() == NodeType.LINK_NODE;
			}
		}

		ContentToken body = (ContentToken)tokens[BODY];
		return body.Type == ParenthesisType.CURLY_BRACKETS;
	}

	private int GetModifiers(List<Token> tokens)
	{
		KeywordToken first = (KeywordToken)tokens[MODIFIER];
		KeywordToken second = (KeywordToken)tokens[MODIFIER + 1];

		int modifiers = AccessModifier.PUBLIC;

		if (first != null)
		{
			KeywordToken keyword = (KeywordToken)first;
			AccessModifierKeyword modifier = (AccessModifierKeyword)keyword.Keyword;
			modifiers |= modifier.Modifier;

			if (second != null)
			{
				keyword = (KeywordToken)second;
				modifier = (AccessModifierKeyword)keyword.Keyword;
				modifiers |= modifier.Modifier;
			}
		}

		return modifiers;
	}

	private List<Token> GetBody(List<Token> tokens)
	{
		return ((ContentToken)tokens[BODY]).GetTokens();
	}

	private Type GetReturnType(Context context, List<Token> tokens)
	{
		Token token = tokens[RETURN_TYPE];

		switch (token.Type)
		{

			case TokenType.KEYWORD:
			{
				return Types.UNKNOWN;
			}

			case TokenType.IDENTIFIER:
			{
				IdentifierToken id = (IdentifierToken)token;

				if (context.IsTypeDeclared(id.Value))
				{
					return context.GetType(id.Value);
				}
				else
				{
					return new UnresolvedType(context, id.Value);
				}
			}

			case TokenType.DYNAMIC:
			{
				DynamicToken dynamic = (DynamicToken)token;
				Node node = dynamic.Node;

				if (node.GetNodeType() == NodeType.TYPE_NODE)
				{
					TypeNode type = (TypeNode)node;
					return type.Type;
				}
				else if (node is IResolvable resolvable)
				{
					return new UnresolvedType(context, resolvable);
				}

				break;
			}
		}

		throw Errors.Get(tokens[HEAD].Position, "Couldn't resolve return type");
	}


	public override Node Build(Context context, List<Token> tokens)
	{
		Function function;

		int modifiers = GetModifiers(tokens);
		Type result = GetReturnType(context, tokens);
		List<Token> body = GetBody(tokens);

		Token token = tokens[HEAD];

		if (token.Type == TokenType.FUNCTION)
		{
			FunctionToken head = (FunctionToken)token;
			function = new Function(context, head.Name, modifiers, result);
			function.SetParameters(head.GetParsedParameters(function));
		}
		else
		{
			IdentifierToken name = (IdentifierToken)token;
			function = new Function(context, name.Value, modifiers, result);
		}

		return new FunctionNode(function, body);
	}
}
