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

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		var previous = tokens[FORMER].To<DynamicToken>();
		
		if (!previous.Node.Is(NodeType.IF_NODE, NodeType.ELSE_IF_NODE) ||
			tokens[ELSE].To<KeywordToken>().Keyword != Keywords.ELSE)
		{
			return false;
		}

		var condition = tokens[CONDITION].To<DynamicToken>();
		return condition.Node is IType type && Equals(type.GetType(), Types.BOOL);
	}

	public override Node? Build(Context environment, List<Token> tokens)
	{
		var condition = tokens[CONDITION].To<DynamicToken>();
		var body = tokens[BODY].To<ContentToken>();

		var context = new Context();
		context.Link(environment);

		return new ElseIfNode(context, condition.Node, Parser.Parse(context, body.GetTokens()));
	}

	public override int GetStart()
	{
		return 1;
	}
}