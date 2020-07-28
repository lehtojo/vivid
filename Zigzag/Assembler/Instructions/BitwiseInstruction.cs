public class BitwiseInstruction : DualParameterInstruction
{
	private const string AND_INSTRUCTION = "and";
	private const string XOR_INSTRUCTION = "xor";
	private const string OR_INSTRUCTION = "or";
	private const string SHIFT_LEFT_INSTRUCTION = "sal";
	private const string SHIFT_RIGHT_INSTRUCTION = "sar";

	public string Instruction { get; private set; }

	public bool Assigns { get; set; } = false;

	public static BitwiseInstruction And(Unit unit, Result first, Result second, Format format, bool assigns = false)
	{
		return new BitwiseInstruction(unit, AND_INSTRUCTION, first, second, format, assigns);
	}

	public static BitwiseInstruction Xor(Unit unit, Result first, Result second, Format format, bool assigns = false)
	{
		return new BitwiseInstruction(unit, XOR_INSTRUCTION, first, second, format, assigns);
	}

	public static BitwiseInstruction Or(Unit unit, Result first, Result second, Format format, bool assigns = false)
	{
		return new BitwiseInstruction(unit, OR_INSTRUCTION, first, second, format, assigns);
	}

	public static BitwiseInstruction ShiftLeft(Unit unit, Result first, Result second, Format format, bool assigns = false)
	{
		return new BitwiseInstruction(unit, SHIFT_LEFT_INSTRUCTION, first, second, format, assigns);
	}

	public static BitwiseInstruction ShiftRight(Unit unit, Result first, Result second, Format format, bool assigns = false)
	{
		return new BitwiseInstruction(unit, SHIFT_RIGHT_INSTRUCTION, first, second, format, assigns);
	}

	private BitwiseInstruction(Unit unit, string instruction, Result first, Result second, Format format, bool assigns) : base(unit, first, second, format) 
	{
		Instruction = instruction;
		Description = "Executes bitwise XOR-operation between the operands";
		
		if (Assigns = assigns)
		{
			Result.Metadata = First.Metadata;
		}
	}

	public override void OnSimulate()
	{
		if (Assigns && First.Metadata.IsPrimarilyVariable)
		{
      	Unit.Scope!.Variables[First.Metadata.Variable] = Result;
			Result.Metadata.Attach(new VariableAttribute(First.Metadata.Variable));
		}
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
					ParameterFlag.DESTINATION | (Assigns ? ParameterFlag.WRITE_ACCESS : ParameterFlag.NONE),
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
					ParameterFlag.DESTINATION | (Assigns ? ParameterFlag.WRITE_ACCESS : ParameterFlag.NONE),
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