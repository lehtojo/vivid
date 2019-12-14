public static class Call
{
	public static Instructions Build(Unit unit, FunctionNode node)
	{
		return Call.Build(unit, null, node);
	}

	public static Instructions Build(Unit unit, Reference @object, string function, Size result, params Reference[] parameters)
	{
		var instructions = new Instructions();
		var evacuation = Memory.Evacuate(unit);

		var stack = unit.Stack;

		// When the function is for example in the middle of an expression, some critical values must be saved before the call
		if (evacuation.IsNecessary)
		{
			evacuation.Start(unit, instructions);
		}

		var memory = 0;

		// Pass the function parameters
		for (int i = parameters.Length - 1; i >= 0; i--)
		{
			var parameter = parameters[i];
			stack.Push(instructions, parameter);

			memory += parameter.GetSize().Bytes;
		}

		// Pass the object pointer
		if (@object != null)
		{
			stack.Push(instructions, @object);
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
			stack.Shrink(instructions, memory);
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

	public static Instructions Build(Unit unit, Reference @object, FunctionImplementation function, Node parameters)
	{
		var instructions = new Instructions();
		var evacuation = Memory.Evacuate(unit);
		var stack = unit.Stack;

		// When the function is for example in the middle of an expression, some critical values must be saved before the call
		if (evacuation.IsNecessary)
		{
			evacuation.Start(unit, instructions);
		}

		var memory = 0;

		var iterator = parameters.Last;

		// Pass the function parameters
		while (iterator != null)
		{
			var parameter = References.Read(unit, iterator);
			instructions.Append(parameter);

			stack.Push(instructions, parameter.Reference);

			memory += parameter.Reference.GetSize().Bytes;

			iterator = iterator.Previous;
		}

		// Pass the object pointer
		if (function.IsMember)
		{
			if (@object != null)
			{
				stack.Push(instructions, @object);
			}
			else
			{
				var register = unit.GetObjectPointer();

				if (register != null)
				{
					stack.Push(instructions, new RegisterReference(register, Size.DWORD));
				}
				else
				{
					stack.Push(instructions, References.GetObjectPointer(unit));
				}
			}

			memory += References.GetObjectPointer(unit).GetSize().Bytes;
		}

		// Unit must be reset since function may affect the registers
		unit.Reset();

		// Get the return type
		var result = function.ReturnType;

		// Call the function
		instructions.Append(new Instruction($"call {function.Metadata.GetFullname()}"));

		if (result != null)
		{
			instructions.SetReference(Value.GetOperation(unit.EAX, Size.Get(result.Size)));
		}

		// Remove parameters from the stack, if needed
		if (memory > 0)
		{
			unit.Stack.Shrink(instructions, memory);
		}

		// Restore saved values from stack, if needed
		if (evacuation.IsNecessary)
		{
			evacuation.Restore(unit, instructions);
		}

		return instructions;
	}
}