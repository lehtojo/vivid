using System.Collections.Generic;

public class ElseIfPattern : Pattern
{
	public const int PRIORITY = 1;

	public const int FORMER = 0;
	public const int ELSE = 2;
	public const int IF = 3;
	public const int CONDITION = 4;
	public const int BODY = 6;

	// $if [\n] else if $bool [\n] (...) 
	public ElseIfPattern() : base
	(
		TokenType.DYNAMIC, 
		TokenType.END | TokenType.OPTIONAL, 
		TokenType.KEYWORD,
		TokenType.KEYWORD,
		TokenType.DYNAMIC, 
		TokenType.END | TokenType.OPTIONAL, 
		TokenType.CONTENT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		var previous = (DynamicToken)tokens[FORMER];
		
		if (previous.Node.GetNodeType() != NodeType.IF_NODE ||
			((KeywordToken)tokens[ELSE]).Keyword != Keywords.ELSE ||
			((KeywordToken)tokens[IF]).Keyword != Keywords.IF)
		{
			return false;
		}

		var condition = (DynamicToken)tokens[CONDITION];
		return condition.Node is IType type && type.GetType() == Types.BOOL;
	}

	public override Node Build(Context environment, List<Token> tokens)
	{
		var former = (IfNode)((DynamicToken)tokens[FORMER]).Node;
		var condition = (DynamicToken)tokens[CONDITION];
		var body = (ContentToken)tokens[BODY];

		var context = new Context();
		context.Link(environment);

		var node = new ElseIfNode(context, condition.Node, Parser.Parse(context, body.GetTokens()));
		former.AddSuccessor(node);

		return null;
	}

	public override int GetStart()
	{
		return 1;
	}
}