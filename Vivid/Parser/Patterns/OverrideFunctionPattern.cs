using System.Collections.Generic;
using System.Linq;

class OverrideFunctionPattern : Pattern
{
	public const int PRIORITY = 22;

	public const int OVERRIDE = 0;
	public const int FUNCTION = 1;

	// Pattern 1: override $name (...) [\n] {...}
	// Pattern 2: override $name (...) [\n] => ...
	public OverrideFunctionPattern() : base
	(
		TokenType.KEYWORD,
		TokenType.FUNCTION,
		TokenType.END | TokenType.OPTIONAL
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		if (!context.IsType || !tokens[OVERRIDE].Is(Keywords.OVERRIDE)) return false; // Override functions must be inside types

		var next = Pattern.Peek(state);
		if (next == null) return false;

		if (next.Is(ParenthesisType.CURLY_BRACKETS) || next.Is(Operators.HEAVY_ARROW))
		{
			Pattern.Consume(state);
			return true;
		}

		return false;
	}

	public override Node Build(Context context, PatternState state, List<Token> tokens)
	{
		var blueprint = (List<Token>?)null;
		var end = (Position?)null;

		if (tokens.Last().Is(Operators.HEAVY_ARROW))
		{
			var position = tokens.Last().Position;
			blueprint = Common.ConsumeBlock(state);
			blueprint.Insert(0, new OperatorToken(Operators.HEAVY_ARROW, position));
			if (blueprint.Any()) { end = Common.GetEndOfToken(blueprint.Last()); }
		}
		else
		{
			blueprint = tokens.Last().To<ContentToken>().Tokens;
			end = tokens.Last().To<ContentToken>().End;
		}

		var descriptor = tokens[FUNCTION].To<FunctionToken>();
		var function = new Function(context, Modifier.DEFAULT, descriptor.Name, blueprint, descriptor.Position, end);

		function.Parameters.AddRange(descriptor.GetParameters(function));

		context.To<Type>().DeclareOverride(function);

		return new FunctionDefinitionNode(function, descriptor.Position);
	}
}