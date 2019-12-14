using System;
using System.Collections.Generic;
using System.Text;

public static class Decimals
{
	private static bool IsInteger(Node node)
	{
		if (node is IType type)
		{
			return type.GetType() == Types.NORMAL;
		}

		throw new InvalidOperationException("BUG: Performing floating point arithmetics with an uncontextable node!");
	}

	public static Instructions Build(Unit unit, OperatorNode node, ReferenceType type)
	{
		// Build instructions for calculating the result
		var instructions = Build(unit, node.Operator, node.Left, node.Right);

		if (type != ReferenceType.VALUE)
		{
			return instructions;
		}

		return instructions;
		/*var stack = unit.Stack;
		var fpu = unit.Fpu;

		// Reserve stack memory for moving taking the result from FPU
		stack.Reserve(instructions);

		// Pop the result from FPU to the top of the stack
		fpu.Pop(instructions, IsInteger(node));

		// Get a register for storing the result
		var register = unit.GetNextRegister();
		instructions.Append(Memory.Clear(unit, register, false));

		stack.Pop(instructions, new RegisterReference(register));

		return instructions.SetReference(Value.GetOperation(register, Size.DWORD));*/
	}

	public static Reference[] GetReferences(Unit unit, Instructions program, Node left, Node right)
	{
		var references = new Reference[2];

		if (left.GetNodeType() == NodeType.VARIABLE_NODE)
		{
			var variable = left as VariableNode;
			var element = unit.Fpu.Find(variable.Variable);

			if (element != null)
			{
				references[0] = new FpuStackReference(element);
			}
		}

		if (right.GetNodeType() == NodeType.VARIABLE_NODE)
		{
			var variable = right as VariableNode;
			var element = unit.Fpu.Find(variable.Variable);

			if (element != null)
			{
				references[1] = new FpuStackReference(element);
			}
		}

		if (references[0] == null && references[1] == null)
		{
			return References.Get(unit, program, left, right, ReferenceType.READ, ReferenceType.READ);
		}

		if (references[0] == null)
		{
			var instructions = References.Get(unit, left, ReferenceType.READ);
			program.Append(instructions);

			references[0] = instructions.Reference;
		}

		if (references[1] == null)
		{
			var instructions = References.Get(unit, right, ReferenceType.READ);
			program.Append(instructions);

			references[1] = instructions.Reference;
		}

		return references;
	}

	private static FpuStackReference Upload(Unit unit, Instructions instructions, Node node, Reference source)
	{
		if (source.GetType() != LocationType.FPU)
		{
			if (source.GetType() != LocationType.MEMORY)
			{
				source = unit.Stack.Push(instructions, source);
			}

			var reference = unit.Fpu.Push(instructions, source, IsInteger(node));

			if (node is VariableNode variable)
			{
				reference.Element.Metadata = variable.Variable;
			}
			else if (node is NumberNode number)
			{
				reference.Element.Metadata = number.Value;
			}

			return reference;
		}

		return source as FpuStackReference;
	}

	public static Instructions Build(Unit unit, Operator operation, Node first, Node last)
	{
		var instructions = new Instructions();
		var references = GetReferences(unit, instructions, first, last);

		// Upload both operands to the FPU stack
		var left = Upload(unit, instructions, first, references[0]);
		var right = Upload(unit, instructions, last, references[1]);

		var result = unit.Fpu.Perform(instructions, operation, left, right);

		return instructions.SetReference(result);
	}
}