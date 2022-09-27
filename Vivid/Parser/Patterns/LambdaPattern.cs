using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class LambdaPattern : Pattern
{
	private const int PRIORITY = 19;

	private const int PARAMETERS = 0;
	private const int OPERATOR = 1;
	private const int BODY = 3;

	// Pattern 1: ($1, $2, ..., $n) -> [\n] ...
	// Pattern 2: $name -> [\n] ...
	// Pattern 3: ($1, $2, ..., $n) -> [\n] {...}
	public LambdaPattern() : base
	(
		 TokenType.PARENTHESIS | TokenType.IDENTIFIER,
		 TokenType.OPERATOR,
		 TokenType.END | TokenType.OPTIONAL
	) { }

	private static bool TryConsumeBody(Context context, PatternState state)
	{
		if (Common.ConsumeBody(state))
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
				typeof(OffsetPattern),
				typeof(OperatorPattern),
				typeof(PreIncrementAndDecrementPattern),
				typeof(PostIncrementAndDecrementPattern),
				typeof(ReturnPattern),
				typeof(UnarySignPattern),
				typeof(IsPattern)
			}

		).Count > 0;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		if ((tokens[PARAMETERS].Is(TokenType.PARENTHESIS) && !tokens[PARAMETERS].Is(ParenthesisType.PARENTHESIS)) || !tokens[OPERATOR].Is(Operators.ARROW))
		{
			return false;
		}

		return TryConsumeBody(context, state);
	}

	private static ParenthesisToken GetParameterTokens(List<Token> tokens)
	{
		return tokens[PARAMETERS].Type == TokenType.PARENTHESIS
			? tokens[PARAMETERS].To<ParenthesisToken>()
			: new ParenthesisToken(tokens[PARAMETERS]);
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		var blueprint = tokens[BODY].Is(ParenthesisType.CURLY_BRACKETS) ? tokens[BODY].To<ParenthesisToken>().Tokens : tokens.Skip(BODY).ToList();

		if (!tokens[BODY].Is(ParenthesisType.CURLY_BRACKETS))
		{
			blueprint.Insert(0, new KeywordToken(Keywords.RETURN, tokens[OPERATOR].Position));
		}

		var start = tokens[PARAMETERS].Position;
		var end = tokens[BODY].Is(ParenthesisType.CURLY_BRACKETS) ? tokens[BODY].To<ParenthesisToken>().End : null;

		var environment = context.GetImplementationParent() ?? throw Errors.Get(start, "Lambda must be inside a function");
		var name = environment.CreateLambda().ToString();

		// Create a function token manually since it contains some useful helper functions
		var function = new FunctionToken(new IdentifierToken(name), GetParameterTokens(tokens));

		/// NOTE: Use the current context instead of the parent function context, because we could be inside a nested scope for example and we must be able capture variables from that scope
		var lambda = new Lambda(context, Modifier.DEFAULT, name, blueprint, start, end);
		environment.Declare(lambda);

		lambda.Parameters.AddRange(function.GetParameters(lambda));

		if (lambda.Parameters.All(i => i.Type != null && !i.Type.IsUnresolved))
		{
			var implementation = lambda.Implement(lambda.Parameters.Select(i => i.Type!));

			return new LambdaNode(implementation, start);
		}

		return new LambdaNode(lambda, start);
	}

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}
}