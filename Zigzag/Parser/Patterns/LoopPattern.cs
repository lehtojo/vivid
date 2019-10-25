using System.Collections.Generic;
public class LoopPattern : Pattern
{
	public const int PRIORITY = 15;

	private const int WHILE = 0;
	private const int STEPS = 1;
	private const int BODY = 3;

	// Pattern:
	// while (...) [\n] {...}
	public LoopPattern() : base(TokenType.KEYWORD, /* loop */
								TokenType.DYNAMIC | TokenType.OPTIONAL, /* [(...)] */
								TokenType.END | TokenType.OPTIONAL, /* [\n] */
								TokenType.CONTENT) /* {...} */
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(List<Token> tokens)
	{
		KeywordToken keyword = (KeywordToken)tokens[WHILE];

		if (keyword.Keyword != Keywords.LOOP)
		{
			return false;
		}

		ContentToken body = (ContentToken)tokens[BODY];
		return body.Type == ParenthesisType.CURLY_BRACKETS;
	}

	private List<Token> GetBody(List<Token> tokens)
	{
		ContentToken content = (ContentToken)tokens[BODY];
		return content.GetTokens();
	}

	private Node Mold(List<Token> tokens, Node steps)
	{
		var iterator = steps.First;
		var count = 0;

		while (iterator != null)
		{
			count++;
			iterator = iterator.Next;
		}

		switch (count)
		{
			case 0:
			{
				throw Errors.Get(tokens[WHILE].Position, "Loop parenthesis cannot be empty");
			}
			case 1:
			{
				steps.Insert(steps.First, new Node());
				steps.Add(new Node());
				return steps;
			}
			case 2:
			{
				steps.Insert(steps.First, new Node());
				return steps;
			}
			default:
			{
				return steps;
			}
		}
	}

	private Node GetSteps(List<Token> tokens)
	{
		DynamicToken dynamic = (DynamicToken)tokens[STEPS];

		if (dynamic != null)
		{
			Node steps = Mold(tokens, dynamic.Node);
			return steps;
		}

		return null;
	}

	public override Node Build(Context @base, List<Token> tokens)
	{
		Context context = new Context();
		context.Link(@base);

		Node body = Parser.Parse(context, GetBody(tokens), Parser.MIN_PRIORITY, Parser.MEMBERS - 1);
		Node steps = GetSteps(tokens);

		return new LoopNode(context, steps, body);
	}
}
