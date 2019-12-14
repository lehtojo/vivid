using System.Collections.Generic;

public class Evacuation
{
	public List<Value> Values { get; private set; } = new List<Value>();
	public bool IsNecessary => Values.Count > 0;

	/**
     * Appends instructions to evacuate values
     * @param instructions Where instructions should be appended
     */
	public void Start(Unit unit, Instructions instructions)
	{
		var stack = unit.Stack;

		foreach (var value in Values)
		{
			stack.Push(instructions, value);
			//instructions.Append(new Instruction($"push {value.GetRegister()}"));
		}

		var fpu = unit.Fpu;

		if (fpu.Elements.Count > 0)
		{
			instructions.Append(fpu.Save(unit));
		}
	}

	/**
     * Restores evacuated values to registers
     * @param instructions Where instructions should be appended
     */
	public void Restore(Unit unit, Instructions instructions)
	{
		var fpu = unit.Fpu;

		if (fpu.Elements.Count > 0)
		{
			instructions.Append(fpu.Restore(unit));
		}

		var stack = unit.Stack;

		for (int i = Values.Count - 1; i >= 0; i--)
		{
			var value = Values[i];
			var register = value.GetRegister();

			if (!register.IsCritical)
			{
				register.Attach(value);
			}
			else
			{
				register = unit.GetNextRegister();
				register.Attach(value);
			}

			stack.Pop(instructions, new RegisterReference(register));

			//instructions.Append(new Instruction($"pop {register}"));
		}
	}
}