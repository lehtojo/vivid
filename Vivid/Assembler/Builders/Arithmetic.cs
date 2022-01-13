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
		if (Equals(operation, Operators.ATOMIC_EXCHANGE_ADD))
		{
			return BuildAtomicExchangeAdd(unit, node);
		}
		if (operation.Type == OperatorType.COMPARISON || operation.Type == OperatorType.LOGIC)
		{
			throw new InvalidOperationException("Found a boolean value which should have been already outlined");
		}

		throw new ArgumentException($"Operator node is not implemented '{operation.Identifier}'");
	}

	/// <summary>
	/// Builds a manual condition
	/// </summary>
	public static Result BuildCondition(Unit unit, Condition condition)
	{
		return new CompareInstruction(unit, condition.Left, condition.Right).Execute();
	}

	/// <summary>
	/// Builds a left shift operation which can not assign
	/// </summary>
	public static Result BuildShiftLeft(Unit unit, OperatorNode shift)
	{
		var left = References.Get(unit, shift.Left);
		var right = References.Get(unit, shift.Right);

		return BitwiseInstruction.ShiftLeft(unit, left, right, Assembler.Format).Execute();
	}

	/// <summary>
	/// Builds a right shift operation which can not assign
	/// </summary>
	public static Result BuildShiftRight(Unit unit, OperatorNode shift)
	{
		var left = References.Get(unit, shift.Left);
		var right = References.Get(unit, shift.Right);

		return BitwiseInstruction.ShiftRight(unit, left, right, Assembler.Format, shift.Left.GetType().Format.IsUnsigned()).Execute();
	}

	/// <summary>
	/// Builds an exchange-add operator
	/// </summary>
	public static Result BuildAtomicExchangeAdd(Unit unit, OperatorNode operation)
	{
		var left = References.Get(unit, operation.Left);
		var right = References.Get(unit, operation.Right);
		var format = operation.GetType().To<Number>().Type;

		return new AtomicExchangeAdditionInstruction(unit, left, right, format).Execute();
	}

	/// <summary>
	/// Builds a not operation which can not assign and work with booleans as well
	/// </summary>
	public static Result BuildNot(Unit unit, NotNode node)
	{
		var type = node.Object.GetType();

		if (Primitives.IsPrimitive(type, Primitives.BOOL))
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
		var is_decimal = node.GetType().Format.IsDecimal();

		if (Assembler.IsX64 && is_decimal)
		{
			// Define a constant which negates decimal values
			var negator_constant = BitConverter.GetBytes(0x8000000000000000).Concat(BitConverter.GetBytes(0x8000000000000000)).ToArray();

			var negator = new Result(new ConstantDataSectionHandle(negator_constant), Format.INT128);

			return BitwiseInstruction.Xor(unit, References.Get(unit, node.Object), negator, Format.DECIMAL).Execute();
		}

		return SingleParameterInstruction.Negate(unit, References.Get(unit, node.Object), is_decimal).Execute();
	}

	/// <summary>
	/// Builds an addition operation which can assign the result if specified
	/// </summary>
	private static Result BuildAdditionOperator(Unit unit, OperatorNode operation, bool assigns = false)
	{
		var access = assigns ? AccessMode.WRITE : AccessMode.READ;

		var left = References.Get(unit, operation.Left, access);
		var right = References.Get(unit, operation.Right);
		var number_type = operation.GetType().To<Number>().Type;

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
		var number_type = operation.GetType().To<Number>().Type;

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
		var number_type = operation.GetType().To<Number>().Type;

		var result = new MultiplicationInstruction(unit, left, right, number_type, assigns).Execute();

		return result;
	}

	/// <summary>
	/// Returns whether a division with non power of two divisor possible with the specified division node
	/// </summary>
	private static bool IsNonPowerOfTwoIntegerDivisionPossible(OperatorNode operation)
	{
		var format = operation.GetType().To<Number>().Type;

		// 1. This algorithm is only responsible for integer divisions
		// 2. This divisor must be a constant integer
		if (format.IsDecimal() || !operation.Right.Is(NodeType.NUMBER))
		{
			return false;
		}

		// NOTE: The value must be an integer since the format of the division is integer here
		var constant = (long)operation.Right.To<NumberNode>().Value;

		// This algorithm handles positive divisors which can not be expressed as power of two
		return constant > 0 && !Common.IsPowerOfTwo(constant);
	}

	/// <summary>
	/// Builds a division or remainder operation which can assign the result if specified
	/// </summary>
	private static Result BuildDivisionOperator(Unit unit, bool modulus, OperatorNode operation, bool assigns = false)
	{
		var is_unsigned = operation.Left.GetType().Format.IsUnsigned();
		var format = operation.GetType().To<Number>().Type;

		if (Assembler.IsX64 && !is_unsigned && !modulus && IsNonPowerOfTwoIntegerDivisionPossible(operation))
		{
			return BuildConstantDivision(unit, operation.Left, (long)operation.Right.To<NumberNode>().Value, assigns);
		}

		var access = assigns ? AccessMode.WRITE : AccessMode.READ;

		var left = References.Get(unit, operation.Left, access);
		var right = References.Get(unit, operation.Right);

		var result = new DivisionInstruction(unit, modulus, left, right, format, assigns, is_unsigned).Execute();

		return result;
	}

	/// <summary>
	/// Tries to determine the local variable the specified node and its result represent
	/// </summary>
	private static Variable? TryGetLocalVariable(Unit unit, Node node, Result result)
	{
		var local = unit.GetValueOwner(result);
		if (local != null) return local;
		if (result.Value.Is(HandleInstanceType.STACK_VARIABLE)) return result.Value.To<StackVariableHandle>().Variable;
		if (node.Is(NodeType.VARIABLE) && node.To<VariableNode>().Variable.IsPredictable) return node.To<VariableNode>().Variable;
		return null;
	}

	/// <summary>
	/// Builds an assignment operation
	/// </summary>
	private static Result BuildAssignOperator(Unit unit, OperatorNode node)
	{
		var left = References.Get(unit, node.Left, AccessMode.WRITE);
		var right = References.Get(unit, node.Right);

		var local = TryGetLocalVariable(unit, node.Left, left);

		// Check if the destination represents a local variable and ensure the assignment is not conditional
		/// NOTE: Disposable packs must be assigned to intermediate variables so that their values can be extracted even in debug mode
		if (node.Condition == null && local != null && (!Assembler.IsDebuggingEnabled || right.Value.Instance == HandleInstanceType.DISPOSABLE_PACK))
		{
			return new SetVariableInstruction(unit, local, right).Execute();
		}

		// Externally used variables need an immediate update 
		return new MoveInstruction(unit, left, right, node.Condition).Execute();
	}

	/// <summary>
	/// Builds bitwise operations such as AND, XOR and OR which can assign the result if specified
	/// </summary>
	private static Result BuildBitwiseOperator(Unit unit, OperatorNode operation, bool assigns = false)
	{
		var left = References.Get(unit, operation.Left, assigns ? AccessMode.WRITE : AccessMode.READ);
		var right = References.Get(unit, operation.Right);

		var number_type = operation.GetType().To<Number>().Type;
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
	/// Returns the index of the last bit set to one
	/// </summary>
	private static int GetLastSetBitIndex(ulong value)
	{
		for (var i = 63; i >= 0; i--)
		{
			if ((value & (1UL << i)) != 0) return i;
		}

		throw new ApplicationException("Value must not be zero");
	}

	private static Result BuildConstantDivision(Unit unit, Node left, long divisor, bool assigns = false)
	{
		var access = assigns ? AccessMode.WRITE : AccessMode.READ;

		// Retrieve the destination
		var destination = References.Get(unit, left, access);
		var dividend = destination;

		// Multiply the variable with the reciprocal of the divisor
		var reciprocal = new Uint128(1UL, 0UL) / new Uint128((ulong)Math.Abs(divisor));
		var padding = Math.Min(63 - GetLastSetBitIndex(reciprocal.Low) - 1, 7);

		if (padding > 0)
		{
			// Remove the padding
			reciprocal = new Uint128(1UL << padding, 0UL) / new Uint128((ulong)Math.Abs(divisor));
		}

		reciprocal.Low += 1;

		var multiplication = (Result?)null;

		if (Assembler.IsArm64)
		{
			multiplication = new MultiplicationInstruction(unit, dividend, new Result(new ConstantHandle((long)reciprocal.Low), Assembler.Format), Assembler.Format, false).Execute();
		}
		else
		{
			multiplication = new LongMultiplicationInstruction(unit, dividend, new Result(new ConstantHandle((long)reciprocal.Low), Assembler.Format), left.GetType().Format.IsUnsigned()).Execute();
		}

		// The following offset fixes the result of the division when the result is negative by setting the offset's value to one if the result is negative, otherwise zero
		var offset = BitwiseInstruction.ShiftRight(unit, multiplication, new Result(new ConstantHandle(63L), Assembler.Format), multiplication.Format, true).Execute();

		if (padding > 0)
		{
			// Shift the result to the right by the padding
			BitwiseInstruction.ShiftRight(unit, multiplication, new Result(new ConstantHandle((long)padding), Assembler.Format), Assembler.Format, false, true).Execute();
		}

		// Fix the division by adding the offset to the multiplication
		var addition = new AdditionInstruction(unit, offset, multiplication, multiplication.Format, false).Execute();

		// Assign the result if needed
		if (assigns)
		{
			var local = TryGetLocalVariable(unit, left, destination);

			if (local != null && !Assembler.IsDebuggingEnabled)
			{
				return new SetVariableInstruction(unit, local, addition).Execute();
			}

			return new MoveInstruction(unit, destination, addition).Execute();
		}

		return addition;
	}
}