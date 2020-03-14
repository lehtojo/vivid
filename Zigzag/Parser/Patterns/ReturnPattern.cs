using System.Collections.Generic;

class ReturnPattern : Pattern
{
	public const int PRIORITY = 1;

	public const int RETURN = 0;
	public const int SOURCE = 1;

	// => ...
	public ReturnPattern() : base
	(
		TokenType.OPERATOR, TokenType.NUMBER | TokenType.STRING | TokenType.DYNAMIC
	) {}

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		var @operator = (OperatorToken)tokens[RETURN];
		return @operator.Operator == Operators.RETURN;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		var token = tokens[SOURCE];
		var source = Singleton.Parse(context, token);

		var function = context.GetFunctionParent();

		if (function == null)
		{
			throw Errors.Get(tokens[RETURN].Position, "Return statement cannot be outside a function!");
		}

		if (token is NumberToken)
		{
			function.ReturnType = Types.NORMAL;
		}
		else if (token is StringToken)
		{
			function.ReturnType = Types.LINK;
		}

		return new ReturnNode(source);
	}
}