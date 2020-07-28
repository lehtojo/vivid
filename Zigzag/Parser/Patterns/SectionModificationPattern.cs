using System.Collections.Generic;

public class SectionModificationPattern : Pattern
{
	public const int SECTION = 0;
	public const int OBJECT = 2;

	public const int PRIORITY = 0;
	
	// Example: $section [\n] $variable/$function/$type
	public SectionModificationPattern() : base
	(
		TokenType.DYNAMIC,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.DYNAMIC
	) {}

	public override bool Passes(Context context, List<Token> tokens)
	{
		if (!tokens[SECTION].To<DynamicToken>().Node.Is(NodeType.SECTION_NODE))
		{
			return false;
		}

		var target = tokens[OBJECT].To<DynamicToken>().Node;

		if (target.Is(NodeType.VARIABLE_NODE, NodeType.FUNCTION_DEFINITION_NODE, NodeType.TYPE_NODE))
		{
			return true;
		}

		return target.Is(NodeType.OPERATOR_NODE) && 
					target.To<OperatorNode>().Operator == Operators.ASSIGN &&
					target.To<OperatorNode>().Left is VariableNode x && x.Variable.Category == VariableCategory.MEMBER;
	}

	public override Node? Build(Context context, List<Token> tokens)
	{
		var section = tokens[SECTION].To<DynamicToken>().Node.To<SectionNode>();

		switch (tokens[OBJECT].To<DynamicToken>().Node)
		{
			case VariableNode x:
				x.Variable.Modifiers = section.Modifiers;
				section.Add(x);
				return section;

			case FunctionDefinitionNode y:
				y.Function.Modifiers = section.Modifiers;
				section.Add(y);
				return section;

			case TypeNode z:
				z.Type.Modifiers = section.Modifiers;
				section.Add(z);
				return section;

			case OperatorNode w:
				w.Left.To<VariableNode>().Variable.Modifiers = section.Modifiers;
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