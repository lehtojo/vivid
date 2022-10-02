using System;
using System.Collections.Generic;
using System.Linq;

public static class Conditionals
{
	/// <summary>
	/// Builds the body of an if-statement or an else-if-statement
	/// </summary>
	public static Result BuildBody(Unit unit, ScopeNode body)
	{
		// Build the body
		var result = Builders.Build(unit, body);

		// Restore the state after the body
		unit.AddDebugPosition(body.End);
		return result;
	}

	/// <summary>
	/// Builds an if-statement or an else-if-statement
	/// </summary>
	public static Result Build(Unit unit, IfNode statement, Node condition, LabelInstruction end)
	{
		// Set the next label to be the end label if there is no successor since then there wont be any other comparisons
		var interphase = statement.Successor == null ? end.Label : unit.GetNextLabel();

		// Build the nodes around the actual condition by disabling the condition temporarily
		var instance = statement.Condition.Instance;
		statement.Condition.Instance = NodeType.DISABLED;

		Builders.Build(unit, statement.Left);

		statement.Condition.Instance = instance;

		// Jump to the next label based on the comparison
		BuildCondition(unit, condition, interphase);

		// Build the body of this if-statement
		var result = BuildBody(unit, statement.Body);

		// If the body of the if-statement is executed it must skip the potential successors
		if (statement.Successor == null) return result;

		// Skip the next successor from this if-statement's body and add the interphase label
		unit.Add(new JumpInstruction(unit, end.Label));
		unit.Add(new LabelInstruction(unit, interphase));

		// Build the successor
		return Build(unit, statement.Successor, end);
	}

	private static Result Build(Unit unit, Node node, LabelInstruction end)
	{
		unit.AddDebugPosition(node);

		if (node.Is(NodeType.IF, NodeType.ELSE_IF)) return Build(unit, node.To<IfNode>(), node.To<IfNode>().Condition, end);

		var result = BuildBody(unit, node.To<ElseNode>().Body);

		return result;
	}

	public static Result Start(Unit unit, IfNode node)
	{
		var end = new LabelInstruction(unit, unit.GetNextLabel());
		var result = Build(unit, node, end);
		unit.Add(end);

		return result;
	}

	public static void BuildCondition(Unit unit, Node condition, Label failure)
	{
		var success = unit.GetNextLabel();

		var instructions = BuildCondition(unit, condition, success, failure);
		instructions.Add(new LabelInstruction(unit, success));

		BuildConditionInstructions(unit, instructions);
	}

	public static void BuildConditionInstructions(Unit unit, List<Instruction> instructions)
	{
		// Remove all occurrences of the following pattern from the instructions:
		// Jump L0
		// L0:
		for (var i = instructions.Count - 2; i >= 0; i--)
		{
			if (instructions[i].Is(InstructionType.JUMP) && instructions[i + 1].Is(InstructionType.LABEL))
			{
				var jump = instructions[i].To<JumpInstruction>();
				var label = instructions[i + 1].To<LabelInstruction>();

				if (!jump.IsConditional && Equals(jump.Label, label.Label))
				{
					instructions.RemoveAt(i);
				}
			}
		}

		// Replace all occurrences of the following pattern in the instructions:
		// Conditional Jump L0
		// Jump L1
		// L0:
		// =====================================
		// Inverted Conditional Jump L1
		// L0:
		for (var i = instructions.Count - 3; i >= 0; i--)
		{
			if (instructions[i].Is(InstructionType.JUMP) && instructions[i + 1].Is(InstructionType.JUMP) && instructions[i + 2].Is(InstructionType.LABEL))
			{
				var conditional_jump = instructions[i].To<JumpInstruction>();
				var jump = instructions[i + 1].To<JumpInstruction>();
				var label = instructions[i + 2].To<LabelInstruction>();

				if (conditional_jump.IsConditional && !jump.IsConditional && Equals(conditional_jump.Label, label.Label) && !Equals(jump.Label, label.Label))
				{
					conditional_jump.Invert();
					conditional_jump.Label = jump.Label;

					instructions.RemoveAt(i + 1);
				}
			}
		}

		// Remove unused labels
		var labels = instructions.Where(i => i.Is(InstructionType.LABEL)).Select(i => i.To<LabelInstruction>()).ToList();
		var jumps = instructions.Where(i => i.Is(InstructionType.JUMP)).Select(i => i.To<JumpInstruction>());

		foreach (var label in labels)
		{
			// Check if any jump instruction uses the current label
			if (!jumps.Any(i => i.Label == label.Label))
			{
				// Since the label is not used, it can be removed
				instructions.Remove(label);
			}
		}

		// Add all the instructions to the unit
		foreach (var instruction in instructions)
		{
			if (instruction.Is(InstructionType.TEMPORARY_COMPARE))
			{
				instruction.To<TemporaryCompareInstruction>().Add();
			}
			else
			{
				unit.Add(instruction);
			}
		}
	}

