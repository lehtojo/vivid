using System.Collections.Generic;

public class ModifierPattern : Pattern
{
	public const int MODIFIER = 0;
	public const int VARIABLE = 1;

	public const int PRIORITY = 1;
	
	// Example: (public static) name
	public ModifierPattern() : base
	(
		TokenType.KEYWORD,
		TokenType.DYNAMIC
	) {}

	public override bool Passes(Context context, List<Token> tokens)
	{
		var modifier = tokens[MODIFIER].To<KeywordToken>();

		if (modifier.Keyword.Type != KeywordType.ACCESS_MODIFIER)
		{
			return false;
		}

		var variable = tokens[VARIABLE].To<DynamicToken>();

		return variable.Node.Is(NodeType.VARIABLE_NODE);
	}

	public override Node? Build(Context context, List<Token> tokens)
	{
		var modifiers = tokens[MODIFIER].To<KeywordToken>().Keyword.To<AccessModifierKeyword>().Modifier;
		var variable = tokens[VARIABLE].To<DynamicToken>().Node.To<VariableNode>().Variable;

		variable.Modifiers = modifiers;

		return null;
	}

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}
}