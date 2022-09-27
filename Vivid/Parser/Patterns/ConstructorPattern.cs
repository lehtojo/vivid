using System.Collections.Generic;
using System.Linq;

public class ConstructorPattern : Pattern
{
	public const int PRIORITY = 23;

	private const int HEADER = 0;

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
		var descriptor = tokens[HEADER].To<FunctionToken>();

		if (descriptor.Name != Keywords.INIT.Identifier && descriptor.Name != Keywords.DEINIT.Identifier) return false;

		// Optionally consume a line ending
		Consume(state, TokenType.END | TokenType.OPTIONAL);

		// Try to consume curly brackets
		if (Consume(state, out Token? brackets, TokenType.PARENTHESIS)) return brackets!.Is(ParenthesisType.CURLY_BRACKETS);

		// Try to consume a heavy arrow operator
		return Consume(state, out Token? arrow, TokenType.OPERATOR) && arrow!.Is(Operators.HEAVY_ARROW);
	}

	/// <summary>
	/// Support member parameters by redirecting their values to corresponding members of the self pointer
	/// Example: init(this.a, this.b) { ... } => init(a, b) { this.a = a \n this.b = b \n ... }
	/// </summary>
	private void ApplyMemberParameters(Function function)
	{
		foreach (var parameter in function.Parameters)
		{
			// Ignore normal parameters
			if (!parameter.IsMemberParameter) continue;

			var position = parameter.Position!;
			var member = parameter.Name.Substring(Function.SELF_POINTER_IDENTIFIER.Length + 1);

			// Create the following pattern using tokens: this.$member = $parameter \n
			function.Blueprint.AddRange(new[]
			{
				new IdentifierToken(Function.SELF_POINTER_IDENTIFIER, position),
				new OperatorToken(Operators.DOT, position),
				new IdentifierToken(member, position),
				new OperatorToken(Operators.ASSIGN, position),
				new IdentifierToken(parameter.Name, position),
				new Token(TokenType.END)
			});
		}
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		var blueprint = tokens.Last().Is(ParenthesisType.CURLY_BRACKETS) ? tokens.Last().To<ParenthesisToken>().Tokens : null;

		var function = (Function?)null;
		var descriptor = tokens[HEADER].To<FunctionToken>();
		var type = (Type)context;

		var start = descriptor.Position;
		var end = tokens.Last().Is(ParenthesisType.CURLY_BRACKETS) ? tokens.Last().To<ParenthesisToken>().End : null;

		if (descriptor.Name == Keywords.INIT.Identifier)
		{
			function = new Constructor(type, Modifier.DEFAULT, start, end);
			function.Parameters.AddRange(descriptor.GetParameters(function));
			function.DeclareSelfPointer();

			ApplyMemberParameters(function);
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

			if (!Common.ConsumeBlock(function, state, blueprint)) throw Errors.Get(descriptor.Position, "Expected a short function body");

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