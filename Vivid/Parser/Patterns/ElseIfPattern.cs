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

	private static bool TryConsumeBody(Context context, PatternState state)
	{
		if (Try(state, () => Consume(state, out Token? body, TokenType.CONTENT) && body!.To<ContentToken>().Type == ParenthesisType.CURLY_BRACKETS))
		{
			return true;
		}

		return Consume
		(
			context,
			state,
			new List<System.Type>
			{
				typeof(CastPattern),
				typeof(CommandPattern),
				typeof(LinkPattern),
				typeof(NotPattern),
				typeof(OperatorPattern),
				typeof(PreIncrementAndDecrementPattern),
				typeof(PostIncrementAndDecrementPattern),
				typeof(ReturnPattern),
				typeof(UnarySignPattern)
			}

		).Count > 0;
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

		return TryConsumeBody(context, state);
	}

	public override Node? Build(Context environment, List<Token> tokens)
	{
		var condition = Singleton.Parse(environment, tokens[CONDITION]);
		var body = tokens[BODY].Is(ParenthesisType.CURLY_BRACKETS) ? tokens[BODY].To<ContentToken>().Tokens : tokens.Skip(BODY).ToList();

		var context = new Context();
		context.Link(environment);

		return new ElseIfNode(context, condition, Parser.Parse(context, body, 0, 20));
	}

	public override int GetStart()
	{
		return 1;
	}
}