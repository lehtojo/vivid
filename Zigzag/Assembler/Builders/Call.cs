public class Call
{
	public static Instructions Build(Unit unit, FunctionNode node)
	{
		return Call.Build(unit, null, node);
	}

	public static Instructions Build(Unit unit, Reference @object, string function, Size result, params Reference[] parameters)
	{
		Instructions instructions = new Instructions();
		Evacuation evacuation = Memory.Evacuate(unit);

		// When the function is for example in the middle of an expression, some critical values must be saved before the call
		if (evacuation.IsNecessary)
		{
			evacuation.Start(instructions);
		}

		int memory = 0;

		// Pass the function parameters
		for (int i = parameters.Length - 1; i >= 0; i--)
		{
			Reference parameter = parameters[i];
			instructions.Append(new Instruction("push", parameter));

			memory += parameter.GetSize().Bytes;
		}

		// Pass the object pointer
		if (@object != null)
		{
			instructions.Append(new Instruction("push", @object));
			memory += @object.GetSize().Bytes;
		}

		// Unit must be reset since function may affect the registers
		unit.Reset();

		// Call the function
		instructions.Append(new Instruction(string.Format("call {0}", function)));
		instructions.SetReference(Value.GetOperation(unit.EAX, result));

		// Remove parameters from the stack, if needed
		if (memory > 0)
		{
			instructions.Append("add esp, {0}", memory);
		}

		// Restore saved values from stack, if needed
		if (evacuation.IsNecessary)
		{
			evacuation.Restore(unit, instructions);
		}

		return instructions;
	}

	public static Instructions Build(Unit unit, Reference @object, FunctionNode node)
	{
		return Call.Build(unit, @object, node.Function, node);
	}

	public static Instructions Build(Unit unit, Reference @object, Function function, Node parameters)
	{
		Instructions instructions = new Instructions();
		Evacuation evacuation = Memory.Evacuate(unit);

		// When the function is for example in the middle of an expression, some critical values must be saved before the call
		if (evacuation.IsNecessary)
		{
			evacuation.Start(instructions);
		}

		int memory = 0;

		Node iterator = parameters.Last;

		// Pass the function parameters
		while (iterator != null)
		{
			Instructions parameter = References.Read(unit, iterator);
			instructions.Append(parameter);
			instructions.Append(new Instruction("push", parameter.Reference));

			memory += parameter.Reference.GetSize().Bytes;

			iterator = iterator.Previous;
		}

		// Pass the object pointer
		if (function.IsMember)
		{
			if (@object != null)
			{
				instructions.Append(new Instruction("push", @object));
			}
			else
			{
				Register register = unit.GetObjectPointer();

				if (register != null)
				{
					instructions.Append(new Instruction("push", new RegisterReference(register, Size.DWORD)));
				}
				else
				{
					instructions.Append(new Instruction("push", References.GetObjectPointer(unit)));
				}
			}

			memory += References.GetObjectPointer(unit).GetSize().Bytes;
		}

		// Unit must be reset since function may affect the registers
		unit.Reset();

		// Get the return type
		Type result = function.ReturnType;

		// Call the function
		instructions.Append(new Instruction(string.Format("call {0}", function.Fullname)));

		if (result != null)
		{
			instructions.SetReference(Value.GetOperation(unit.EAX, Size.Get(result.Size)));
		}

		// Remove parameters from the stack, if needed
		if (memory > 0)
		{
			instructions.Append("add esp, {0}", memory);
		}

		// Restore saved values from stack, if needed
		if (evacuation.IsNecessary)
		{
			evacuation.Restore(unit, instructions);
		}

		return instructions;
	}
}