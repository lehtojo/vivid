using System;

public class Arrays
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
	private static string Combine(Reference @object, Reference index, int stride)
	{
		return string.Format("[{0}+{1}*{2}]", ToString(@object), ToString(index), stride);
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
		Instructions instructions = new Instructions();

		Reference[] operands = References.Get(unit, instructions, node.Left, node.Right, ReferenceType.VALUE, ReferenceType.VALUE);

		Reference left = operands[0];
		Reference right = operands[1];

		Register register = unit.GetNextRegister();

		Type type = Types.UNKNOWN;

		try
		{
			type = node.GetContext();
		}
		catch
		{
			Console.Error.WriteLine("ERROR: Couldn't resolve array operation return type");
			return null;
		}

		Size stride = GetStride(type);

		instructions.Append(Memory.Clear(unit, register, false));
		instructions.Append("lea {0}, {1}", register, Arrays.Combine(left, right, stride.Bytes));

		if (reference == ReferenceType.REGISTER || reference == ReferenceType.VALUE)
		{
			Instructions move = Memory.ToRegister(unit, new MemoryReference(register, 0, stride));
			instructions.Append(move);

			Value value = Value.GetOperation(move.Reference.GetRegister(), stride);

			return instructions.SetReference(value);
		}

		register.Attach(Value.GetOperation(register, stride));

		instructions.SetReference(new MemoryReference(register, 0, stride));

		return instructions;
	}
}