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

	private void BuildShift()
	{
		var unlock = (Instruction?)null;
		var shifter = new Result(Second.Value, Format.INT8);

		if (!Second.IsConstant)
		{
			// Relocate the second operand to the shift register
			var register = Unit.GetShiftRegister();
			Memory.ClearRegister(Unit, register);

			shifter = new MoveInstruction(Unit, new Result(new RegisterHandle(register), Format.INT8), Second)
			{
				Type = Assigns ? MoveType.RELOCATE : MoveType.COPY

			}.Execute();

			// Lock the shift register since it's very important it doesn't get relocated
			LockStateInstruction.Lock(Unit, register).Execute();
			unlock = LockStateInstruction.Unlock(Unit, register);
		}

		var flags = Assigns ? ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH : ParameterFlag.NONE;

		if (First.IsMemoryAddress && Assigns)
		{
			Build(
				Instruction,
				new InstructionParameter(
					First,
					ParameterFlag.DESTINATION | ParameterFlag.READS | flags,
					HandleType.MEMORY
				),
				new InstructionParameter(
					shifter,
					ParameterFlag.NONE,
					HandleType.CONSTANT,
					HandleType.REGISTER
				)
			);

			if (unlock != null)
			{
				Unit.Append(unlock);
			}

			return;
		}

		Build(
			Instruction,
			new InstructionParameter(
				First,
				ParameterFlag.DESTINATION | ParameterFlag.READS | flags,
				HandleType.REGISTER
			),
			new InstructionParameter(
				shifter,
				ParameterFlag.NONE,
				HandleType.CONSTANT,
				HandleType.REGISTER
			)
		);

		if (unlock != null)
		{
			Unit.Append(unlock);
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
					ParameterFlag.DESTINATION | ParameterFlag.READS,
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

		if (Instruction == SHIFT_LEFT_INSTRUCTION || Instruction == SHIFT_RIGHT_INSTRUCTION)
		{
			BuildShift();
			return;
		}

		var flags = ParameterFlag.DESTINATION | (Assigns ? ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH : ParameterFlag.NONE);

		if (First.IsMemoryAddress && Assigns)
		{
			Build(
				Instruction,
				First.Size,
				new InstructionParameter(
					First,
					ParameterFlag.READS | flags,
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
				ParameterFlag.READS | flags,
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