﻿using System.Collections.Generic;

public class OffsetPattern : Pattern
{
	private const int PRIORITY = 19;

	private const int OBJECT = 0;
	private const int ARGUMENTS = 1;

	// Pattern: ... [...]
	public OffsetPattern() : base
	(
		TokenType.OBJECT, TokenType.CONTENT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return tokens[ARGUMENTS].To<ContentToken>().Type == ParenthesisType.BRACKETS;
	}

	public override Node Build(Context context, PatternState state, List<Token> tokens)
	{
		var source = Singleton.Parse(context, tokens[OBJECT]);
		var arguments = Singleton.Parse(context, tokens[ARGUMENTS]);

		// If there are no arguments, add number zero as argument
		if (arguments.First == null)
		{
			arguments.Add(new NumberNode(Parser.Format, 0L, tokens[ARGUMENTS].Position));
		}

		return new OffsetNode(source, arguments, tokens[ARGUMENTS].Position);
	}
}