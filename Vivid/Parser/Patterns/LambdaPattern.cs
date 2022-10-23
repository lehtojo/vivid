using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class LambdaPattern : Pattern
{
	private const int PARAMETERS = 0;
	private const int OPERATOR = 1;

	// Pattern 1: ($1, $2, ..., $n) -> [\n] ...
	// Pattern 2: $name -> [\n] ...
	// Pattern 3: ($1, $2, ..., $n) -> [\n] {...}
	public LambdaPattern() : base
	(
		 TokenType.PARENTHESIS | TokenType.IDENTIFIER,
		 TokenType.OPERATOR,
		 TokenType.END | TokenType.OPTIONAL
	)
	{ Priority = 19; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		// If the parameters are added inside parenthesis, it must be a normal parenthesis
		if (tokens[PARAMETERS].Is(TokenType.PARENTHESIS) && !tokens[PARAMETERS].Is(ParenthesisType.PARENTHESIS)) return false;
		if (!tokens[OPERATOR].Is(Operators.ARROW)) return false;

		// Try to consume normal curly parenthesis as the body blueprint
		var next = state.Peek();
		if (next != null && next.Is(ParenthesisType.CURLY_BRACKETS)) state.Consume();

		return true;
	}

	private static ParenthesisToken GetParameterTokens(List<Token> tokens)
	{
		return tokens[PARAMETERS].Type == TokenType.PARENTHESIS
			? tokens[PARAMETERS].To<ParenthesisToken>()
			: new ParenthesisToken(tokens[PARAMETERS]);
	}

	public override Node? Build(Context context, ParserState state, List<Token> tokens)
	{
		var blueprint = (List<Token>?)null;
		var start = tokens[PARAMETERS].Position;
		var end = (Position?)null;
		var last = tokens[tokens.Count - 1];

		// Load the function blueprint
		if (last.Is(ParenthesisType.CURLY_BRACKETS))
		{
			blueprint = last.To<ParenthesisToken>().Tokens;
			end = last.To<ParenthesisToken>().End;
		}
		else
		{
			blueprint = new List<Token>();
			var position = last.Position;

			Common.ConsumeBlock(state, blueprint);

			blueprint.Insert(0, new KeywordToken(Keywords.RETURN, position));
			if (blueprint.Count > 0) { end = Common.GetEndOfToken(blueprint[blueprint.Count - 1]); }
		}

		var environment = context.FindLambdaContainerParent();
		if (environment == null) throw Errors.Get(start, "Can not create a lambda here");

		var name = environment.CreateLambda().ToString();

		// Create a function token manually since it contains some useful helper functions
		var header = new FunctionToken(new IdentifierToken(name), GetParameterTokens(tokens));
		var function = new Lambda(context, Modifier.DEFAULT, name, blueprint, start, end);
		environment.Declare(function);

		// Parse the lambda parameters
		var parameters = header.GetParameters(function);
		function.Parameters.AddRange(parameters);

		// The lambda can be implemented already, if all parameters are resolved
		var implement = true;

		foreach (var parameter in parameters)
		{
			if (parameter.Type != null && parameter.Type.IsResolved()) continue;
			implement = false;
			break;
		}

		if (implement)
		{
			var types = parameters.Select(i => i.Type!).ToList();
			var implementation = function.Implement(types);
			return new LambdaNode(implementation, start);
		}

		return new LambdaNode(function, start);
	}
}