public class Instruction
{
	private string Assembly;

	public static Instruction Unsafe(string command, Reference left, Reference right, Size size)
	{
		return new Instruction($"{command} {left.Peek(size)}, {right.Peek(size)}");
	}

	public Instruction(string command, Reference left, Reference right, Size size)
	{
		Assembly = $"{command} {left.Use(size)}, {right.Use(size)}";
	}

	public Instruction(string command, Reference operand)
	{
		Assembly = $"{command} {operand.Use(operand.GetSize())}";
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