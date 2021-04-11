using System.Collections.Generic;
using System.Linq;

public class NamespacePattern : Pattern
{
	public const int PRIORITY = 23;

	public const int NAME = 1;
	
	// Pattern: namespace $1.$2. ... .$n [\n] [{...}]
	public NamespacePattern() : base
	(
		TokenType.KEYWORD,
		TokenType.IDENTIFIER
	) {}

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		// Require the first token to be the namespace keyword
		if (!tokens.First().Is(Keywords.NAMESPACE)) return false;

		while (true)
		{
			// Continue if the next operator is a dot operator
			var next = Peek(state);

			if (next == null || !next.Is(Operators.DOT)) break;

			// Consume the dot operator
			Consume(state);

			// The next token must be an identifier
			if (!Consume(state, TokenType.IDENTIFIER)) return false;
		}
		
		// Optionally consume a line ending
		Consume(state, TokenType.END | TokenType.OPTIONAL);

		// Optionally consume curly brackets
		Consume(state, TokenType.CONTENT | TokenType.OPTIONAL);

		return tokens.Last().Is(TokenType.NONE) || tokens.Last().Is(ParenthesisType.CURLY_BRACKETS);
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		// Collect all the parent types and ensure they all are namespaces
		var types = context.GetParentTypes();

		if (types.Any(i => !i.IsStatic)) throw Errors.Get(tokens.First().Position, "Can not create a namespace under a type");
		
		List<Token>? blueprint;

		if (tokens.Last().Is(TokenType.NONE))
		{
			// Collect all tokens after the name
			blueprint = state.Tokens.Skip(state.End).ToList();
			state.Formatted.AddRange(blueprint);
			state.End += blueprint.Count;
		}
		else
		{
			// Get the blueprint from the the curly brackets
			blueprint = state.Formatted.Last().To<ContentToken>().Tokens;
		}

		// Find the line ending token which is after the name
		var end = tokens.FindIndex(i => i.Is(TokenType.END) || i.Is(TokenType.NONE));
		var name = tokens.GetRange(NAME, end - NAME);

		return new NamespaceNode(name, blueprint);
	}
}