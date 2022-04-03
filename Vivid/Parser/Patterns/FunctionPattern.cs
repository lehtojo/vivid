using System.Collections.Generic;
using System.Linq;

class FunctionPattern : Pattern
{
	public const int PRIORITY = 22;

	public const int FUNCTION = 0;
	public const int COLON = 1;

	// Pattern: $name (...) [\n] {...}
	public FunctionPattern() : base
	(
		TokenType.FUNCTION
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		// Look for a return type
		if (Consume(state, Operators.COLON))
		{
			// Expected: $name (...) : $return-type [\n] {...}
			if (!Common.ConsumeType(state)) return false;
		}

		Consume(state, TokenType.END | TokenType.OPTIONAL); // Optionally consume a line ending

		return Consume(state, ParenthesisType.CURLY_BRACKETS); // Consume the function body
	}

	public override Node Build(Context context, PatternState state, List<Token> tokens)
	{
		var descriptor = tokens[FUNCTION].To<FunctionToken>();
		var blueprint = tokens.Last().To<ContentToken>();
		var return_type = (Type?)null;

		// Process the return type if such was consumed
		if (tokens[COLON].Is(Operators.COLON))
		{
			var return_type_start = COLON + 1; // Start after the colon
			var return_type_end = tokens.Count - 2; // Stop before the line ending
			var return_type_tokens = tokens.GetRange(return_type_start, return_type_end - return_type_start);

			return_type = Common.ReadType(context, new Queue<Token>(return_type_tokens));

			// Verify the return type could be parsed in some form
			if (return_type == null) throw Errors.Get(tokens[COLON].Position, "Could not understand the return type");
		}

		var function = new Function(context, Modifier.DEFAULT, descriptor.Name, blueprint.Tokens, descriptor.Position, blueprint.End);
		function.ReturnType = return_type;
		function.Parameters.AddRange(descriptor.GetParameters(function));

		context.Declare(function);

		return new FunctionDefinitionNode(function, descriptor.Position);
	}
}
