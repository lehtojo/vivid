using System.Linq;

/// <summary>
/// This instruction adds the two specified operand together and outputs a result.
/// This instruction is works on all architectures
/// </summary>
public class AdditionInstruction : DualParameterInstruction
{
	public const string SHARED_STANDARD_ADDITION_INSTRUCTION = "add";

	private const int STANDARD_ADDITION_FIRST = 0;
	private const int STANDARD_ADDITION_SECOND = 1;

	private const string X64_EXTENDED_ADDITION_INSTRUCTION = "lea";
	private const string X64_SINGLE_PRECISION_ADDITION_INSTRUCTION = "addss";
	private const string X64_DOUBLE_PRECISION_ADDITION_INSTRUCTION = "addsd";

	public const string ARM64_DECIMAL_ADDITION_INSTRUCTION = "fadd";

	public bool Assigns { get; private set; }

	public AdditionInstruction(Unit unit, Result first, Result second, Format format, bool assigns) : base(unit, first, second, format, InstructionType.ADDITION)
	{
		Assigns = assigns;
	}

	public override void OnBuild()
	{
		if (Assembler.IsX64)
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
				Unit.Append(new MoveInstruction(Unit, First, Result), true);
			}

			var instruction = Assembler.Is32bit ? X64_SINGLE_PRECISION_ADDITION_INSTRUCTION : X64_DOUBLE_PRECISION_ADDITION_INSTRUCTION;
			var result = Memory.LoadOperand(Unit, First, true, Assigns);

			// NOTE: Changed the parameter flag to none because any attachment could override the contents of the destination register and the variable should move to an appropriate register attaching the variable there
			var flags = Assigns ? ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH : ParameterFlag.NONE;

			Build(
				instruction,
				new InstructionParameter(
					result,
					ParameterFlag.DESTINATION | ParameterFlag.READS | flags,
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

		if (Assigns)
		{
			Build(
				SHARED_STANDARD_ADDITION_INSTRUCTION,
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

		if (First.IsExpiring(Position))
		{
			Build(
				SHARED_STANDARD_ADDITION_INSTRUCTION,
				Assembler.Size,
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
			X64_EXTENDED_ADDITION_INSTRUCTION,
			Assembler.Size,
			new InstructionParameter(
				Result,
				ParameterFlag.DESTINATION,
				HandleType.REGISTER
			),
			new InstructionParameter(
				new Result(calculation, Assembler.Format),
				ParameterFlag.NONE,
				HandleType.EXPRESSION
			)
		);
	}

	public void OnBuildArm64()
	{
		var is_decimal = First.Format.IsDecimal() || Second.Format.IsDecimal();
		var instruction = is_decimal ? ARM64_DECIMAL_ADDITION_INSTRUCTION : SHARED_STANDARD_ADDITION_INSTRUCTION;
		var base_register_type = is_decimal ? HandleType.MEDIA_REGISTER : HandleType.REGISTER;
		var types = is_decimal ? new[] { HandleType.MEDIA_REGISTER } : new[] { HandleType.CONSTANT, HandleType.REGISTER, HandleType.MODIFIER };

		if (Assigns)
		{
			if (First.IsMemoryAddress)
			{
				Unit.Append(new MoveInstruction(Unit, First, Result), true);
			}

			var result = Memory.LoadOperand(Unit, First, is_decimal, Assigns);

			Build(
				instruction,
				Assembler.Size,
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

		Memory.GetResultRegisterFor(Unit, Result, is_decimal);

		Build(
			instruction,
			Assembler.Size,
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
		if (Operation == X64_SINGLE_PRECISION_ADDITION_INSTRUCTION || Operation == X64_DOUBLE_PRECISION_ADDITION_INSTRUCTION)
		{
			return false;
		}

		if (Operation == X64_EXTENDED_ADDITION_INSTRUCTION)
		{
			if (!handle.Is(HandleType.REGISTER))
			{
				return false;
			}

			Destination!.Value = handle;
			return true;
		}

		var first = Parameters[STANDARD_ADDITION_FIRST];
		var second = Parameters[STANDARD_ADDITION_SECOND];

		if (handle.Type == HandleType.REGISTER && first.IsAnyRegister && (second.IsAnyRegister || second.IsConstant))
		{
			Operation = X64_EXTENDED_ADDITION_INSTRUCTION;

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
		if (Operation == SHARED_STANDARD_ADDITION_INSTRUCTION && handle.Is(HandleType.REGISTER))
		{
			Parameters.First().Value = handle;
			return true;
		}

		if (Operation == ARM64_DECIMAL_ADDITION_INSTRUCTION && handle.Is(HandleType.MEDIA_REGISTER))
		{
			Parameters.First().Value = handle;
			return true;
		}

		return false;
	}

	public override bool Redirect(Handle handle)
	{
		if (Assembler.IsArm64)
		{
			return RedirectArm64(handle);
		}

		return RedirectX64(handle);
	}
}