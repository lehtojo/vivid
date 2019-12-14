using System.Collections.Generic;

public class Comparison
{
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

	public static Instructions Jump(Unit unit, OperatorNode node, bool invert, string label)
	{
		var instructions = new Instructions();

		References.Get(unit, instructions, node.Left, node.Right, ReferenceType.VALUE, ReferenceType.READ, out Reference left, out Reference right);

		instructions.Append(new Instruction("cmp", left, right, left.GetSize()));

		unit.Step(instructions);

		var operation = node.Operator as ComparisonOperator;

		if (invert)
		{
			operation = operation.Counterpart;
		}

		var instruction = GetComparisonJump(operation);
		instructions.Append($"{instruction} {label}");

		return instructions;
	}
}