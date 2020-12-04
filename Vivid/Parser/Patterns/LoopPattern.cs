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

	// Pattern: loop [(...)] [\n] (...)
	public LoopPattern() : base
	(
		TokenType.KEYWORD, TokenType.CONTENT | TokenType.OPTIONAL, TokenType.END | TokenType.OPTIONAL, TokenType.CONTENT
	)
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return tokens[KEYWORD].To<KeywordToken>().Keyword == Keywords.LOOP;
	}

	private static Node? GetSteps(Context context, ContentToken content)
	{
		if (content.IsEmpty)
		{
			return null;
		}

		var steps = new Node();
		var sections = content.GetSections();

		foreach (var section in sections)
		{
			// Parse the tokens of the step and add them under a normal node
			var step = new Node();
			Parser.Parse(step, context, section);

			steps.Add(step);
		}

		switch (sections.Count)
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

		steps_context.Link(environment);
		body_context.Link(steps_context);

		var steps_token = tokens[STEPS];

		Node? steps = null;

		if (steps_token.Type != TokenType.NONE)
		{
			steps = GetSteps(steps_context, steps_token.To<ContentToken>());
		}

		var token = tokens[BODY].To<ContentToken>();

		var body = new ContextNode(body_context);
		Parser.Parse(body_context, token.Tokens, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY).ForEach(n => body.Add(n));

		return new LoopNode(steps_context, steps, body, tokens[KEYWORD].Position);
	}
}