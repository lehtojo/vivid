using System.Collections.Generic;

public class LoopPattern : Pattern
{
	public const int PRIORITY = 1;

	public const int KEYWORD = 0;
	public const int STEPS = 1;
	public const int BODY = 3;

	// (i < 10)
	public const int WHILE_LOOP = 1;

	// (i < 10, i++)
	public const int SHORT_FOR_LOOP = 2;

	// (i = 0, i < 10, i++)
	public const int FOR_LOOP = 3;

	// loop (...) [\n] (...)
	public LoopPattern() : base
	(
		TokenType.KEYWORD, TokenType.CONTENT, TokenType.END | TokenType.OPTIONAL, TokenType.CONTENT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		return tokens[KEYWORD].To<KeywordToken>().Keyword == Keywords.LOOP;
	}

	private Node? GetSteps(Context context, ContentToken content)
	{
		var steps = new Node();

		for (var i = 0; i < content.SectionCount; i++)
		{
			Parser.Parse(steps, context, content.GetTokens(i));
		}

		if (content.IsEmpty)
		{
			return null;
		}

		switch (content.SectionCount)
		{
			case WHILE_LOOP:
			{
				// Padding: ([Added], Condition, [Added])
				steps.Insert(steps.First!, new Node());
				steps.Add(new Node());
				break;
			}

			case SHORT_FOR_LOOP:
			{
				// Padding: ([Added], Initialization, Action)
				steps.Insert(steps.First!, new Node());
				break;
			}

			case FOR_LOOP:
			{
				// Padding: (Initialization, Condition, Action)
				break;
			}

			default:
			{
				throw Errors.Get(content.Position, "Invalid loop");
			}
		}

		return steps;
	}

	public override Node Build(Context environment, List<Token> tokens)
	{
		var body_context = new Context();
		var steps_context = new Context();

		body_context.Link(environment);
		steps_context.Link(environment);

		var steps = GetSteps(steps_context, tokens[STEPS].To<ContentToken>());

		var token = tokens[BODY].To<ContentToken>();
		var body = Parser.Parse(body_context, token.GetTokens(), 0, 20);

		return new LoopNode(steps_context, body_context, steps, body);
	}
}
