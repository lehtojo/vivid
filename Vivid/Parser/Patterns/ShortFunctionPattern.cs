using System.Collections.Generic;
using System.Linq;

public class ShortFunctionPattern : Pattern
{
	public const int PRIORITY = 20;

	public const int HEADER = 0;
	public const int ARROW = 1;
	public const int BODY = 2;

	// Pattern: $name (...) => ...
	public ShortFunctionPattern() : base
	(
		TokenType.FUNCTION, TokenType.OPERATOR
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		if (!tokens[ARROW].Is(Operators.HEAVY_ARROW)) return false;

		Common.ConsumeBody(state);
		return true;
	}

	public override Node Build(Context context, PatternState state, List<Token> tokens)
	{
		var header = tokens[HEADER].To<FunctionToken>();
		var blueprint = tokens.Last().Is(ParenthesisType.CURLY_BRACKETS) ? tokens.Last().To<ContentToken>().Tokens : null;

		var start = header.Position;
		var end = tokens.Last().Is(ParenthesisType.CURLY_BRACKETS) ? tokens.Last().To<ContentToken>().End : null;

		var function = new Function(context, Modifier.DEFAULT, header.Name, start, end);
		function.Parameters.AddRange(header.GetParameters(function));
		
		context.Declare(function);

		// Declare a self pointer if the function is a member of a type, since consuming the body may require it
		if (function.IsMember && !function.IsStatic)
		{
			function.DeclareSelfPointer();
		}

		if (blueprint == null)
		{
			blueprint = new List<Token> { tokens[ARROW] };

			if (!Common.ConsumeBlock(function, state, blueprint))
			{
				throw Errors.Get(header.Position, "Expected a short function body");
			}

			tokens.AddRange(blueprint);
		}

		function.Blueprint.AddRange(blueprint);

		return new FunctionDefinitionNode(function, header.Position);
	}
}
