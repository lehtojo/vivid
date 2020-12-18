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

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		if (!tokens[OPERATOR].Is(Operators.IMPLICATION))
		{
			return false;
		}

		Common.ConsumeBody(state);
		return true;
	}

	public override Node Build(Context context, PatternState state, List<Token> tokens)
	{
		var header = tokens[HEADER].To<FunctionToken>();
		var blueprint = (List<Token>?)null;

		if (tokens.Last().Is(ParenthesisType.CURLY_BRACKETS))
		{
			blueprint = tokens.Last().To<ContentToken>().Tokens;
		}

		var function = new Function(context, AccessModifier.PUBLIC, header.Name) { Position = header.Position };
		function.Parameters.AddRange(header.GetParameters(function));
		context.Declare(function);

		// Declare a self pointer if the function is a member of a type, since consuming the body may require it
		if (function.IsMember)
		{
			function.DeclareSelfPointer();
		}

		if (blueprint == null)
		{
			blueprint = new List<Token> { new OperatorToken(Operators.IMPLICATION) };

			if (!Common.ConsumeBlock(function, state, blueprint))
			{
				throw Errors.Get(header.Position, "Short function has an empty body");
			}

			tokens.AddRange(blueprint);
		}

		function.Blueprint.AddRange(blueprint);

		return new FunctionDefinitionNode(function, header.Position);
	}
}
