using System.Collections.Generic;

class ReturnPattern : Pattern
{
	public const int PRIORITY = 0;

	public const int RETURN = 0;
	public const int VALUE = 1;

	// => ...
	public ReturnPattern() : base
	(
		TokenType.OPERATOR, TokenType.OBJECT
	)
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return tokens[RETURN].To<OperatorToken>().Operator == Operators.IMPLICATION;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		var token = tokens[VALUE];
		var value = Singleton.Parse(context, token);

		var function = context.GetFunctionParent();

		if (function == null)
		{
			throw Errors.Get(tokens[RETURN].Position, "Return statement can not be outside a function");
		}

		return new ReturnNode(value, tokens[RETURN].Position);
	}
}