﻿using System.Collections.Generic;

public class ListPattern : Pattern
{
	public const int PRIORITY = 0;

	public const int LEFT = 0;
	public const int COMMA = 1;
	public const int RIGHT = 2;

	// Pattern: ... , ...
	public ListPattern() : base
	(
		TokenType.OBJECT,
		TokenType.OPERATOR,
		TokenType.OBJECT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return tokens[COMMA].To<OperatorToken>().Operator == Operators.COMMA;
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		if (tokens[LEFT] is DynamicToken left && left.Node is ListNode list)
		{
			list.Add(Singleton.Parse(context, tokens[RIGHT]));
			return list;
		}

		return new ListNode(tokens[COMMA].Position, Singleton.Parse(context, tokens[LEFT]), Singleton.Parse(context, tokens[RIGHT]));
	}
}