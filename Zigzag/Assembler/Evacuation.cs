using System.Collections.Generic;

public class Evacuation
{
	public List<Value> Values { get; private set; } = new List<Value>();
	public bool IsNecessary => Values.Count > 0;

	/**
     * Appends instructions to evacuate values
     * @param instructions Where instructions should be appended
     */
	public void Start(Instructions instructions)
	{
		foreach (Value value in Values)
		{
			instructions.Append(new Instruction($"push {value.GetRegister()}"));
		}
	}

	/**
     * Restores evacuated values to registers
     * @param instructions Where instructions should be appended
     */
	public void Restore(Unit unit, Instructions instructions)
	{
		for (int i = Values.Count - 1; i >= 0; i--)
		{
			Value value = Values[i];
			Register register = value.GetRegister();

			if (!register.IsCritical)
			{
				register.Attach(value);
			}
			else
			{
				register = unit.GetNextRegister();
				register.Attach(value);
			}

			instructions.Append(new Instruction($"pop {register}"));
		}
	}
}