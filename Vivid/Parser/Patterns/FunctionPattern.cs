using System.Collections.Generic;
using System.Linq;

class FunctionPattern : Pattern
{
	public const int FUNCTION = 0;
	public const int COLON = 1;

	public const int RETURN_TYPE_START = COLON + 1;

	// Pattern: $name (...) [: $return-type] [\n] {...}
	public FunctionPattern() : base
	(
		TokenType.FUNCTION
	)
	{ Priority = 22; IsConsumable = false; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		// Look for a return type
		if (state.ConsumeOperator(Operators.COLON))
		{
			// Expected: $name (...) : $return-type [\n] {...}
			if (!Common.ConsumeType(state)) return false;
		}

		state.ConsumeOptional(TokenType.END); // Optionally consume a line ending

		return state.ConsumeParenthesis(ParenthesisType.CURLY_BRACKETS); // Consume the function body
	}

	public override Node Build(Context context, ParserState state, List<Token> tokens)
	{
		var descriptor = tokens[FUNCTION].To<FunctionToken>();
		var blueprint = tokens.Last().To<ParenthesisToken>();
		var return_type = (Type?)null;
		var start = descriptor.Position;
		var end = blueprint.End;

		// Process the return type if such was consumed
		if (tokens[COLON].Is(Operators.COLON))
		{
			// Collect the return type tokens after the colon and before the line ending
			var return_type_tokens = tokens.GetRange(RETURN_TYPE_START, tokens.Count - 2 - RETURN_TYPE_START);
			return_type = Common.ReadType(context, return_type_tokens);

			// Verify the return type could be parsed in some form
			if (return_type == null) throw Errors.Get(tokens[COLON].Position, "Could not understand the return type");
		}

		var function = new Function(context, Modifier.DEFAULT, descriptor.Name, blueprint.Tokens, start, end);
		function.ReturnType = return_type;
		function.Parameters.AddRange(descriptor.GetParameters(function));

		context.Declare(function);

		return new FunctionDefinitionNode(function, start);
	}
}
