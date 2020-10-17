﻿using System.Collections.Generic;
using System.Linq;

public class IfPattern : Pattern
{
	public const int PRIORITY = 1;

	public const int KEYWORD = 0;
	public const int CONDITION = 1;
	public const int BODY = 3;

	// if $bool [\n] [{}]
	public IfPattern() : base
	(
		TokenType.KEYWORD, TokenType.OBJECT, TokenType.END | TokenType.OPTIONAL
	)
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		var keyword = tokens[KEYWORD].To<KeywordToken>();

		if (keyword.Keyword != Keywords.IF)
		{
			return false;
		}

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
				typeof(OperatorPattern),
				typeof(PreIncrementAndDecrementPattern),
				typeof(PostIncrementAndDecrementPattern),
				typeof(ReturnPattern),
				typeof(UnarySignPattern)
			}

		).Count > 0;
	}

	public override Node Build(Context environment, List<Token> tokens)
	{
		var condition = Singleton.Parse(environment, tokens[CONDITION]);
		var body = tokens[BODY].Is(ParenthesisType.CURLY_BRACKETS) ? tokens[BODY].To<ContentToken>().Tokens : tokens.Skip(BODY).ToList();

		var context = new Context();
		context.Link(environment);

		return new IfNode(context, condition, Parser.Parse(context, body, 0, 20));
	}
}