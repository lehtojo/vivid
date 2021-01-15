using System.Collections.Generic;

public class SpecificModificationPattern : Pattern
{
	public const int MODIFIER = 0;
	public const int OBJECT = 2;

	public const int PRIORITY = 1;

	// Example: (public static) [\n] $variable/$function/$type
	public SpecificModificationPattern() : base
	(
		TokenType.KEYWORD,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.DYNAMIC
	)
	{ }

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		var modifier = tokens[MODIFIER].To<KeywordToken>();

		if (modifier.Keyword.Type != KeywordType.MODIFIER)
		{
			return false;
		}

		return tokens[OBJECT].To<DynamicToken>().Node.Is(NodeType.VARIABLE, NodeType.FUNCTION_DEFINITION, NodeType.TYPE);
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		var modifiers = tokens[MODIFIER].To<KeywordToken>().Keyword.To<ModifierKeyword>().Modifier;
		var destination = tokens[OBJECT].To<DynamicToken>().Node;

		switch (destination.GetNodeType())
		{
			case NodeType.VARIABLE:
			{
				destination.To<VariableNode>().Variable.Modifiers = modifiers;
				return destination;
			}

			case NodeType.FUNCTION_DEFINITION:
			{
				if (Flag.Has(Modifier.EXTERNAL, modifiers))
				{
					throw Errors.Get(tokens[MODIFIER].Position, "Can not add external modifier to the function definition since it is a definition");
				}

				destination.To<FunctionDefinitionNode>().Function.Modifiers = modifiers;
				return destination;
			}

			case NodeType.TYPE:
			{
				destination.To<TypeNode>().Type.Modifiers = modifiers;
				return destination;
			}

			default: return tokens[OBJECT].To<DynamicToken>().Node;
		}
	}

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}
}