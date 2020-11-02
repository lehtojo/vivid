using System.Collections.Generic;
using System.Linq;

public class ShortFunctionPattern : Pattern
{
	public const int PRIORITY = 20;

	public const int HEADER = 0;
	public const int OPERATOR = 1;
	public const int BODY = 2;

	// a-z (...) => ...
	public ShortFunctionPattern() : base
	(
		TokenType.FUNCTION, TokenType.OPERATOR
	)
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

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
				typeof(OffsetPattern),
				typeof(OperatorPattern),
				typeof(PreIncrementAndDecrementPattern),
				typeof(PostIncrementAndDecrementPattern),
				typeof(UnarySignPattern)
			}

		).Count > 0;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return tokens[OPERATOR].To<OperatorToken>().Operator == Operators.IMPLICATION && TryConsumeBody(context, state);
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		var header = tokens[HEADER].To<FunctionToken>();

		List<Token>? body;

		if (tokens[BODY].Is(ParenthesisType.CURLY_BRACKETS))
		{
			body = tokens[BODY].To<ContentToken>().Tokens;
		}
		else
		{
			body = tokens.Skip(BODY).ToList();
			body.Insert(0, new OperatorToken(Operators.IMPLICATION));
		}

		var function = new Function(context, AccessModifier.PUBLIC, header.Name, body);
		function.Parameters = header.GetParameters(function);

		context.Declare(function);

		return new FunctionDefinitionNode(function);
	}
}
