using System;
using System.Linq;
using System.Numerics;

public static class Arithmetic
{
	public static Result Build(Unit unit, IncrementNode node)
	{
		return BuildIncrementOperation(unit, node);
	}

	public static Result Build(Unit unit, DecrementNode node)
	{
		return BuildDecrementOperation(unit, node);
	}

	public static Result BuildNot(Unit unit, NotNode node)
	{
		if (node.Object.GetType() == Types.BOOL)
		{
			var value = References.Get(unit, node.Object);

			return BitwiseInstruction.Xor(unit, value, new Result(new ConstantHandle(1L), Assembler.Format), value.Format).Execute();
		}

		return SingleParameterInstruction.Not(unit, References.Get(unit, node.Object)).Execute();
	}

	public static Result BuildNegate(Unit unit, NegateNode node)
	{
		if (node.GetType() == Types.DECIMAL)
		{
			// Define a constant which negates decimal values
			var negator_constant = BitConverter.GetBytes(0x8000000000000000).Concat(BitConverter.GetBytes(0x8000000000000000)).ToArray();

			var negator = new Result(new ConstantDataSectionHandle(negator_constant), Format.INT128);

			return BitwiseInstruction.Xor(unit, References.Get(unit, node.Object), negator, Format.DECIMAL).Execute();
		}

		return SingleParameterInstruction.Negate(unit, References.Get(unit, node.Object)).Execute();
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
		if (Equals(operation, Operators.ASSIGN_AND) || Equals(operation, Operators.ASSIGN_XOR) || Equals(operation, Operators.ASSIGN_OR))
		{
			return BuildBitwiseOperator(unit, node, true);
		}
		if (operation.Type == OperatorType.COMPARISON || operation.Type == OperatorType.LOGIC)
		{
			throw new InvalidOperationException("Found a boolean value which should have been already outlined");
		}

		throw new ArgumentException("Node not implemented yet");
	}

	private static Result BuildAdditionOperator(Unit unit, OperatorNode operation, bool assigns = false)
	{
		var is_destination_complex = IsComplexDestination(operation.Left) && operation.Left.GetType() == Types.DECIMAL;
		var access = (assigns && !is_destination_complex) ? AccessMode.WRITE : AccessMode.READ;

		var left = References.Get(unit, operation.Left, access);
		var right = References.Get(unit, operation.Right);

		var number_type = operation.GetType()!.To<Number>().Type;

		var result = new AdditionInstruction(unit, left, right, number_type, assigns).Execute();

		if (is_destination_complex && assigns)
		{
			return new MoveInstruction(unit, References.Get(unit, operation.Left, AccessMode.WRITE), result).Execute();
		}

		return result;
	}

	private static Result BuildIncrementOperation(Unit unit, IncrementNode increment)
	{
		var left = References.Get(unit, increment.Object, AccessMode.WRITE);
		var right = References.Get(unit, new NumberNode(Assembler.Size.ToFormat(false), 1L));

		var number_type = increment.Object.GetType()!.To<Number>().Type;

		if (increment.Post)
		{
			// Load the variable to be incremented to a register and then increment the source location
			var result = new DuplicateInstruction(unit, left).Execute();
			unit.Append(new AdditionInstruction(unit, left, right, number_type, true));

			return result;
		}

		// Edit the object first and then copy it since there are situations where not copying would cause an error (for example: ++i + i++)
		unit.Append(new AdditionInstruction(unit, left, right, number_type, true));

		return new DuplicateInstruction(unit, left).Execute();
	}

	private static Result BuildDecrementOperation(Unit unit, DecrementNode decrement)
	{
		var left = References.Get(unit, decrement.Object, AccessMode.WRITE);
		var right = References.Get(unit, new NumberNode(Assembler.Size.ToFormat(false), 1L));

		var number_type = decrement.Object.GetType()!.To<Number>().Type;

		if (decrement.Post)
		{
			// Load the variable to be incremented to a register and then increment the source location
			var result = new DuplicateInstruction(unit, left).Execute();
			unit.Append(new SubtractionInstruction(unit, left, right, number_type, true));

			return result;
		}

		// Edit the object first and then copy it since there are situations where not copying would cause an error (for example: --i + i--)
		unit.Append(new SubtractionInstruction(unit, left, right, number_type, true));

		return new DuplicateInstruction(unit, left).Execute();
	}

	private static Result BuildSubtractionOperator(Unit unit, OperatorNode operation, bool assigns = false)
	{
		var is_destination_complex = IsComplexDestination(operation.Left) && operation.Left.GetType() == Types.DECIMAL;
		var access = (assigns && !is_destination_complex) ? AccessMode.WRITE : AccessMode.READ;

		var left = References.Get(unit, operation.Left, access);
		var right = References.Get(unit, operation.Right);

		var number_type = operation.GetType()!.To<Number>().Type;

		var result = new SubtractionInstruction(unit, left, right, number_type, assigns).Execute();

		if (is_destination_complex && assigns)
		{
			return new MoveInstruction(unit, References.Get(unit, operation.Left, AccessMode.WRITE), result).Execute();
		}

		return result;
	}

