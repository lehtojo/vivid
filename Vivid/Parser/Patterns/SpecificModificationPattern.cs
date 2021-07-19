using System.Collections.Generic;

public class SpecificModificationPattern : Pattern
{
	public const int MODIFIER = 0;
	public const int OBJECT = 2;

	// Pattern: $modifiers [\n] $variable/$function/$type
	public SpecificModificationPattern() : base
	(
		TokenType.KEYWORD,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.DYNAMIC
	) { }

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		var modifier = tokens[MODIFIER].To<KeywordToken>();
		if (modifier.Keyword.Type != KeywordType.MODIFIER) return false;

		var node = tokens[OBJECT].To<DynamicToken>().Node;
		return node.Is(NodeType.CONSTRUCTION, NodeType.VARIABLE, NodeType.FUNCTION_DEFINITION, NodeType.TYPE) || (node.Is(NodeType.LINK) && node.Right.Is(NodeType.CONSTRUCTION));
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		var modifiers = tokens[MODIFIER].To<KeywordToken>().Keyword.To<ModifierKeyword>().Modifier;
		var destination = tokens[OBJECT].To<DynamicToken>().Node;

		switch (destination.Instance)
		{
			case NodeType.VARIABLE:
			{
				var variable = destination.To<VariableNode>().Variable;
				variable.Modifiers = Modifier.Combine(variable.Modifiers, modifiers);
				return destination;
			}

			case NodeType.FUNCTION_DEFINITION:
			{
				if (Flag.Has(Modifier.IMPORTED, modifiers))
				{
					throw Errors.Get(tokens[MODIFIER].Position, "Can not add external modifier to the function definition since it is a definition");
				}

				var function = destination.To<FunctionDefinitionNode>().Function;
				function.Modifiers = Modifier.Combine(function.Modifiers, modifiers);
				return destination;
			}

			case NodeType.TYPE:
			{
				var type = destination.To<TypeNode>().Type;
				type.Modifiers = Modifier.Combine(type.Modifiers, modifiers);
				return destination;
			}

			case NodeType.CONSTRUCTION:
			{
				var construction = destination.To<ConstructionNode>();
				construction.IsStackAllocated = Flag.Has(modifiers, Modifier.INLINE);
				return destination;
			}

			case NodeType.LINK:
			{
				var construction = destination.Right.To<ConstructionNode>();
				construction.IsStackAllocated = Flag.Has(modifiers, Modifier.INLINE);
				return destination;
			}

			default: return tokens[OBJECT].To<DynamicToken>().Node;
		}
	}

	public override int GetPriority(List<Token> tokens)
	{
		return Parser.PRIORITY_ALL;
	}
}