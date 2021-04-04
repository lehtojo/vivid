using System.Collections.Generic;

public class SectionModificationPattern : Pattern
{
	public const int SECTION = 0;
	public const int OBJECT = 2;

	public const int PRIORITY = 0;

	// Pattern: $section [\n] $variable/$function/$type
	public SectionModificationPattern() : base
	(
		TokenType.DYNAMIC,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.DYNAMIC
	) { }

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		if (!tokens[SECTION].To<DynamicToken>().Node.Is(NodeType.SECTION))
		{
			return false;
		}

		var target = tokens[OBJECT].To<DynamicToken>().Node;

		if (target.Is(NodeType.VARIABLE, NodeType.FUNCTION_DEFINITION, NodeType.TYPE))
		{
			return true;
		}

		return target.Is(NodeType.OPERATOR) && target.Is(Operators.ASSIGN) && target.To<OperatorNode>().Left is VariableNode x && x.Variable.IsMember;
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		var section = tokens[SECTION].To<DynamicToken>().Node.To<SectionNode>();

		switch (tokens[OBJECT].To<DynamicToken>().Node)
		{
			case VariableNode x:
			x.Variable.Modifiers = Modifier.Combine(x.Variable.Modifiers, section.Modifiers);
			section.Add(x);
			return section;

			case FunctionDefinitionNode y:
			y.Function.Modifiers = Modifier.Combine(y.Function.Modifiers, section.Modifiers);
			section.Add(y);
			return section;

			case TypeNode z:
			z.Type.Modifiers = Modifier.Combine(z.Type.Modifiers, section.Modifiers);
			section.Add(z);
			return section;

			case OperatorNode w:
			var variable = w.Left.To<VariableNode>().Variable;
			variable.Modifiers = Modifier.Combine(variable.Modifiers, section.Modifiers);
			section.Add(w);
			return section;

			default: return section;
		}
	}

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}
}