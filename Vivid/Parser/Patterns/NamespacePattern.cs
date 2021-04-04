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
		if (!tokens.First().Is(Keywords.NAMESPACE))
		{
			return false;
		}

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

	private static Type CreateNamespace(Context context, List<Token> tokens)
	{
		var components = new List<string>();

		for (var i = NAME; i < tokens.Count; i += 2)
		{
			if (!tokens[i].Is(TokenType.IDENTIFIER))
			{
				break;
			}

			var name = tokens[i].To<IdentifierToken>().Value;
			var type = context.GetType(name);

			context = type != null ? type : new Type(context, name, Modifier.DEFAULT | Modifier.STATIC, tokens.First().Position);
		}

		return (Type)context;
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		var result = CreateNamespace(context, tokens);
		
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

		return new TypeNode(result, blueprint, result.Position);
	}
}