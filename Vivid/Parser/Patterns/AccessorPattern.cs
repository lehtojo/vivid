﻿using System.Collections.Generic;

public class AccessorPattern : Pattern
{
	private const int OBJECT = 0;
	private const int ARGUMENTS = 1;

	// Pattern: ... [...]
	public AccessorPattern() : base
	(
		TokenType.OBJECT, TokenType.PARENTHESIS
	)
	{ Priority = 19; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return tokens[ARGUMENTS].To<ParenthesisToken>().Opening == ParenthesisType.BRACKETS;
	}

	public override Node Build(Context context, ParserState state, List<Token> tokens)
	{
		var source = Singleton.Parse(context, tokens[OBJECT]);
		var arguments = Singleton.Parse(context, tokens[ARGUMENTS]);

		// If there are no arguments, add number zero as argument
		if (arguments.First == null)
		{
			arguments.Add(new NumberNode(Parser.Format, 0L, tokens[ARGUMENTS].Position));
		}

		return new AccessorNode(source, arguments, tokens[ARGUMENTS].Position);
	}
}