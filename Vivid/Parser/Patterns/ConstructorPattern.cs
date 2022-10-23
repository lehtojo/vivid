using System.Collections.Generic;
using System.Linq;

public class ConstructorPattern : Pattern
{
	private const int HEADER = 0;

	// Pattern: init/deinit (...) [\n] {...}
	public ConstructorPattern() : base
	(
		TokenType.FUNCTION
	)
	{ Priority = 23; IsConsumable = false; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		// Constructors and destructors must be inside a type
		if (!context.IsType) return false;

		// Ensure the function matches either a constructor or a destructor
		var descriptor = tokens[HEADER].To<FunctionToken>();

		if (descriptor.Name != Keywords.INIT.Identifier && descriptor.Name != Keywords.DEINIT.Identifier) return false;

		// Optionally consume a line ending
		state.ConsumeOptional(TokenType.END);

		// Consume the function body
		return state.ConsumeParenthesis(ParenthesisType.CURLY_BRACKETS);
	}

	public override Node? Build(Context context, ParserState state, List<Token> tokens)
	{
		var descriptor = tokens[HEADER].To<FunctionToken>();
		var type = (Type)context;

		var blueprint = tokens.Last().To<ParenthesisToken>();
		var start = descriptor.Position;
		var end = blueprint.End;

		var function = (Function?)null;
		var is_constructor = descriptor.Name == Keywords.INIT.Identifier;

		if (is_constructor)
		{
			function = new Constructor(type, Modifier.DEFAULT, start, end);
		}
		else
		{
			function = new Destructor(type, Modifier.DEFAULT, start, end);
		}

		function.Parameters.AddRange(descriptor.GetParameters(function));
		function.Blueprint.AddRange(blueprint.Tokens);

		if (is_constructor)
		{
			type.AddConstructor((Constructor)function);
		}
		else
		{
			type.AddDestructor((Destructor)function);
		}

		return new FunctionDefinitionNode(function, descriptor.Position);
	}
}