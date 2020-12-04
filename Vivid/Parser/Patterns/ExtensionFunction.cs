using System.Collections.Generic;
using System.Linq;

public class ExtensionFunction : Pattern
{
	public const int PRIORITY = 20;

	private const int DESTINATION = 0;
	private const int OPERATOR = 1;
	private const int FUNCTION = 2;
	private const int BODY = 4;

	// Examples:
	// Player.spawn(position: Vector) [\n] {...}
	public ExtensionFunction() : base
	(
	   TokenType.IDENTIFIER,
	   TokenType.OPERATOR,
	   TokenType.FUNCTION,
	   TokenType.END | TokenType.OPTIONAL,
	   TokenType.CONTENT
	)
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		if (!tokens[OPERATOR].Is(Operators.DOT) || !tokens[BODY].Is(ParenthesisType.CURLY_BRACKETS))
		{
			return false;
		}

		var name = tokens[DESTINATION].To<IdentifierToken>().Value;

		if (!context.IsTypeDeclared(name))
		{
			throw Errors.Get(tokens[DESTINATION].Position, $"Type '{name}' is not defined");
		}

		return true;
	}

	public override Node? Build(Context environment, List<Token> tokens)
	{
		var destination = environment.GetType(tokens[DESTINATION].To<IdentifierToken>().Value);

		if (destination == null)
		{
			throw Errors.Get(tokens[DESTINATION].Position, "Could not resolve the destination of the extension function");
		}

		var descriptor = tokens[FUNCTION].To<FunctionToken>();

		var function = new Function(destination, AccessModifier.PUBLIC, descriptor.Name, tokens[BODY].To<ContentToken>().Tokens);
		function.Position = tokens.First().Position;
		function.Parameters.AddRange(descriptor.GetParameters(function));

		destination.Declare(function);

		return new FunctionDefinitionNode(function, destination.Position);
	}
}