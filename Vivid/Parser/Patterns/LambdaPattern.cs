using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class LambdaPattern : Pattern
{
	private const int PRIORITY = 19;

	private const int PARAMETERS = 0;
	private const int OPERATOR = 1;
	private const int BODY = 3;

	// Examples:
	// (a: num, b) => [\n] a + b - 10
	// x => [\n] x * x
	// y: System => [\n] y.start()
	// (z) => [\n] { if z > 0 { => 1 } else => -1 }
	public LambdaPattern() : base
	(
		 TokenType.CONTENT | TokenType.IDENTIFIER,
		 TokenType.OPERATOR,
		 TokenType.END | TokenType.OPTIONAL
	)
	{ }

	private static bool TryConsumeBody(Context context, PatternState state)
	{
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

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		if (!tokens[OPERATOR].Is(Operators.IMPLICATION))
		{
			return false;
		}

		return TryConsumeBody(context, state);
	}

	private static ContentToken GetParameterTokens(List<Token> tokens)
	{
		return tokens[PARAMETERS].Type == TokenType.CONTENT
		   ? tokens[PARAMETERS].To<ContentToken>()
		   : new ContentToken(tokens[PARAMETERS]);
	}

	public override Node? Build(Context context, List<Token> tokens)
	{
		var body = tokens[BODY].Is(ParenthesisType.CURLY_BRACKETS) ? tokens[BODY].To<ContentToken>().Tokens : tokens.Skip(BODY).ToList();

		var name = context.GetNextLambda().ToString(CultureInfo.InvariantCulture);

		// Create a function token manually since it contains some useful helper functions
		var function = new FunctionToken(
		   new IdentifierToken(name),
		   GetParameterTokens(tokens)
		);

		var lambda = new Lambda(
		   context,
		   AccessModifier.PUBLIC,
		   name,
		   body
		);

		lambda.Parameters = function.GetParameters(lambda);

		if (lambda.Parameters.All(p => p.Type != null && !p.Type.IsUnresolved))
		{
			lambda.Implement(lambda.Parameters.Select(p => p.Type!));
		}

		return new LambdaNode(lambda);
	}

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}
}