public class BitwiseInstruction : DualParameterInstruction
{
	private const string AND_INSTRUCTION = "and";
	private const string XOR_INSTRUCTION = "xor";
	private const string OR_INSTRUCTION = "or";

	public string Instruction { get; private set; }
	public bool IsSafe { get; set; } = true;

	public static BitwiseInstruction And(Unit unit, Result first, Result second, Format format)
	{
		return new BitwiseInstruction(unit, AND_INSTRUCTION, first, second, format);
	}

	public static BitwiseInstruction Xor(Unit unit, Result first, Result second, Format format)
	{
		return new BitwiseInstruction(unit, XOR_INSTRUCTION, first, second, format);
	}

	public static BitwiseInstruction Or(Unit unit, Result first, Result second, Format format)
	{
		return new BitwiseInstruction(unit, OR_INSTRUCTION, first, second, format);
	}

	private BitwiseInstruction(Unit unit, string instruction, Result first, Result second, Format format) : base(unit, first, second, format) 
	{
		Instruction = instruction;
		Description = "Executes bitwise XOR-operation between the operands";
	}

	public override void OnBuild()
	{
		if (First.IsMemoryAddress)
		{
			Build(
				Instruction,
				Assembler.Size,
				new InstructionParameter(
					First,
					ParameterFlag.DESTINATION | (IsSafe ? ParameterFlag.NONE : ParameterFlag.WRITE_ACCESS),
					HandleType.MEMORY
				),
				new InstructionParameter(
					Second,
					ParameterFlag.NONE,
					HandleType.CONSTANT,
					HandleType.REGISTER
				)
			);
		}
		else
		{
			Build(
				Instruction,
				Assembler.Size,
				new InstructionParameter(
					First,
					ParameterFlag.DESTINATION | (IsSafe ? ParameterFlag.NONE : ParameterFlag.WRITE_ACCESS),
					HandleType.REGISTER
				),
				new InstructionParameter(
					Second,
					ParameterFlag.NONE,
					HandleType.CONSTANT,
					HandleType.REGISTER,
					HandleType.MEMORY
				)
			);
		}
	}

	public override Result GetDestinationDependency()
	{
		return First;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.BITWISE;
	}
}