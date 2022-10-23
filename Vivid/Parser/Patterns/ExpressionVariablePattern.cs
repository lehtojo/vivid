using System.Collections.Generic;
using System.Linq;

public class ExpressionVariablePattern : Pattern
{
	public const int ARROW = 1;

	// Pattern: $name => ...
	public ExpressionVariablePattern() : base
	(
		TokenType.IDENTIFIER,
		TokenType.OPERATOR
	)
	{ Priority = 21; IsConsumable = false; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return (context.IsType || context.IsNamespace) && tokens[ARROW].Is(Operators.HEAVY_ARROW);
	}

	public override Node? Build(Context type, ParserState state, List<Token> tokens)
	{
		var name = tokens.First().To<IdentifierToken>();

		// Create function which has the name of the property but has no parameters
		var function = new Function(type, Modifier.DEFAULT, name.Value, name.Position, null);

		var blueprint = new List<Token> { new KeywordToken(Keywords.RETURN, tokens[ARROW].Position)};

		Common.ConsumeBlock(state, blueprint);

		// Save the blueprint
		function.Blueprint.AddRange(blueprint);

		// Finally, declare the function
		type.Declare(function);

		return new FunctionDefinitionNode(function, name.Position);
	}
}