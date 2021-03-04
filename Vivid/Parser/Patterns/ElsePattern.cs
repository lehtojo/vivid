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

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		if (tokens[ELSE].To<KeywordToken>().Keyword != Keywords.ELSE)
		{
			return false;
		}

		var former = tokens[FORMER].To<DynamicToken>();

		if (!former.Node.Is(NodeType.IF, NodeType.ELSE_IF))
		{
			return false;
		}

		Try(state, () => Consume(state, out Token? body, TokenType.CONTENT) && body!.To<ContentToken>().Type == ParenthesisType.CURLY_BRACKETS);
		return true;
	}

	public override Node? Build(Context environment, PatternState state, List<Token> tokens)
	{
		var body = tokens.Last().Is(ParenthesisType.CURLY_BRACKETS) ? tokens.Last().To<ContentToken>().Tokens : null;
		var context = new Context(environment);

		if (body == null)
		{
			body = new List<Token>();

			if (!Common.ConsumeBlock(context, state, body))
			{
				throw Errors.Get(tokens[ELSE].Position, "Else-statement has an empty body");
			}
		}

		var node = Parser.Parse(context, body, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY);

		return new ElseNode(context, node, tokens[ELSE].Position);
	}

	public override int GetStart()
	{
		return 1;
	}
}
