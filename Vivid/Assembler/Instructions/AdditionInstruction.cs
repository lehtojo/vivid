using System.Linq;

/// <summary>
/// This instruction adds the two specified operand together and outputs a result.
/// This instruction is works on all architectures
/// </summary>
public class AdditionInstruction : DualParameterInstruction
{
	public bool Assigns { get; private set; }

	public AdditionInstruction(Unit unit, Result first, Result second, Format format, bool assigns) : base(unit, first, second, format, InstructionType.ADDITION)
	{
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

	public void OnBuildX64()
	{
		if (First.Format.IsDecimal() || Second.Format.IsDecimal())
		{
			if (Assigns && First.IsMemoryAddress)
			{
				Unit.Add(new MoveInstruction(Unit, First, Result), true);
			}

			var result = Memory.LoadOperand(Unit, First, true, Assigns);
			var types = Second.Format.IsDecimal() ? new[] { HandleType.MEDIA_REGISTER, HandleType.MEMORY } : new[] { HandleType.MEDIA_REGISTER };

			// NOTE: Changed the parameter flag to none because any attachment could override the contents of the destination register and the variable should move to an appropriate register attaching the variable there
			var flags = Assigns ? ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH : ParameterFlag.NONE;

			Build(
				Instructions.X64.DOUBLE_PRECISION_ADD,
				new InstructionParameter(
					result,
					ParameterFlag.DESTINATION | ParameterFlag.READS | flags,
					HandleType.MEDIA_REGISTER
				),
				new InstructionParameter(
					Second,
					ParameterFlag.NONE,
					types
				)
			);

			return;
		}

		if (Assigns)
		{
			Build(
				Instructions.Shared.ADD,
				First.Size,
				new InstructionParameter(
					First,
					ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH | ParameterFlag.READS,
					HandleType.REGISTER,
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

		if (First.IsDeactivating())
		{
			Build(
				Instructions.Shared.ADD,
				Settings.Size,
				new InstructionParameter(
					First,
					ParameterFlag.DESTINATION | ParameterFlag.READS,
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

			return;
		}

		var calculation = ExpressionHandle.CreateAddition(First, Second);

		Build(
			Instructions.X64.EVALUATE,
			Settings.Size,
			new InstructionParameter(
				Result,
				ParameterFlag.DESTINATION,
				HandleType.REGISTER
			),
			new InstructionParameter(
				new Result(calculation, Settings.Format),
				ParameterFlag.NONE,
				HandleType.EXPRESSION
			)
		);
	}

	public void OnBuildArm64()
	{
		var is_decimal = First.Format.IsDecimal() || Second.Format.IsDecimal();
		var instruction = is_decimal ? Instructions.Arm64.DECIMAL_ADD : Instructions.Shared.ADD;
		var base_register_type = is_decimal ? HandleType.MEDIA_REGISTER : HandleType.REGISTER;
		var types = is_decimal ? new[] { HandleType.MEDIA_REGISTER } : new[] { HandleType.CONSTANT, HandleType.REGISTER, HandleType.MODIFIER };

		if (Assigns)
		{
			if (First.IsMemoryAddress)
			{
				Unit.Add(new MoveInstruction(Unit, First, Result), true);
			}

			var result = Memory.LoadOperand(Unit, First, is_decimal, Assigns);

			Build(
				instruction,
				Settings.Size,
				new InstructionParameter(
					result,
					ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH,
					base_register_type
				),
				new InstructionParameter(
					result,
					ParameterFlag.NONE,
					base_register_type
				),
				new InstructionParameter(
					Second,
					ParameterFlag.CreateBitLimit(12),
					types
				)
			);

			return;
		}

		Memory.GetResultRegisterFor(Unit, Result, Unsigned, is_decimal);

		Build(
			instruction,
			Settings.Size,
			new InstructionParameter(
				Result,
				ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS,
				base_register_type
			),
			new InstructionParameter(
				First,
				ParameterFlag.NONE,
				base_register_type
			),
			new InstructionParameter(
				Second,
				ParameterFlag.CreateBitLimit(12),
				types
			)
		);
	}

	public bool RedirectX64(Handle handle)
	{
		if (Operation == Instructions.X64.DOUBLE_PRECISION_ADD) return false;

		if (Operation == Instructions.X64.EVALUATE)
		{
			if (!handle.Is(HandleType.REGISTER)) return false;

			Destination!.Value = handle;
			return true;
		}

		var first = Parameters[0];
		var second = Parameters[1];

		if (handle.Type == HandleType.REGISTER && first.IsAnyRegister && (second.IsAnyRegister || second.IsConstant))
		{
			Operation = Instructions.X64.EVALUATE;

			var calculation = ExpressionHandle.CreateAddition(first.Value!, second.Value!);

			Parameters.Clear();
			Parameters.Add(new InstructionParameter(handle, ParameterFlag.DESTINATION));
			Parameters.Add(new InstructionParameter(calculation, ParameterFlag.NONE));

			return true;
		}

		return false;
	}

	public bool RedirectArm64(Handle handle)
	{
		if (Operation == Instructions.Shared.ADD && handle.Is(HandleType.REGISTER))
		{
			Parameters.First().Value = handle;
			return true;
		}

		if (Operation == Instructions.Arm64.DECIMAL_ADD && handle.Is(HandleType.MEDIA_REGISTER))
		{
			Parameters.First().Value = handle;
			return true;
		}

		return false;
	}

	public override bool Redirect(Handle handle, bool root)
	{
		if (Settings.IsArm64)
		{
			return RedirectArm64(handle);
		}

		return RedirectX64(handle);
	}
}