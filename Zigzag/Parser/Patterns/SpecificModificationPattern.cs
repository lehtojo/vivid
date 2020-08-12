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
	) {}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		var modifier = tokens[MODIFIER].To<KeywordToken>();

		if (modifier.Keyword.Type != KeywordType.ACCESS_MODIFIER)
		{
			return false;
		}

		return tokens[OBJECT].To<DynamicToken>().Node.Is(NodeType.VARIABLE_NODE, NodeType.FUNCTION_DEFINITION_NODE, NodeType.TYPE_NODE);
	}

	public override Node? Build(Context context, List<Token> tokens)
	{
		var modifiers = tokens[MODIFIER].To<KeywordToken>().Keyword.To<AccessModifierKeyword>().Modifier;

		switch (tokens[OBJECT].To<DynamicToken>().Node)
		{
			case VariableNode x:
				x.Variable.Modifiers = modifiers;
				return x;

			case FunctionDefinitionNode y:
				y.Function.Modifiers = modifiers;
				return y;

			case TypeNode z:
				z.Type.Modifiers = modifiers;
				return z;

			default: return tokens[OBJECT].To<DynamicToken>().Node;
		}
	}

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}
}