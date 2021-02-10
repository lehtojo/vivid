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

		var steps = (Node?)null;
		var sections = content.GetSections();

		switch (sections.Count)
		{
			case WHILE_LOOP:
			{
				// Create root of the steps with an empty initialization step
				steps = new Node { new Node() };

				// Add the condition
				steps.Add(Parser.Parse(context, sections[0], Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY));

				// Add an empty action step
				steps.Add(new Node());
				break;
			}

			case SHORT_FOR_LOOP:
			{
				// Create root of the steps with an empty initialization step
				steps = new Node { new Node() };

				// Add the condition
				steps.Add(Parser.Parse(context, sections[0], Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY));

				// Add the action step
				steps.Add(Parser.Parse(context, sections[1], Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY));
				break;
			}

			case FOR_LOOP:
			{
				// Create root of the steps with the initialization step
				steps = new Node { Parser.Parse(context, sections[0], Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY) };

				// Add the condition
				steps.Add(Parser.Parse(context, sections[1], Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY));

				// Add an empty action node
				steps.Add(Parser.Parse(context, sections[2], Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY));
				break;
			}

			default:
			{
				throw Errors.Get(content.Position, "The loop has too many sections");
			}
		}

		return steps;
	}

	public override Node Build(Context environment, PatternState state, List<Token> tokens)
	{
		var steps_context = new Context(environment);
		var body_context = new Context(steps_context);

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