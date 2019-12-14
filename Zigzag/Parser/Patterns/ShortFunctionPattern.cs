using System;
using System.Collections.Generic;
using System.Text;

public class ShortFunctionPattern : Pattern
{
	public const int PRIORITY = 20;

	public const int HEADER = 0;
	public const int OPERATOR = 2;
	public const int VALUE = 3;

	// a-z (...) [\n] => ...
	public ShortFunctionPattern() : base
	(
		TokenType.FUNCTION, TokenType.END | TokenType.OPTIONAL, TokenType.OPERATOR, TokenType.OBJECT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		var @operator = tokens[OPERATOR] as OperatorToken;
		return @operator.Operator == Operators.RETURN;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		var header = tokens[HEADER] as FunctionToken;
		var value = tokens[VALUE];

		var function = new Function(context, AccessModifier.PUBLIC, header.Name, header.GetParameterNames(), new List<Token>() { tokens[OPERATOR], value });
		context.Declare(function);

		return new Node();
	}
}
