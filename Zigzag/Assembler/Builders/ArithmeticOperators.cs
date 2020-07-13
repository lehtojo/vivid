using System;

public static class ArithmeticOperators
{
	public static Result Build(Unit unit, IncrementNode node)
	{
		return BuildIncrementOperation(unit, node);
	}

	public static Result Build(Unit unit, DecrementNode node)
	{
		return BuildDecrementOperation(unit, node);
	}

	public static Result BuildNegate(Unit unit, NegateNode node)
	{
		return new NegateInstruction(unit, References.Get(unit, node.Target)).Execute();
	}

	public static Result Build(Unit unit, OperatorNode node)
	{
		var operation = node.Operator;
		
		if (Equals(operation, Operators.ADD))
		{
			return BuildAdditionOperator(unit, node);
		}
		if (Equals(operation, Operators.SUBTRACT))
		{
			return BuildSubtractionOperator(unit, node);
		}
		if (Equals(operation, Operators.MULTIPLY))
		{
			return BuildMultiplicationOperator(unit, node);
		}
		if (Equals(operation, Operators.DIVIDE))
		{
			return BuildDivisionOperator(unit, false, node);
		}
		if (Equals(operation, Operators.MODULUS))
		{
			return BuildDivisionOperator(unit, true, node);
		}
		if (Equals(operation, Operators.ASSIGN_ADD))
		{
			return BuildAdditionOperator(unit, node, true);
		}
		if (Equals(operation, Operators.ASSIGN_SUBTRACT))
		{
			return BuildSubtractionOperator(unit, node, true);
		}
		if (Equals(operation, Operators.ASSIGN_MULTIPLY))
		{
			return BuildMultiplicationOperator(unit, node, true);
		}
		if (Equals(operation, Operators.ASSIGN_DIVIDE))
		{
			return BuildDivisionOperator(unit, false, node, true);
		}
		if (Equals(operation, Operators.ASSIGN_MODULUS))
		{
			return BuildDivisionOperator(unit, true, node, true);
		}
		if (Equals(operation, Operators.ASSIGN))
		{
			return BuildAssignOperator(unit, node);
		}
		if (Equals(operation, Operators.BITWISE_AND) || Equals(operation, Operators.BITWISE_XOR) || Equals(operation, Operators.BITWISE_OR))
		{
			return BuildBitwiseOperator(unit, node);
		}

		throw new ArgumentException("Node not implemented yet");
	}

	private static Result BuildAdditionOperator(Unit unit, OperatorNode operation, bool assigns = false)
	{   
		var left = References.Get(unit, operation.Left, assigns ? AccessMode.WRITE : AccessMode.READ);
		var right = References.Get(unit, operation.Right);

		var number_type = operation.GetType()!.To<Number>().Type;

		return new AdditionInstruction(unit, left, right, number_type, assigns).Execute();
	}

	private static Result BuildIncrementOperation(Unit unit, IncrementNode increment)
	{   
		var left = References.Get(unit, increment.Object, AccessMode.WRITE);
		var right = References.Get(unit, new NumberNode(Assembler.Size.ToFormat(false), 1L));
		
		var number_type = increment.Object.GetType()!.To<Number>().Type;

		return new AdditionInstruction(unit, left, right, number_type, true).Execute();
	}

	private static Result BuildDecrementOperation(Unit unit, DecrementNode decrement)
	{   
		var left = References.Get(unit, decrement.Object, AccessMode.WRITE);
		var right = References.Get(unit, new NumberNode(Assembler.Size.ToFormat(false), 1L));
		
		var number_type = decrement.Object.GetType()!.To<Number>().Type;

		return new SubtractionInstruction(unit, left, right, number_type, true).Execute();
	}

	private static Result BuildSubtractionOperator(Unit unit, OperatorNode operation, bool assigns = false)
	{
		var left = References.Get(unit, operation.Left, assigns ? AccessMode.WRITE : AccessMode.READ);
		var right = References.Get(unit, operation.Right);
		
		var number_type = operation.GetType()!.To<Number>().Type;

		return new SubtractionInstruction(unit, left, right, number_type, assigns).Execute();
	}

	/// <summary>
	/// Returns whether the node represents a object located in memory
	private static bool IsComplexDestination(Node node)
	{
		return node.Is(NodeType.VARIABLE_NODE) && !node.To<VariableNode>().Variable.IsPredictable ||
					node.Is(NodeType.LINK_NODE) || node.Is(NodeType.OFFSET_NODE);
	}

	private static Result BuildMultiplicationOperator(Unit unit, OperatorNode operation, bool assigns = false)
	{
		var left = References.Get(unit, operation.Left, assigns ? AccessMode.WRITE : AccessMode.READ);
		var right = References.Get(unit, operation.Right);
		
		var number_type = operation.GetType()!.To<Number>().Type;

		var result = new MultiplicationInstruction(unit, left, right, number_type, assigns).Execute();

		if (IsComplexDestination(operation.Left) && assigns)
		{
			return new MoveInstruction(unit, left, result).Execute();
		}

		return result;
	}

	private static Result BuildDivisionOperator(Unit unit, bool modulus, OperatorNode operation, bool assigns = false)
	{
		var left = References.Get(unit, operation.Left, assigns ? AccessMode.WRITE : AccessMode.READ);
		var right = References.Get(unit, operation.Right);
		
		var number_type = operation.GetType()!.To<Number>().Type;

		var result = new DivisionInstruction(unit, modulus, left, right, number_type, assigns).Execute();

		if (IsComplexDestination(operation.Left) && assigns)
		{
			return new MoveInstruction(unit, left, result).Execute();
		}

		return result;
	}

	private static Result BuildAssignOperator(Unit unit, OperatorNode node) 
	{
		var left = References.Get(unit, node.Left, AccessMode.WRITE);
		var right = References.Get(unit, node.Right);
		
		if (node.Left.Is(NodeType.VARIABLE_NODE) && node.Left.To<VariableNode>().Variable.IsPredictable)
		{
			var variable = node.Left.To<VariableNode>().Variable;

			var instruction = new SetVariableInstruction(unit, variable, right);
			instruction.Value.Metadata.Attach(new VariableAttribute(variable));

			return instruction.Execute();
		}

		// Externally used variables need an immediate update 
		return new MoveInstruction(unit, left, right).Execute();
	}

	private static Result BuildBitwiseOperator(Unit unit, OperatorNode operation)
	{
		var left = References.Get(unit, operation.Left, AccessMode.READ);
		var right = References.Get(unit, operation.Right);
		
		var number_type = operation.GetType()!.To<Number>().Type;

		if (operation.Operator == Operators.BITWISE_AND)
		{
			return BitwiseInstruction.And(unit, left, right, number_type).Execute();
		}
		if (operation.Operator == Operators.BITWISE_XOR)
		{
			return BitwiseInstruction.Xor(unit, left, right, number_type).Execute();
		}
		if (operation.Operator == Operators.BITWISE_OR)
		{
			return BitwiseInstruction.Or(unit, left, right, number_type).Execute();
		}

		throw new InvalidOperationException("Tried to build bitwise operation from a node which didn't represent bitwise operation");
	}

	private static void GetDivisionConstants(int divider, int bits)
	{
		throw new NotImplementedException("Constant division optimization not implemented yet");
	}
}