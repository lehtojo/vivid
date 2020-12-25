using System;
using System.Linq;

public static class Arithmetic
{
	/// <summary>
	/// Builds the specified operator node
	/// </summary>
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
			unit.TryAppendPosition(node);
			return BuildAdditionOperator(unit, node, true);
		}
		if (Equals(operation, Operators.ASSIGN_SUBTRACT))
		{
			unit.TryAppendPosition(node);
			return BuildSubtractionOperator(unit, node, true);
		}
		if (Equals(operation, Operators.ASSIGN_MULTIPLY))
		{
			unit.TryAppendPosition(node);
			return BuildMultiplicationOperator(unit, node, true);
		}
		if (Equals(operation, Operators.ASSIGN_DIVIDE))
		{
			unit.TryAppendPosition(node);
			return BuildDivisionOperator(unit, false, node, true);
		}
		if (Equals(operation, Operators.ASSIGN_MODULUS))
		{
			unit.TryAppendPosition(node);
			return BuildDivisionOperator(unit, true, node, true);
		}
		if (Equals(operation, Operators.ASSIGN))
		{
			unit.TryAppendPosition(node);
			return BuildAssignOperator(unit, node);
		}
		if (Equals(operation, Operators.BITWISE_AND) || Equals(operation, Operators.BITWISE_XOR) || Equals(operation, Operators.BITWISE_OR))
		{
			return BuildBitwiseOperator(unit, node);
		}
		if (Equals(operation, Operators.ASSIGN_AND) || Equals(operation, Operators.ASSIGN_XOR) || Equals(operation, Operators.ASSIGN_OR))
		{
			unit.TryAppendPosition(node);
			return BuildBitwiseOperator(unit, node, true);
		}
		if (Equals(operation, Operators.SHIFT_LEFT))
		{
			return BuildShiftLeft(unit, node);
		}
		if (Equals(operation, Operators.SHIFT_RIGHT))
		{
			return BuildShiftRight(unit, node);
		}
		if (operation.Type == OperatorType.COMPARISON || operation.Type == OperatorType.LOGIC)
		{
			throw new InvalidOperationException("Found a boolean value which should have been already outlined");
		}
		
		throw new ArgumentException($"Operator node is not implemented '{operation.Identifier}'");
	}

	/// <summary>
	/// Builds a left shift operation which can not assign
	/// </summary>
	public static Result BuildShiftLeft(Unit unit, OperatorNode shift)
	{
		var left = References.Get(unit, shift.Left, AccessMode.READ);
		var right = References.Get(unit, shift.Right, AccessMode.READ);
		
		return BitwiseInstruction.ShiftLeft(unit, left, right, Assembler.Format).Execute();
	}

	/// <summary>
	/// Builds a right shift operation which can not assign
	/// </summary>
	public static Result BuildShiftRight(Unit unit, OperatorNode shift)
	{
		var left = References.Get(unit, shift.Left, AccessMode.READ);
		var right = References.Get(unit, shift.Right, AccessMode.READ);
		
		return BitwiseInstruction.ShiftRight(unit, left, right, Assembler.Format).Execute();
	}

	/// <summary>
	/// Builds a not operation which can not assign and work with booleans as well
	/// </summary>
	public static Result BuildNot(Unit unit, NotNode node)
	{
		if (node.Object.GetType() == Types.BOOL)
		{
			var value = References.Get(unit, node.Object);

			return BitwiseInstruction.Xor(unit, value, new Result(new ConstantHandle(1L), Assembler.Format), value.Format).Execute();
		}

		return SingleParameterInstruction.Not(unit, References.Get(unit, node.Object)).Execute();
	}

	/// <summary>
	/// Builds a negation operation which can not assign
	/// </summary>
	public static Result BuildNegate(Unit unit, NegateNode node)
	{
		if (Assembler.IsX64 && node.GetType() == Types.DECIMAL)
		{
			// Define a constant which negates decimal values
			var negator_constant = BitConverter.GetBytes(0x8000000000000000).Concat(BitConverter.GetBytes(0x8000000000000000)).ToArray();

			var negator = new Result(new ConstantDataSectionHandle(negator_constant), Format.INT128);

			return BitwiseInstruction.Xor(unit, References.Get(unit, node.Object), negator, Format.DECIMAL).Execute();
		}

		return SingleParameterInstruction.Negate(unit, References.Get(unit, node.Object), node.GetType() == Types.DECIMAL).Execute();
	}

	/// <summary>
	/// Builds an addition operation which can assign the result if specified
	/// </summary>
	private static Result BuildAdditionOperator(Unit unit, OperatorNode operation, bool assigns = false)
	{
		var access = assigns ? AccessMode.WRITE : AccessMode.READ;

		var left = References.Get(unit, operation.Left, access);
		var right = References.Get(unit, operation.Right);
		var number_type = operation.GetType()!.To<Number>().Type;

		var result = new AdditionInstruction(unit, left, right, number_type, assigns).Execute();

		return result;
	}

	/// <summary>
	/// Builds a subtraction operation which can assign the result if specified
	/// </summary>
	private static Result BuildSubtractionOperator(Unit unit, OperatorNode operation, bool assigns = false)
	{
		var access = assigns ? AccessMode.WRITE : AccessMode.READ;

		var left = References.Get(unit, operation.Left, access);
		var right = References.Get(unit, operation.Right);
		var number_type = operation.GetType()!.To<Number>().Type;

		var result = new SubtractionInstruction(unit, left, right, number_type, assigns).Execute();

		return result;
	}

	/// <summary>
	/// Builds a multiplication operation which can assign the result if specified
	/// </summary>
	private static Result BuildMultiplicationOperator(Unit unit, OperatorNode operation, bool assigns = false)
	{
		var access = assigns ? AccessMode.WRITE : AccessMode.READ;

		var left = References.Get(unit, operation.Left, access);
		var right = References.Get(unit, operation.Right);
		var number_type = operation.GetType()!.To<Number>().Type;

		var result = new MultiplicationInstruction(unit, left, right, number_type, assigns).Execute();

		return result;
	}

	/// <summary>
	/// Builds a division or remainder operation which can assign the result if specified
	/// </summary>
	private static Result BuildDivisionOperator(Unit unit, bool modulus, OperatorNode operation, bool assigns = false)
	{
		if (Assembler.IsX64 && !modulus && operation.Right.Is(NodeType.NUMBER) && operation.Right.To<NumberNode>().Value is long divisor && !IsPowerOfTwo(divisor))
		{
			return BuildConstantDivision(unit, operation.Left, divisor, assigns);
		}

		var access = assigns ? AccessMode.WRITE : AccessMode.READ;

		var left = References.Get(unit, operation.Left, access);
		var right = References.Get(unit, operation.Right);
		var format = operation.GetType()!.To<Number>().Type;
		var is_unsigned = operation.Left.GetType() is Number number ? number.IsUnsigned : true;

		var result = new DivisionInstruction(unit, modulus, left, right, format, assigns, is_unsigned).Execute();

		return result;
	}

	/// <summary>
	/// Builds an assignment operation
	/// </summary>
	private static Result BuildAssignOperator(Unit unit, OperatorNode node)
	{
		// NOTE: The right side should be loaded first since mathematical analysis uses this order
		var right = References.Get(unit, node.Right);
		var left = References.Get(unit, node.Left, AccessMode.WRITE);

		if (node.Left.Is(NodeType.VARIABLE) && node.Left.To<VariableNode>().Variable.IsPredictable && !Assembler.IsDebuggingEnabled)
		{
			var variable = node.Left.To<VariableNode>().Variable;

			if (IsAliasAssignment(node.Right))
			{
				// The assignment is an alias assignment, so the alias variable should be duplicated
				right = new DuplicateInstruction(unit, right).Execute();
			}

			var instruction = new SetVariableInstruction(unit, variable, right);
			instruction.Value.Metadata.Attach(new VariableAttribute(variable));

			return instruction.Execute();
		}

		// Externally used variables need an immediate update 
		return new MoveInstruction(unit, left, right).Execute();
	}

	/// <summary>
	/// Builds bitwise operations such as AND, XOR and OR which can assign the result if specified
	/// </summary>
	private static Result BuildBitwiseOperator(Unit unit, OperatorNode operation, bool assigns = false)
	{
		var left = References.Get(unit, operation.Left, assigns ? AccessMode.WRITE : AccessMode.READ);
		var right = References.Get(unit, operation.Right);

		var number_type = operation.GetType()!.To<Number>().Type;
		var result = (Result?)null;

		if (operation.Is(Operators.BITWISE_AND) || operation.Is(Operators.ASSIGN_AND))
		{
			result = BitwiseInstruction.And(unit, left, right, number_type, assigns).Execute();
		}
		if (operation.Is(Operators.BITWISE_XOR) || operation.Is(Operators.ASSIGN_XOR))
		{
			result = BitwiseInstruction.Xor(unit, left, right, number_type, assigns).Execute();
		}
		if (operation.Is(Operators.BITWISE_OR) || operation.Is(Operators.ASSIGN_OR))
		{
			result = BitwiseInstruction.Or(unit, left, right, number_type, assigns).Execute();
		}

		if (result == null)
		{
			throw new InvalidOperationException("Tried to build bitwise operation from a node which did not represent bitwise operation");
		}

		return result;
	}

	/// <summary>
	/// Returns whether a variable is being assigned with another variable
	/// </summary>
	private static bool IsAliasAssignment(Node value)
	{
		if (value.Is(NodeType.VARIABLE) && value.To<VariableNode>().Variable.IsPredictable)
		{
			return true;
		}

		return value.Is(NodeType.CAST) && value.GetLeftWhile(i => i.Is(NodeType.CAST)) is VariableNode x && x.Variable.IsPredictable;
	}

	/// <summary>
	/// Returns whether the node represents a object located in memory
	/// </summary>
	private static bool IsComplexDestination(Node node)
	{
		return node.Is(NodeType.VARIABLE) && !node.To<VariableNode>().Variable.IsPredictable ||
				 node.Is(NodeType.LINK) || node.Is(NodeType.OFFSET);
	}

	/// <summary>
	/// Returns whether the specified integer fullfills the following equation:
	/// x = 2^y where y is an integer constant
	/// </summary>
	private static bool IsPowerOfTwo(long x)
	{
		return (x & (x - 1)) == 0;
	}

	private static long GetDivisorReciprocal(long divisor)
	{
		if (divisor == 1)
		{
			return 1;
		}

		var fraction = (decimal)1 / divisor;
		var result = (long)0;

		for (var i = 0; i < 64; i++)
		{
			fraction *= 2;

			if (fraction >= 1)
			{
				result |= 1L << (63 - i);
				fraction -= (int)fraction;
			}
		}

		return result + 1;
	}

	private static Result BuildConstantDivision(Unit unit, Node left, long divisor, bool assigns = false)
	{
		var access = assigns ? AccessMode.WRITE : AccessMode.READ;

		// Retrieve the destination
		var destination = References.Get(unit, left, access);
		var dividend = destination;

		if (assigns)
		{
			dividend = new DuplicateInstruction(unit, destination).Execute();
		}

		// Multiply the variable with the divisor's reciprocal
		var reciprocal = new ConstantHandle(GetDivisorReciprocal(divisor));
		var multiplication = (Result?)null;

		if (Assembler.IsArm64)
		{
			multiplication = new MultiplicationInstruction(unit, dividend, new Result(reciprocal, Assembler.Format), Assembler.Format, false).Execute();
		}
		else
		{
			multiplication = new LongMultiplicationInstruction(unit, dividend, new Result(reciprocal, Assembler.Format), Assembler.Format).Execute();
		}

		// The following offset fixes the result of the division when the result is negative by setting the offset's value to one if the result is negative, otherwise zero
		var offset = BitwiseInstruction.ShiftRight(unit, multiplication, new Result(new ConstantHandle(63L), Assembler.Format), multiplication.Format).Execute();

		// Fix the division by adding the offset to the multiplication
		var addition = new AdditionInstruction(unit, offset, multiplication, multiplication.Format, false).Execute();

		// Assign the result if needed
		if (assigns)
		{
			/// NOTE: When a predictable variable is being assigned it must not be under nodes which hide it
			if (left.Is(NodeType.VARIABLE) && left.To<VariableNode>().Variable.IsPredictable)
			{
				var variable = left.To<VariableNode>().Variable;
				
				var instruction = new SetVariableInstruction(unit, variable, addition);
				instruction.Value.Metadata.Attach(new VariableAttribute(variable));

				return instruction.Execute();
			}

			return new MoveInstruction(unit, destination, addition).Execute();
		}

		return addition;
	}
}