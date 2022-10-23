using System.Collections.Generic;

class OverrideFunctionPattern : Pattern
{
	public const int OVERRIDE = 0;
	public const int FUNCTION = 1;

	// Pattern: override $name (...) [\n] {...}
	public OverrideFunctionPattern() : base
	(
		TokenType.KEYWORD,
		TokenType.FUNCTION,
		TokenType.END | TokenType.OPTIONAL
	)
	{ Priority = 22; IsConsumable = false; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		if (!context.IsType || !tokens[OVERRIDE].Is(Keywords.OVERRIDE)) return false; // Override functions must be inside types

		// Consume the function body
		return state.ConsumeParenthesis(ParenthesisType.CURLY_BRACKETS);
	}

	public override Node Build(Context context, ParserState state, List<Token> tokens)
	{
		var descriptor = tokens[FUNCTION].To<FunctionToken>();
		var blueprint = tokens[tokens.Count - 1].To<ParenthesisToken>();
		var start = descriptor.Position;
		var end = blueprint.End;

		var function = new Function(context, Modifier.DEFAULT, descriptor.Name, blueprint.Tokens, start, end);

		// Parse the function parameters
		function.Parameters.AddRange(descriptor.GetParameters(function));

		// Declare the override function and return a function definition node
		context.To<Type>().DeclareOverride(function);
		return new FunctionDefinitionNode(function, start);
	}
}