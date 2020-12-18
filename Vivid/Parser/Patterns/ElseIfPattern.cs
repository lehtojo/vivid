using System.Collections.Generic;
using System.Linq;

public class ElseIfPattern : Pattern
{
	public const int PRIORITY = 1;

	public const int FORMER = 0;
	public const int ELSE = 2;
	public const int CONDITION = 3;
	public const int BODY = 5;

	// $if [\n] else $bool [\n] [{}]
	public ElseIfPattern() : base
	(
		TokenType.DYNAMIC,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.KEYWORD,
		TokenType.OBJECT,
		TokenType.END | TokenType.OPTIONAL
	)
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		var previous = tokens[FORMER].To<DynamicToken>();

		if (!previous.Node.Is(NodeType.IF, NodeType.ELSE_IF) ||
			tokens[ELSE].To<KeywordToken>().Keyword != Keywords.ELSE)
		{
			return false;
		}

		if (tokens[CONDITION].Is(ParenthesisType.CURLY_BRACKETS))
		{
			return false;
		}

		Try(state, () => Consume(state, out Token? body, TokenType.CONTENT) && body!.To<ContentToken>().Type == ParenthesisType.CURLY_BRACKETS);
		return true;
	}

	public override Node? Build(Context environment, PatternState state, List<Token> tokens)
	{
		var condition = Singleton.Parse(environment, tokens[CONDITION]);
		var body = tokens.Last().Is(ParenthesisType.CURLY_BRACKETS) ? tokens.Last().To<ContentToken>().Tokens : null;

		var context = new Context(environment);

		if (body == null)
		{
			body = new List<Token>();
			
			if (!Common.ConsumeBlock(context, state, body))
			{
				throw Errors.Get(tokens[ELSE].Position, "Else-if-statement has an empty body");
			}
		}

		var node = Parser.Parse(context, body, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY);

		return new ElseIfNode(context, condition, node);
	}

	public override int GetStart()
	{
		return 1;
	}
}