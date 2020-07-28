using System.Collections.Generic;

public class ElseIfPattern : Pattern
{
	public const int PRIORITY = 1;

	public const int FORMER = 0;
	public const int ELSE = 2;
	public const int CONDITION = 3;
	public const int BODY = 5;

	// $if [\n] else $bool [\n] (...) 
	public ElseIfPattern() : base
	(
		TokenType.DYNAMIC, 
		TokenType.END | TokenType.OPTIONAL, 
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
		var previous = tokens[FORMER].To<DynamicToken>();
		
		if (previous.Node.GetNodeType() != NodeType.IF_NODE ||
			tokens[ELSE].To<KeywordToken>().Keyword != Keywords.ELSE)
		{
			return false;
		}

		var condition = tokens[CONDITION].To<DynamicToken>();
		return condition.Node is IType type && Equals(type.GetType(), Types.BOOL);
	}

	public override Node? Build(Context environment, List<Token> tokens)
	{
		var former = tokens[FORMER].To<DynamicToken>().Node.To<IfNode>();
		var condition = tokens[CONDITION].To<DynamicToken>();
		var body = tokens[BODY].To<ContentToken>();

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