using System.Collections.Generic;
using System.Linq;

public class ElsePattern : Pattern
{
	public const int PRIORITY = 1;

	public const int FORMER = 0;
	public const int ELSE = 2;
	public const int BODY = 4;

	// $([else] if) [\n] else [\n] [{}]
	public ElsePattern() : base
	(
		TokenType.DYNAMIC,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.KEYWORD,
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
				typeof(OffsetPattern),
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
		if (tokens[ELSE].To<KeywordToken>().Keyword != Keywords.ELSE)
		{
			return false;
		}

		var former = tokens[FORMER].To<DynamicToken>();

		return (former.Node.GetNodeType() == NodeType.IF ||
				former.Node.GetNodeType() == NodeType.ELSE_IF) &&
				TryConsumeBody(context, state);
	}

	public override Node? Build(Context environment, List<Token> tokens)
	{
		var body = tokens[BODY].Is(ParenthesisType.CURLY_BRACKETS) ? tokens[BODY].To<ContentToken>().Tokens : tokens.Skip(BODY).ToList();

		var context = new Context();
		context.Link(environment);

		return new ElseNode(context, Parser.Parse(context, body));
	}

	public override int GetStart()
	{
		return 1;
	}
}
