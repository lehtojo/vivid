using System;
using System.Linq;

/// <summary>
/// This instruction does the specified bitwise operation between the two specified operand together and outputs a result.
/// This instruction is works on all architectures
/// </summary>
public class BitwiseInstruction : DualParameterInstruction
{
	public string Instruction { get; private set; }

	public bool IsUnsigned { get; set; } = false;
	public bool Assigns { get; set; } = false;

	public static BitwiseInstruction CreateAnd(Unit unit, Result first, Result second, Format format, bool assigns = false)
	{
		return new BitwiseInstruction(unit, Instructions.Shared.AND, first, second, format, assigns);
	}

	public static BitwiseInstruction CreateXor(Unit unit, Result first, Result second, Format format, bool assigns = false)
	{
		if (Settings.IsX64)
		{
			if (format.IsDecimal()) return new BitwiseInstruction(unit, Instructions.X64.DOUBLE_PRECISION_XOR, first, second, format, assigns);

			return new BitwiseInstruction(unit, Instructions.X64.XOR, first, second, format, assigns);
		}

		if (format.IsDecimal()) throw new ApplicationException("Unsupported instruction");

		return new BitwiseInstruction(unit, Instructions.Arm64.XOR, first, second, format, assigns);
	}

	public static BitwiseInstruction CreateOr(Unit unit, Result first, Result second, Format format, bool assigns = false)
	{
		return new BitwiseInstruction(unit, Settings.IsArm64 ? Instructions.Arm64.OR : Instructions.X64.OR, first, second, format, assigns);
	}

	public static BitwiseInstruction CreateShiftLeft(Unit unit, Result first, Result second, Format format, bool assigns = false)
	{
		return new BitwiseInstruction(unit, Settings.IsArm64 ? Instructions.Arm64.SHIFT_LEFT : Instructions.X64.SHIFT_LEFT, first, second, format, assigns);
	}

	public static BitwiseInstruction CreateShiftRight(Unit unit, Result first, Result second, Format format, bool is_unsigned, bool assigns = false)
	{
		var instruction = string.Empty;

		if (Settings.IsX64) { instruction = is_unsigned ? Instructions.X64.SHIFT_RIGHT_UNSIGNED : Instructions.X64.SHIFT_RIGHT; }
		else { instruction = is_unsigned ? Instructions.Arm64.SHIFT_RIGHT_UNSIGNED : Instructions.Arm64.SHIFT_RIGHT; }

		return new BitwiseInstruction(unit, instruction, first, second, format, is_unsigned, assigns);
	}

	private BitwiseInstruction(Unit unit, string instruction, Result first, Result second, Format format, bool assigns) : base(unit, first, second, format, InstructionType.BITWISE)
	{
		Instruction = instruction;
		Description = "Executes a bitwise operation between the operands";
		Assigns = assigns;
	}

	private BitwiseInstruction(Unit unit, string instruction, Result first, Result second, Format format, bool is_unsigned, bool assigns) : base(unit, first, second, format, InstructionType.BITWISE)
	{
		Instruction = instruction;
		Description = "Executes a bitwise operation between the operands";
		IsUnsigned = is_unsigned;
		Assigns = assigns;
	}

	public override void OnBuild()
	{
		if (Settings.IsX64)
		{
			OnBuildX64();
		}
		else
		{
			OnBuildArm64();
		}
	}

	private void BuildShiftX64()
	{
		var locked = (Register?)null;
		var shifter = new Result(Second.Value, Format.INT8);

		if (!Second.IsConstant)
		{
			// Relocate the second operand to the shift register
			var register = Unit.GetShiftRegister();
			Memory.ClearRegister(Unit, register);

			shifter = new MoveInstruction(Unit, new Result(new RegisterHandle(register), Format.INT8), Second)
			{
				Type = Assigns ? MoveType.RELOCATE : MoveType.COPY

			}.Add();

			// Lock the shift register since it is very important it does not get relocated
			register.Lock();
			locked = register;
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

			// Finally, if a register was locked, unlock it now
			if (locked != null) locked.Unlock();
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

		// Finally, if a register was locked, unlock it now
		if (locked != null) locked.Unlock();
	}

	public void OnBuildX64()
	{
		if (Instruction == Instructions.X64.DOUBLE_PRECISION_XOR)
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

		if (Instruction == Instructions.X64.SHIFT_LEFT || Instruction == Instructions.X64.SHIFT_RIGHT || Instruction == Instructions.X64.SHIFT_RIGHT_UNSIGNED)
		{
			BuildShiftX64();
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
			Settings.Size,
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

	public void OnBuildArm64()
	{
		if (Assigns)
		{
			if (First.IsMemoryAddress)
			{
				Unit.Add(new MoveInstruction(Unit, First, Result), true);
			}

			var result = Memory.LoadOperand(Unit, First, false, Assigns);

			Build(
				Instruction,
				Settings.Size,
				new InstructionParameter(
					result,
					ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH,
					HandleType.REGISTER
				),
				new InstructionParameter(
					result,
					ParameterFlag.NONE,
					HandleType.REGISTER
				),
				new InstructionParameter(
					Second,
					ParameterFlag.CreateBitLimit(12),
					HandleType.CONSTANT,
					HandleType.REGISTER
				)
			);

			return;
		}

		Memory.GetResultRegisterFor(Unit, Result, Unsigned, false);

		Build(
			Instruction,
			Settings.Size,
			new InstructionParameter(
				Result,
				ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS,
				HandleType.REGISTER
			),
			new InstructionParameter(
				First,
				ParameterFlag.NONE,
				HandleType.REGISTER
			),
			new InstructionParameter(
				Second,
				ParameterFlag.CreateBitLimit(12),
				HandleType.CONSTANT,
				HandleType.REGISTER
			)
		);
	}

	public bool RedirectX64(Handle handle)
	{
		if (!handle.Is(HandleType.REGISTER) || (Operation != Instructions.X64.SHIFT_LEFT && Operation != Instructions.X64.SHIFT_RIGHT))
		{
			return false;
		}

		Parameters.First().Value = handle;
		return true;
	}

	public bool RedirectArm64(Handle handle)
	{
		if (!handle.Is(HandleType.REGISTER))
		{
			return false;
		}

		Parameters.First().Value = handle;
		return true;
	}

	public override bool Redirect(Handle handle, bool root)
	{
		if (Settings.IsArm64)
		{
			return RedirectArm64(handle);
		}
		else
		{
			return root && RedirectX64(handle);
		}
	}
}