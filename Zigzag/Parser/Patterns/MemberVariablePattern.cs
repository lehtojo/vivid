using System;
using System.Collections.Generic;

public class MemberVariablePattern : Pattern
{
	public const int PRIORITY = 20;

	private const int MODIFIER = 0;
	private const int TYPE = 2;
	private const int NAME = 3;

	// Pattern:
	// [private / protected / public] [static] Type / Type.Subtype ...
	public MemberVariablePattern() : base(TokenType.KEYWORD | TokenType.OPTIONAL, /* [private / protected / public] */
										  TokenType.KEYWORD | TokenType.OPTIONAL, /* [static] */
										  TokenType.IDENTIFIER | TokenType.DYNAMIC, /* Type / Type.Subtype */
										  TokenType.IDENTIFIER) /* ... */
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

		Token token = tokens[TYPE];

		if (token.Type == TokenType.DYNAMIC)
		{
			Node node = ((DynamicToken)token).Node;
			return node.GetNodeType() == NodeType.TYPE_NODE || node.GetNodeType() == NodeType.LINK_NODE;
		}

		return true;
	}

	private int GetModifiers(List<Token> tokens)
	{
		KeywordToken first = (KeywordToken)tokens[MODIFIER];
		KeywordToken second = (KeywordToken)tokens[MODIFIER + 1];

		int modifiers = AccessModifier.PUBLIC;

		if (first != null)
		{
			AccessModifierKeyword modifier = (AccessModifierKeyword)first.Keyword;
			modifiers |= modifier.Modifier;

			if (second != null)
			{
				modifier = (AccessModifierKeyword)second.Keyword;
				modifiers |= modifier.Modifier;
			}
		}

		return modifiers;
	}

	private Type GetType(Context context, List<Token> tokens)
	{
		Token token = tokens[TYPE];

		if (token.Type == TokenType.DYNAMIC)
		{
			Node node = ((DynamicToken)token).Node;

			if (node.GetNodeType() == NodeType.TYPE_NODE)
			{
				TypeNode type = (TypeNode)node;
				return type.Type;
			}
			else if (node is IResolvable)
			{
				return new UnresolvedType(context, (IResolvable)node);
			}

			throw new Exception("Node must be resolvable");
		}
		else
		{
			IdentifierToken id = (IdentifierToken)token;

			if (context.IsTypeDeclared(id.Value))
			{
				return context.GetType(id.Value);
			}

			return new UnresolvedType(context, id.Value);
		}
	}

	private string GetName(List<Token> tokens)
	{
		return ((IdentifierToken)tokens[NAME]).Value;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		int modifiers = GetModifiers(tokens);
		Type type = GetType(context, tokens);
		string name = GetName(tokens);

		if (context.IsLocalVariableDeclared(name))
		{
			throw Errors.Get(tokens[NAME].Position, $"Variable '{name}' already exists in this context");
		}

		VariableCategory category = context.IsGlobal ? VariableCategory.GLOBAL : VariableCategory.MEMBER;
		Variable variable = new Variable(context, type, category, name, modifiers);

		return new VariableNode(variable);
	}
}
