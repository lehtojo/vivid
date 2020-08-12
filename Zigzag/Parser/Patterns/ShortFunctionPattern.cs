using System.Collections.Generic;

public class ShortFunctionPattern : ConsumingPattern
{
	public const int PRIORITY = 20;

	public const int HEADER = 0;
	public const int OPERATOR = 2;
	public const int CURLY_BRACKETS = 3;

	// a-z (...) [\n] => ...
	public ShortFunctionPattern() : base
	(
		TokenType.FUNCTION, TokenType.END | TokenType.OPTIONAL, TokenType.OPERATOR, TokenType.CONTENT | TokenType.OPTIONAL
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	private static bool HasCurlyBrackets(List<Token> tokens)
	{
		return tokens[CURLY_BRACKETS].Type == TokenType.CONTENT && tokens[CURLY_BRACKETS].To<ContentToken>().Type == ParenthesisType.CURLY_BRACKETS;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return tokens[OPERATOR].To<OperatorToken>().Operator == Operators.IMPLICATION;
	}

	public override Node Build(Context context, List<Token> tokens, ConsumptionState state)
	{
		var header = tokens[HEADER].To<FunctionToken>();
		var value = tokens[CURLY_BRACKETS];

		Function? function;

		if (HasCurlyBrackets(tokens))
		{
			function = new Function(context, AccessModifier.PUBLIC, header.Name, new List<Token>() { value });
			function.Parameters = header.GetParameters(function);
		}
		else
		{
			// Consume the code if there is no curly brackets
			if (!state.IsConsumed)
			{
				// Consume only tokens which represent the following patterns
				state.Consume(new List<System.Type>
				{
					typeof(ArrayAllocationPattern),
					typeof(CastPattern),
					typeof(LinkPattern),
					typeof(NotPattern),
					typeof(OperatorPattern),
					typeof(PreIncrementAndDecrementPattern),
					typeof(UnarySignPattern),
				});
				
				return new Node();
			}

			var blueprint = new List<Token> { tokens[OPERATOR] };
			blueprint.AddRange(tokens.GetRange(CURLY_BRACKETS + 1, tokens.Count - CURLY_BRACKETS - 1));

			function = new Function(context, AccessModifier.PUBLIC, header.Name, blueprint);
			function.Parameters = header.GetParameters(function);
		}

		context.Declare(function);

		return new FunctionDefinitionNode(function);
	}
}