	public class TemporaryCompareInstruction : TemporaryInstruction
	{
		private Node? Root { get; }
		private Node Comparison { get; }
		private Node First => Comparison.First!;
		private Node Last => Comparison.Last!;

		public TemporaryCompareInstruction(Unit unit, Node comparison) : base(unit, InstructionType.TEMPORARY_COMPARE)
		{
			Root = null;
			Comparison = comparison;
		}

		public TemporaryCompareInstruction(Unit unit, Node root, Node comparison) : base(unit, InstructionType.TEMPORARY_COMPARE)
		{
			Root = root;
			Comparison = comparison;
		}

		public new void Add()
		{
			if (Root != null)
			{
				// Build the code surrounding the comparison
				var instance = Comparison.Instance;
				Comparison.Instance = NodeType.DISABLED;
				Builders.Build(Unit, Root);
				Comparison.Instance = instance;
			}

			// Build the comparison
			var left = References.Get(Unit, First, AccessMode.READ);
			var right = References.Get(Unit, Last, AccessMode.READ);

			// Compare the two operands
			Unit.Add(new CompareInstruction(Unit, left, right));
		}
	}

	public static List<Instruction> BuildCondition(Unit unit, Node condition, Label success, Label failure)
	{
		if (condition.Is(NodeType.OPERATOR))
		{
			var operation = condition.To<OperatorNode>();
			var type = operation.Operator.Type;

			if (type == OperatorType.LOGICAL)
			{
				return BuildLogicalCondition(unit, operation, success, failure);
			}
			else if (type == OperatorType.COMPARISON)
			{
				return BuildComparison(unit, operation, success, failure);
			}
		}

		if (condition.Is(NodeType.PARENTHESIS))
		{
			return BuildCondition(unit, condition.Last ?? throw new ApplicationException("Encountered an empty parenthesis while building a condition"), success, failure);
		}

		#warning Investigate additional code in the second stage here

		var replacement = new OperatorNode(Operators.NOT_EQUALS, condition.Position);
		condition.Replace(replacement);

		replacement.SetOperands(condition, new NumberNode(Assembler.Format, 0L, replacement.Position));

		return BuildCondition(unit, replacement, success, failure);
	}

	public static List<Instruction> BuildComparison(Unit unit, OperatorNode condition, Label success, Label failure)
	{
		var first_type = condition.Left.GetType();
		var second_type = condition.Right.GetType();
		var unsigned = (first_type.Format.IsDecimal() || second_type.Format.IsDecimal()) || (first_type.Format.IsUnsigned() && second_type.Format.IsUnsigned());

		return new List<Instruction>
		{
			new TemporaryCompareInstruction(unit, condition),
			new JumpInstruction(unit, (ComparisonOperator)condition.Operator, false, !unsigned, success),
			new JumpInstruction(unit, failure)
		};
	}

	public static List<Instruction> BuildLogicalCondition(Unit unit, OperatorNode condition, Label success, Label failure)
	{
		var instructions = new List<Instruction>();
		var interphase = unit.GetNextLabel();

		if (Equals(condition.Operator, Operators.AND))
		{
			instructions.AddRange(BuildCondition(unit, condition.Left, interphase, failure));
			instructions.Add(new LabelInstruction(unit, interphase));
			instructions.AddRange(BuildCondition(unit, condition.Right, success, failure));
		}
		else if (Equals(condition.Operator, Operators.OR))
		{
			instructions.AddRange(BuildCondition(unit, condition.Left, success, interphase));
			instructions.Add(new LabelInstruction(unit, interphase));
			instructions.AddRange(BuildCondition(unit, condition.Right, success, failure));
		}
		else
		{
			throw new ApplicationException("Unsupported logical operator encountered while building a conditional statement");
		}

		return instructions;
	}
}