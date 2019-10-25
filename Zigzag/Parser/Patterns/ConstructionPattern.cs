using System.Collections.Generic;

public class ConstructionPattern : Pattern
{
	public const int PRIORITY = 19;

	private const int NEW = 0;
	private const int CONSTRUCTOR = 1;

	// Pattern:
	// new Type(...)
	// new Type.Subtype(...)
	public ConstructionPattern() : base(TokenType.KEYWORD, TokenType.FUNCTION | TokenType.DYNAMIC) {}

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(List<Token> tokens)
	{
		KeywordToken keyword = (KeywordToken)tokens[NEW];

		if (keyword.Keyword != Keywords.NEW)
		{
			return false;
		}

		Token token = tokens[CONSTRUCTOR];

		if (token.Type == TokenType.DYNAMIC)
		{
			DynamicToken dynamic = (DynamicToken)token;
			return dynamic.Node.GetNodeType() == NodeType.LINK_NODE;
		}

		return true;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		return new ConstructionNode(Singleton.Parse(context, tokens[CONSTRUCTOR]));
	}
}