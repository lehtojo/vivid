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
		TokenType.DYNAMIC,
		TokenType.DYNAMIC
	) {}

	public override bool Passes(Context context, List<Token> tokens)
	{
		var inheritant = tokens[INHERITANT].To<DynamicToken>().Node;

		if (!inheritant.Is(NodeType.TYPE_NODE) || inheritant.To<TypeNode>().IsDefinition)
		{
			return false;
		}

		var inheritor = tokens[INHERITOR].To<DynamicToken>().Node;

		return inheritor.Is(NodeType.TYPE_NODE) && inheritor.To<TypeNode>().IsDefinition;
	}

	public override Node? Build(Context context, List<Token> tokens)
	{
		var inheritant = tokens[INHERITANT].To<DynamicToken>().Node.To<TypeNode>().Type;
		var inheritor = tokens[INHERITOR].To<DynamicToken>().Node.To<TypeNode>().Type;

		inheritor.Supertypes.Add(inheritant);
	
		return tokens[INHERITOR].To<DynamicToken>().Node;
	}

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}
}