using System.Collections.Generic;

public class ElsePattern : Pattern
{
	public const int PRIORITY = 1;

	public const int FORMER = 0;
	public const int ELSE = 2;
	public const int BODY = 4;

	// $([else] if) [\n] else [\n] (...)
	public ElsePattern() : base
	(
		TokenType.DYNAMIC, 
		TokenType.END | TokenType.OPTIONAL, 
		TokenType.KEYWORD,
		TokenType.END | TokenType.OPTIONAL, 
		TokenType.CONTENT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		if (tokens[ELSE].To<KeywordToken>().Keyword != Keywords.ELSE)
		{
			return false;
		}
		
		var former = tokens[FORMER].To<DynamicToken>();

		return former.Node.GetNodeType() == NodeType.IF_NODE ||
				former.Node.GetNodeType() == NodeType.ELSE_IF_NODE;
	}

	public override Node? Build(Context environment, List<Token> tokens)
	{
		var body = tokens[BODY].To<ContentToken>();

		var context = new Context();
		context.Link(environment);

		return new ElseNode(context, Parser.Parse(context, body.GetTokens()));
	}

	public override int GetStart()
	{
		return 1;
	}
}