	/// <summary>
	/// Returns whether the node represents a object located in memory
	/// </summary>
	private static bool IsComplexDestination(Node node)
	{
		return node.Is(NodeType.VARIABLE) && !node.To<VariableNode>().Variable.IsPredictable ||
				 node.Is(NodeType.LINK) || node.Is(NodeType.OFFSET);
	}

	private static Result BuildMultiplicationOperator(Unit unit, OperatorNode operation, bool assigns = false)
	{
		var is_destination_complex = IsComplexDestination(operation.Left);
		var access = (assigns && !is_destination_complex) ? AccessMode.WRITE : AccessMode.READ;

		var left = References.Get(unit, operation.Left, access);
		var right = References.Get(unit, operation.Right);
		var number_type = operation.GetType()!.To<Number>().Type;

		var result = new MultiplicationInstruction(unit, left, right, number_type, assigns).Execute();

		if (is_destination_complex && assigns)
		{
			return new MoveInstruction(unit, References.Get(unit, operation.Left, AccessMode.WRITE), result).Execute();
		}

		return result;
	}

	private static Result BuildDivisionOperator(Unit unit, bool modulus, OperatorNode operation, bool assigns = false)
	{
		if (!modulus && operation.Right.Is(NodeType.NUMBER) && operation.Right.To<NumberNode>().Value is long divisor && !IsPowerOfTwo(divisor))
		{
			return BuildConstantDivision(unit, operation.Left, divisor, assigns);
		}

		var is_destination_complex = IsComplexDestination(operation.Left);
		var access = (assigns && !is_destination_complex) ? AccessMode.WRITE : AccessMode.READ;

		var right = References.Get(unit, operation.Right);
		var left = References.Get(unit, operation.Left, access);
		var number_type = operation.GetType()!.To<Number>().Type;
		var is_unsigned = operation.Left.GetType() is Number number ? number.IsUnsigned : true;

		var result = new DivisionInstruction(unit, modulus, left, right, number_type, assigns, is_unsigned).Execute();

		if (is_destination_complex && assigns)
		{
			return new MoveInstruction(unit, References.Get(unit, operation.Left, AccessMode.WRITE), result).Execute();
		}

		return result;
	}

	private static Result BuildAssignOperator(Unit unit, OperatorNode node)
	{
		var left = References.Get(unit, node.Left, AccessMode.WRITE);
		var right = References.Get(unit, node.Right);

		if (node.Left.Is(NodeType.VARIABLE) && node.Left.To<VariableNode>().Variable.IsPredictable)
		{
			var variable = node.Left.To<VariableNode>().Variable;

			if (node.Right.Is(NodeType.VARIABLE) && node.Right.To<VariableNode>().Variable.IsPredictable)
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

	private static Result BuildBitwiseOperator(Unit unit, OperatorNode operation, bool assigns = false)
	{
		var left = References.Get(unit, operation.Left, assigns ? AccessMode.WRITE : AccessMode.READ);
		var right = References.Get(unit, operation.Right);

		var number_type = operation.GetType()!.To<Number>().Type;

		if (operation.Is(Operators.BITWISE_AND) || operation.Is(Operators.ASSIGN_AND))
		{
			return BitwiseInstruction.And(unit, left, right, number_type, assigns).Execute();
		}
		if (operation.Is(Operators.BITWISE_XOR) || operation.Is(Operators.ASSIGN_XOR))
		{
			return BitwiseInstruction.Xor(unit, left, right, number_type, assigns).Execute();
		}
		if (operation.Is(Operators.BITWISE_OR) || operation.Is(Operators.ASSIGN_OR))
		{
			return BitwiseInstruction.Or(unit, left, right, number_type, assigns).Execute();
		}

		throw new InvalidOperationException("Tried to build bitwise operation from a node which didn't represent bitwise operation");
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
		var is_destination_complex = IsComplexDestination(left);
		var access = (assigns && !is_destination_complex) ? AccessMode.WRITE : AccessMode.READ;

		// Retrieve the dividend
		var dividend = References.Get(unit, left, access);

		// Multiply the variable with the divisor's reciprocal
		var reciprocal = new ConstantHandle(GetDivisorReciprocal(divisor));
		var multiplication = new LongMultiplicationInstruction(unit, dividend, new Result(reciprocal, Assembler.Format), Assembler.Format).Execute();

		// The following offset fixes the result of the division when the result is negative by setting the offset's value to one if the result is negative, otherwise zero
		var offset = BitwiseInstruction.ShiftRight(unit, multiplication, new Result(new ConstantHandle(63L), Assembler.Format), multiplication.Format).Execute();

		// Fix the division by adding the offset to the multiplication
		var addition = new AdditionInstruction(unit, offset, multiplication, multiplication.Format, false).Execute();

		// Assign the result if needed
		if (is_destination_complex && assigns)
		{
			return new MoveInstruction(unit, References.Get(unit, left, AccessMode.WRITE), addition).Execute();
		}

		return addition;
	}
}