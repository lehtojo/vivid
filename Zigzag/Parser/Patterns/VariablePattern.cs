using System.Collections.Generic;

public class VariablePattern : Pattern
{
	public const int PRIORITY = 17;

	private const int TYPE = 0;
	private const int NAME = 1;

	// Pattern:
	// Type / Type.Subtype ...
	public VariablePattern() : base(TokenType.IDENTIFIER | TokenType.KEYWORD | TokenType.DYNAMIC, TokenType.IDENTIFIER) {}

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(List<Token> tokens)
	{
		Token token = tokens[TYPE];

		if (token.Type == TokenType.DYNAMIC)
		{
			DynamicToken dynamic = (DynamicToken)token;
			return dynamic.Node.GetNodeType() == NodeType.LINK_NODE;
		}
		else if (token.Type == TokenType.KEYWORD)
		{
			Keyword keyword = ((KeywordToken)token).Keyword;
			return keyword == Keywords.VAR;
		}

		return true;
	}

	private Type GetType(Context context, List<Token> tokens)
	{
		Token token = tokens[TYPE];

		if (token.Type == TokenType.DYNAMIC)
		{
			DynamicToken dynamic = (DynamicToken)token;
			Node node = dynamic.Node;

			if (node.GetNodeType() == NodeType.LINK_NODE)
			{
				return new UnresolvedType(context, (Resolvable)node);
			}

			throw Errors.Get(tokens[NAME].Position, "Couldn't resolve type of the variable '{0}'", GetName(tokens));
		}
		else if (token.Type == TokenType.KEYWORD)
		{
			return Types.UNKNOWN;
		}
		else
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
	}

	private string GetName(List<Token> tokens)
	{
		return ((IdentifierToken)tokens[NAME]).Value;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		Type type = GetType(context, tokens);
		string name = GetName(tokens);

		VariableCategory category = context.IsGlobal ? VariableCategory.GLOBAL : VariableCategory.LOCAL;

		if (context.IsLocalVariableDeclared(name))
		{
			throw Errors.Get(tokens[0].Position, $"Variable '{name}' already exists in this context");
		}

		Variable variable = new Variable(context, type, category, name, AccessModifier.PUBLIC);

		return new VariableNode(variable);
	}
}
