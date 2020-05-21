using System.Collections.Generic;

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
		return tokens[OPERATOR].To<OperatorToken>().Operator == Operators.RETURN;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		var header = tokens[HEADER].To<FunctionToken>();
		var value = tokens[VALUE];

		var function = new Function(context, AccessModifier.PUBLIC, header.Name, new List<Token>() { tokens[OPERATOR], value });
		function.Parameters = header.GetParameterNames(function);
		
		context.Declare(function);

		return new Node();
	}
}
