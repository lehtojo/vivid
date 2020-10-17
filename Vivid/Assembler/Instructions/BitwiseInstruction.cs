using System;

public class BitwiseInstruction : DualParameterInstruction
{
	private const string AND_INSTRUCTION = "and";
	private const string XOR_INSTRUCTION = "xor";
	private const string SINGLE_PRECISION_MEDIA_XOR_INSTRUCTION = "xorps";
	private const string DOUBLE_PRECISION_MEDIA_XOR_INSTRUCTION = "xorpd";
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
		if (format.IsDecimal())
		{
			return new BitwiseInstruction(unit, Assembler.IsTargetX64 ? DOUBLE_PRECISION_MEDIA_XOR_INSTRUCTION : SINGLE_PRECISION_MEDIA_XOR_INSTRUCTION, first, second, format, assigns);
		}

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

	public override void OnBuild()
	{
		if (Instruction == SINGLE_PRECISION_MEDIA_XOR_INSTRUCTION || Instruction == DOUBLE_PRECISION_MEDIA_XOR_INSTRUCTION)
		{
			if (Assigns)
			{
				throw new NotSupportedException("Assigning bitwise XOR-operation on media registers is not allowed");
			}

			Build(
				Instruction,
				new InstructionParameter(
					First,
					ParameterFlag.DESTINATION,
					HandleType.MEDIA_REGISTER
				),
				new InstructionParameter(
					Second,
					ParameterFlag.NONE,
					HandleType.MEDIA_REGISTER,
					HandleType.MEMORY
				)
			);

			return;
		}

		var flags = ParameterFlag.DESTINATION | (Assigns ? ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH : ParameterFlag.NONE);

		if (First.IsMemoryAddress)
		{
			Build(
				Instruction,
				First.Size,
				new InstructionParameter(
					First,
					flags,
					HandleType.MEMORY
				),
				new InstructionParameter(
					Second,
					ParameterFlag.NONE,
					HandleType.CONSTANT,
					HandleType.REGISTER
				)
			);

			return;
		}

		Build(
			Instruction,
			Assembler.Size,
			new InstructionParameter(
				First,
				flags,
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

	public override Result GetDestinationDependency()
	{
		return First;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.BITWISE;
	}
}