using System;
using System.Collections.Generic;

public class InheritancePattern : Pattern
{
	public const int INHERITANT = 0;
	public const int INHERITOR = 1;

	public const int PRIORITY = 1;

	// Pattern: $type $type_definition
	// Example: Enumerable List {T} { ... }
	public InheritancePattern() : base
	(
		TokenType.DYNAMIC | TokenType.IDENTIFIER,
		TokenType.DYNAMIC
	)
	{ }

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		if (tokens[INHERITANT].Is(TokenType.IDENTIFIER))
		{
			var inheritant = tokens[INHERITANT].To<IdentifierToken>().Value;

			if (!context.IsTypeDeclared(inheritant))
			{
				return false;
			}
		}
		else
		{
			var inheritant = tokens[INHERITANT].To<DynamicToken>().Node;

			if (!inheritant.Is(NodeType.TYPE) || inheritant.To<TypeNode>().IsDefinition)
			{
				return false;
			}
		}
		
		var inheritor = tokens[INHERITOR].To<DynamicToken>().Node;

		return inheritor.Is(NodeType.TYPE) && inheritor.To<TypeNode>().IsDefinition;
	}

	private static Type GetInheritantType(Context context, List<Token> tokens)
	{
		if (tokens[INHERITANT].Is(TokenType.IDENTIFIER))
		{
			return context.GetType(tokens[INHERITANT].To<IdentifierToken>().Value) ?? throw new ApplicationException("Could not retrieve inheritant type");
		}

		return tokens[INHERITANT].To<DynamicToken>().Node.To<TypeNode>().Type;
	}

	public override Node? Build(Context context, List<Token> tokens)
	{
		var inheritant = GetInheritantType(context, tokens);
		var inheritor = tokens[INHERITOR].To<DynamicToken>().Node.To<TypeNode>().Type;

		inheritor.Supertypes.Insert(0, inheritant);

		return tokens[INHERITOR].To<DynamicToken>().Node;
	}

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}
}