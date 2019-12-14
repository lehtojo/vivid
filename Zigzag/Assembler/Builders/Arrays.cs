using System;

public static class Arrays
{
	/**
     * Converts reference to string format that is compatible with lea instruction
     * @param reference Reference to convert to string
     * @return Reference represented in string format
     */
	private static string ToString(Reference reference)
	{
		switch (reference.GetType())
		{
			case LocationType.MEMORY:
			{
				MemoryReference memory = (MemoryReference)reference;
				return memory.GetContent();
			}

			case LocationType.NUMBER:
			{
				NumberReference number = (NumberReference)reference;
				return number.Value.ToString();
			}

			case LocationType.REGISTER:
			{
				RegisterReference register = (RegisterReference)reference;
				return register.Peek(reference.GetSize());
			}

			case LocationType.VALUE:
			{
				Value value = (Value)reference;
				return Arrays.ToString(value.Reference);
			}

			default:
			{
				Console.Error.WriteLine("ERROR: Unsupported array usage");
				break;
			}
		}

		return "";
	}

	/**
     * Combines two references into one lea calculation
     * @param object Object to offset in memory
     * @param index Index of the element
     * @param stride Element size in bytes
     * @return Memory calculation for lea instruction
     */
	public static string GetOffsetCalculation(Reference start, Reference index, int stride)
	{
		if (stride == 1)
		{
			return string.Format("[{0}+{1}]", ToString(start), ToString(index));
		}
		else
		{
			return string.Format("[{0}+{1}*{2}]", ToString(start), ToString(index), stride);
		}
	}

	/**
     * Returns the stride between elements of the given type
     * @param type Type of the elements
     * @return Stride between the elements in an array
     */
	private static Size GetStride(Type type)
	{
		return type == Types.LINK ? Size.BYTE : Size.Get(type.Size);
	}

	public static Instructions Build(Unit unit, OperatorNode node, ReferenceType reference)
	{
		var instructions = new Instructions();

		References.Get(unit, instructions, node.Left, node.Right, ReferenceType.VALUE, ReferenceType.VALUE, out Reference left, out Reference right);

		/*var operands = References.Get(unit, instructions, node.Left, node.Right, ReferenceType.VALUE, ReferenceType.VALUE);

		var left = operands[0];
		var right = operands[1];*/

		var type = node.GetType();
		var stride = GetStride(type);

		Register register;
		Reference source;

		if (right.GetType() != LocationType.NUMBER)
		{
			var count = unit.UncriticalRegisterCount;
			
			if (count >= 2)
			{
				source = new ComplexOffsetReference(left, right, stride.Bytes);
			}
			else
			{
				register = unit.GetNextRegister();
				instructions.Append(Memory.Clear(unit, register, false));

				var calculation = Arrays.GetOffsetCalculation(left, right, stride.Bytes);
				instructions.Append($"lea {register}, {calculation}");

				source = new MemoryReference(register, 0, stride);
			}
		}
		else
		{
			var index = right as NumberReference;
			var alignment = (int)(stride.Bytes * (long)index.Value);

			source = new MemoryReference(left.GetRegister(), alignment, stride);

			if (reference == ReferenceType.DIRECT)
			{
				return instructions.SetReference(source);
			}
		}

		if (reference == ReferenceType.REGISTER || reference == ReferenceType.VALUE)
		{
			var move = Memory.ToRegister(unit, source);
			instructions.Append(move);

			return instructions.SetReference(Value.GetOperation(move.Reference.GetRegister(), stride));
		}

		instructions.SetReference(source);

		return instructions;
	}
}