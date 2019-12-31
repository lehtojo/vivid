using System.Collections.Generic;
using System;

public class Comparison
{
	private const string COMPARISON = "cmp";

	private static readonly Dictionary<Operator, string> Jumps = new Dictionary<Operator, string>();

	static Comparison()
	{
		Jumps.Add(Operators.GREATER_THAN, "jg");
		Jumps.Add(Operators.GREATER_OR_EQUAL, "jge");
		Jumps.Add(Operators.LESS_THAN, "jl");
		Jumps.Add(Operators.LESS_OR_EQUAL, "jle");
		Jumps.Add(Operators.EQUALS, "je");
		Jumps.Add(Operators.NOT_EQUALS, "jne");
	}

	private static string GetComparisonJump(ComparisonOperator operation)
	{
		return Jumps[operation];
	}

	private static bool IsComplex(Node node)
	{
		return !(node is OperatorNode x && x.Operator is ComparisonOperator);
	}

	private static void BuildLogicalComparisonJump(Unit unit, Instructions instructions, OperatorNode node, Label success, Label failure, bool fallthrough)
	{
		if (node.Operator == Operators.AND)
		{
			var intermediate = IsComplex(node.Left) ? new Label(unit.NextLabel) : success;

			// Append the left side of the AND-operator
			Jump(unit, instructions, node.Left, true, intermediate, failure, false);

			if (intermediate != success)
			{
				instructions.Label(intermediate.GetName());
			}

			// Append the right side of the AND-operator
			Jump(unit, instructions, node.Right, fallthrough, success, failure, fallthrough);
		}
		else
		{
			var intermediate = IsComplex(node.Left) ? new Label(unit.NextLabel) : failure;

			// Append instructions for the left side of the OR-operator
			Jump(unit, instructions, node.Left, false, success, intermediate, false);

			if (intermediate != failure)
			{
				instructions.Label(intermediate.GetName());
			}

			// Append instructions for the right side of the OR-operator
			Jump(unit, instructions, node.Right, true, success, failure, fallthrough);
		}
	}

	public static void Jump(Unit unit, Instructions instructions, Node condition, bool invert, Label success, Label failure, bool independent = true)
	{
		if (condition.GetNodeType() == NodeType.CONTENT_NODE)
		{
			Jump(unit, instructions, condition.First, invert, success, failure, independent);
			return;
		}
		else if (!(condition is OperatorNode))
		{
			throw new NotImplementedException("Comparison isn't fully implemented yet");
		}

		var node = condition as OperatorNode;

		if (node.Operator.Type == OperatorType.LOGIC)
		{
			BuildLogicalComparisonJump(unit, instructions, node, success, failure, independent);
		}
		else if (node.Operator.Type == OperatorType.COMPARISON)
		{
			// Get references to both sides
			References.Get(unit, instructions, node.Left, node.Right, ReferenceType.VALUE, ReferenceType.READ, out Reference left, out Reference right);

			// Compare both sides
			instructions.Append(new Instruction(COMPARISON, left, right, left.GetSize()));

			// Get the comparsion operator and the destination label
			var operation = node.Operator as ComparisonOperator;
			var label = success;

			if (invert)
			{
				operation = operation.Counterpart;
				label = failure;
			}

			var instruction = GetComparisonJump(operation);
			instructions.Append($"{instruction} {label.GetName()}");
		}

		//unit.Step(instructions)
	}
}