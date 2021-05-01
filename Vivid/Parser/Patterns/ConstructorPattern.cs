using System.Collections.Generic;
using System.Linq;

public class ConstructorPattern : Pattern
{
	public const int PRIORITY = 21;

	private const int HEAD = 0;

	// Pattern: init/deinit (...) [\n] {...}
	public ConstructorPattern() : base
	(
		TokenType.FUNCTION
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		// Constructors and destructors must be inside a type
		if (!context.IsType) return false;

		// Ensure the function matches either a constructor or a destructor
		var descriptor = tokens[HEAD].To<FunctionToken>();

		if (descriptor.Name != Keywords.INIT.Identifier && descriptor.Name != Keywords.DEINIT.Identifier)
		{
			return false;
		}

		// Optionally consume a line ending
		Consume(state, TokenType.END | TokenType.OPTIONAL);

		// Try to consume curly brackets
		if (Consume(state, out Token? brackets, TokenType.CONTENT))
		{
			return brackets!.Is(ParenthesisType.CURLY_BRACKETS);
		}

		// Try to consume a heavy arrow operator
		return Consume(state, out Token? arrow, TokenType.OPERATOR) && arrow!.Is(Operators.HEAVY_ARROW);
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		var blueprint = tokens.Last().Is(ParenthesisType.CURLY_BRACKETS) ? tokens.Last().To<ContentToken>().Tokens : null;

		var function = (Function?)null;
		var descriptor = tokens[HEAD].To<FunctionToken>();
		var type = (Type)context;

		var start = descriptor.Position;
		var end = tokens.Last().Is(ParenthesisType.CURLY_BRACKETS) ? tokens.Last().To<ContentToken>().End : null;

		if (descriptor.Name == Keywords.INIT.Identifier)
		{
			function = new Constructor(type, Modifier.DEFAULT, start, end);
			function.Parameters.AddRange(descriptor.GetParameters(function));
			function.DeclareSelfPointer();
		}
		else
		{
			function = new Destructor(type, Modifier.DEFAULT, start, end);
			function.Parameters.AddRange(descriptor.GetParameters(function));
			function.DeclareSelfPointer();
		}

		if (blueprint == null)
		{
			blueprint = new List<Token>();

			if (!Common.ConsumeBlock(function, state, blueprint))
			{
				throw Errors.Get(descriptor.Position, "Expected a short function body");
			}

			tokens.AddRange(blueprint);
		}

		function.Blueprint.AddRange(blueprint);

		if (descriptor.Name == Keywords.INIT.Identifier)
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