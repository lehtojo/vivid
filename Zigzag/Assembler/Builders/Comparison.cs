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

	private static string GetComparisonJump(ComparisonOperator @operator)
	{
		return Jumps[@operator];
	}

	public static Instructions Jump(Unit unit, OperatorNode node, bool invert, string label)
	{
		Instructions instructions = new Instructions();

		Reference[] operands = References.Get(unit, instructions, node.Left, node.Right, ReferenceType.VALUE, ReferenceType.READ);

		Reference left = operands[0];
		Reference right = operands[1];

		instructions.Append(new Instruction("cmp", left, right, left.GetSize()));

		ComparisonOperator @operator = (ComparisonOperator)node.Operator;

		if (invert)
		{
			@operator = @operator.Counterpart;
		}

		instructions.Append("{0} {1}", GetComparisonJump(@operator), label);

		return instructions;
	}
}