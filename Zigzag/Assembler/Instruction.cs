public class Instruction
{
	private string Assembly;

	public static Instruction Unsafe(string command, Reference left, Reference right, Size size)
	{
		if (right.IsRegister() && size == Size.DWORD)
		{
			return new Instruction($"{command} {left.Peek()}, {right.Peek()}");
		}
		else if (size != Size.DWORD)
		{
			return new Instruction($"{command} {left.Peek(size)}, {right.Peek(size)}");
		}
		else
		{
			return new Instruction($"{command} {left.Peek(size)}, {right.Peek()}");
		}
	}

	public Instruction(string command, Reference left, Reference right, Size size)
	{
		if (right.IsRegister() && size == Size.DWORD)
		{
			Assembly = $"{command} {left.Use()}, {right.Use()}";
		}
		else if (size != Size.DWORD)
		{
			Assembly = $"{command} {left.Use(size)}, {right.Use(size)}";
		}
		else
		{
			Assembly = $"{command} {left.Use(size)}, {right.Use()}";
		}
	}

	public Instruction(string command, Reference operand)
	{
		Assembly = $"{command} {operand.Use(operand.GetSize())}";

		/*if (!operand.IsRegister() && operand.GetSize() == Size.DWORD)
		{
			Assembly = $"{command} {operand.GetSize()} {operand.Use(operand.GetSize())}";
		}
		else
		{
			Assembly = $"{command} {operand.Use(operand.GetSize())}";
		}*/
	}

	public Instruction(string command)
	{
		Assembly = command;
	}

	public override string ToString()
	{
		return Assembly;
	}
}