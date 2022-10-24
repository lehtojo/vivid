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
	)
	{ Priority = Parser.PRIORITY_ALL; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		var modifier = tokens[MODIFIER].To<KeywordToken>();
		if (modifier.Keyword.Type != KeywordType.MODIFIER) return false;

		var node = tokens[OBJECT].To<DynamicToken>().Node;
		return node.Is(NodeType.CONSTRUCTION, NodeType.VARIABLE, NodeType.FUNCTION_DEFINITION, NodeType.TYPE_DEFINITION) || (node.Is(NodeType.LINK) && node.Right.Is(NodeType.CONSTRUCTION));
	}

	public override Node? Build(Context context, ParserState state, List<Token> tokens)
	{
		var modifiers = tokens[MODIFIER].To<KeywordToken>().Keyword.To<ModifierKeyword>().Modifier;
		var destination = tokens[OBJECT].To<DynamicToken>().Node;

		switch (destination.Instance)
		{
			case NodeType.VARIABLE:
			{
				var variable = destination.To<VariableNode>().Variable;
				variable.Modifiers = Modifier.Combine(variable.Modifiers, modifiers);

				// Static variables are categorized as global variables
				if (Flag.Has(modifiers, Modifier.STATIC)) { variable.Category = VariableCategory.GLOBAL; }
				break;
			}

			case NodeType.FUNCTION_DEFINITION:
			{
				if (Flag.Has(Modifier.IMPORTED, modifiers))
				{
					throw Errors.Get(tokens[MODIFIER].Position, "Can not add modifier 'import' to a function definition");
				}

				var function = destination.To<FunctionDefinitionNode>().Function;
				function.Modifiers = Modifier.Combine(function.Modifiers, modifiers);
				break;
			}

			case NodeType.TYPE_DEFINITION:
			{
				var type = destination.To<TypeDefinitionNode>().Type;
				type.Modifiers = Modifier.Combine(type.Modifiers, modifiers);
				break;
			}

			case NodeType.CONSTRUCTION:
			{
				var construction = destination.To<ConstructionNode>();
				construction.IsStackAllocated = Flag.Has(modifiers, Modifier.INLINE);
				break;
			}

			case NodeType.LINK:
			{
				var construction = destination.Right.To<ConstructionNode>();
				construction.IsStackAllocated = Flag.Has(modifiers, Modifier.INLINE);
				break;
			}

			default: break;
		}

		return destination;
	}
}