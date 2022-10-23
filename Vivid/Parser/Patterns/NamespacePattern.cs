using System.Collections.Generic;
using System.Linq;

public class NamespacePattern : Pattern
{
	public const int NAME = 1;

	// Pattern: namespace $1.$2. ... .$n [\n] [{...}]
	public NamespacePattern() : base
	(
		TokenType.KEYWORD,
		TokenType.IDENTIFIER
	)
	{ Priority = 23; IsConsumable = false; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		// Require the first token to be the namespace keyword
		if (!tokens.First().Is(Keywords.NAMESPACE)) return false;

		while (true)
		{
			// Continue if the next operator is a dot operator
			var next = state.Peek();

			if (next == null || !next.Is(Operators.DOT)) break;

			// Consume the dot operator
			state.Consume();

			// The next token must be an identifier
			if (!state.Consume(TokenType.IDENTIFIER)) return false;
		}
		
		// Optionally consume a line ending
		state.ConsumeOptional(TokenType.END);

		// Optionally consume curly brackets
		state.ConsumeOptional(TokenType.PARENTHESIS);

		return tokens.Last().Is(TokenType.NONE) || tokens.Last().Is(ParenthesisType.CURLY_BRACKETS);
	}

	public override Node? Build(Context context, ParserState state, List<Token> tokens)
	{
		// Save the end index of the name
		var end = tokens.Count - 2;

		// Collect all the parent types and ensure they all are namespaces
		var types = context.GetParentTypes();

		if (types.Any(i => !i.IsStatic)) throw Errors.Get(tokens.First().Position, "Can not create a namespace inside a normal type");
		
		List<Token>? blueprint;

		if (tokens.Last().Is(TokenType.NONE))
		{
			// Collect all tokens after the name
			blueprint = state.All.Skip(state.End).ToList();
			state.Tokens.AddRange(blueprint);
			state.End += blueprint.Count;
		}
		else
		{
			// Get the blueprint from the the curly brackets
			blueprint = tokens.Last().To<ParenthesisToken>().Tokens;
		}

		// Create the namespace node
		var name = tokens.GetRange(1, end - 1);
		return new NamespaceNode(name, blueprint);
	}
}