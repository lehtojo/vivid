﻿using System.Collections.Generic;

public class LoopPattern : Pattern
{
	public const int KEYWORD = 0;
	public const int STEPS = 1;
	public const int BODY = 3;

	public const int WHILE_LOOP = 1; // Example: (i < 10)
	public const int SHORT_FOR_LOOP = 2; // Example: (i < 10, i++)
	public const int FOR_LOOP = 3; // Example: (i = 0, i < 10, i++)

	// Pattern: loop [(...)] [\n] {...}
	public LoopPattern() : base
	(
		TokenType.KEYWORD, TokenType.PARENTHESIS, TokenType.END | TokenType.OPTIONAL, TokenType.PARENTHESIS
	)
	{ Priority = 1; IsConsumable = false; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return tokens[KEYWORD].To<KeywordToken>().Keyword == Keywords.LOOP && tokens[BODY].Is(ParenthesisType.CURLY_BRACKETS);
	}

	private static Node? GetSteps(Context context, ParenthesisToken content)
	{
		if (content.IsEmpty) return null;

		var steps = (Node?)null;
		var sections = content.GetSections();

		switch (sections.Count)
		{
			case WHILE_LOOP:
			{
				steps = new Node { new Node() };
				steps.Add(Parser.Parse(context, sections[0], Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY));
				steps.Add(new Node());
				break;
			}

			case SHORT_FOR_LOOP:
			{
				steps = new Node { new Node() };
				steps.Add(Parser.Parse(context, sections[0], Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY));
				steps.Add(Parser.Parse(context, sections[1], Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY));
				break;
			}

			case FOR_LOOP:
			{
				steps = new Node { Parser.Parse(context, sections[0], Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY) };
				steps.Add(Parser.Parse(context, sections[1], Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY));
				steps.Add(Parser.Parse(context, sections[2], Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY));
				break;
			}

			default:
			{
				throw Errors.Get(content.Position, "Too many sections");
			}
		}

		return steps;
	}

	public override Node Build(Context environment, ParserState state, List<Token> tokens)
	{
		var steps_context = new Context(environment);
		var body_context = new Context(steps_context);

		var steps_token = tokens[STEPS];
		var steps = GetSteps(steps_context, steps_token.To<ParenthesisToken>());
		if (steps == null) throw Errors.Get(tokens[0].Position, "Can not understand");

		var body_token = tokens[BODY].To<ParenthesisToken>();
		var body = new ScopeNode(body_context, body_token.Position, body_token.End, false);

		Parser.Parse(body_context, body_token.Tokens, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY).ForEach(i => body.Add(i));

		return new LoopNode(steps_context, steps, body, tokens[KEYWORD].Position);
	}
}