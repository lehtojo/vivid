using System.Collections.Generic;

public class SectionModificationPattern : Pattern
{
	public const int SECTION = 0;
	public const int OBJECT = 2;

	// Pattern: $section [\n] $object
	public SectionModificationPattern() : base
	(
		TokenType.DYNAMIC,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.DYNAMIC
	)
	{ Priority = 0; IsConsumable = false; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		// Require the first consumed token to represent a modifier section
		if (tokens[SECTION].To<DynamicToken>().Node.Instance != NodeType.SECTION) return false;

		// Require the next token to represent a variable, function definition, or type definition
		var target = tokens[OBJECT].To<DynamicToken>().Node;
		var type = target.Instance;

		if (type == NodeType.TYPE_DEFINITION || type == NodeType.FUNCTION_DEFINITION || type == NodeType.VARIABLE) return true;

		// Allow member variable assignments as well
		if (!target.Is(Operators.ASSIGN)) return false;

		// Require the destination operand to be a member variable
		return target.First!.Instance == NodeType.VARIABLE && target.First.To<VariableNode>().Variable.IsMember;
	}

	public override Node? Build(Context context, ParserState state, List<Token> tokens)
	{
		// Load the section and target node
		var section = tokens[SECTION].To<DynamicToken>().Node.To<SectionNode>();
		var target = tokens[OBJECT].To<DynamicToken>().Node;

		if (target.Instance == NodeType.VARIABLE)
		{
			var variable = target.To<VariableNode>().Variable;
			var modifiers = variable.Modifiers;
			variable.Modifiers = Modifier.Combine(modifiers, section.Modifiers);
			section.Add(target);

			// Static variables are categorized as global variables
			if (Flag.Has(section.Modifiers, Modifier.STATIC)) { variable.Category = VariableCategory.GLOBAL; }
		}
		else if (target.Instance == NodeType.FUNCTION_DEFINITION)
		{
			var function = target.To<FunctionDefinitionNode>().Function;
			var modifiers = function.Modifiers;
			function.Modifiers = Modifier.Combine(modifiers, section.Modifiers);
			section.Add(target);
		}
		else if (target.Instance == NodeType.TYPE_DEFINITION)
		{
			var type = target.To<TypeDefinitionNode>().Type;
			var modifiers = type.Modifiers;
			type.Modifiers = Modifier.Combine(modifiers, section.Modifiers);
			section.Add(target);
		}
		else if (target.Instance == NodeType.OPERATOR)
		{
			var variable = target.To<OperatorNode>().First!.To<VariableNode>().Variable;
			var modifiers = variable.Modifiers;
			variable.Modifiers = Modifier.Combine(modifiers, section.Modifiers);
			section.Add(target);
		}

		return section;
	}
}