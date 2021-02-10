using System.Linq;

/// <summary>
/// Substracts the specified values together
/// This instruction works on all architectures
/// </summary>
public class SubtractionInstruction : DualParameterInstruction
{
	public const string SHARED_STANDARD_SUBTRACTION_INSTRUCTION = "sub";

	private const int STANDARD_SUBTRACTION_FIRST = 0;
	private const int STANDARD_SUBTRACTION_SECOND = 1;

	private const string X64_EXTENDED_ADDITION_INSTRUCTION = "lea";
	private const string X64_SINGLE_PRECISION_SUBTRACTION_INSTRUCTION = "subss";
	private const string X64_DOUBLE_PRECISION_SUBTRACTION_INSTRUCTION = "subsd";

	public const string ARM64_DECIMAL_SUBSTRACTION_INSTRUCTION = "fsub";

	public bool Assigns { get; private set; }

	public SubtractionInstruction(Unit unit, Result first, Result second, Format format, bool assigns) : base(unit, first, second, format, InstructionType.SUBTRACT)
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
		var flags = ParameterFlag.DESTINATION | (Assigns ? ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH : ParameterFlag.NONE);

		// Handle decimal division separately
		if (First.Format.IsDecimal() || Second.Format.IsDecimal())
		{
			if (Assigns && First.IsMemoryAddress)
			{
				Unit.Append(new MoveInstruction(Unit, First, Result), true);
			}

			var instruction = Assembler.Is32bit ? X64_SINGLE_PRECISION_SUBTRACTION_INSTRUCTION : X64_DOUBLE_PRECISION_SUBTRACTION_INSTRUCTION;
			var result = Memory.LoadOperand(Unit, First, true, Assigns);
			var types = Second.Format.IsDecimal() ? new[] { HandleType.MEDIA_REGISTER, HandleType.MEMORY } : new[] { HandleType.MEDIA_REGISTER };

			Build(
				instruction,
				new InstructionParameter(
					result,
					ParameterFlag.READS | flags,
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
				SHARED_STANDARD_SUBTRACTION_INSTRUCTION,
				First.Size,
				new InstructionParameter(
					First,
					ParameterFlag.READS | flags,
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

		Build(
			SHARED_STANDARD_SUBTRACTION_INSTRUCTION,
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
	}

	public void OnBuildArm64()
	{
		var is_decimal = First.Format.IsDecimal() || Second.Format.IsDecimal();
		var instruction = is_decimal ? ARM64_DECIMAL_SUBSTRACTION_INSTRUCTION : SHARED_STANDARD_SUBTRACTION_INSTRUCTION;
		var base_register_type = is_decimal ? HandleType.MEDIA_REGISTER : HandleType.REGISTER;
		var types = is_decimal ? new[] { HandleType.MEDIA_REGISTER } : new[] { HandleType.CONSTANT, HandleType.REGISTER };

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
		if (Operation == X64_SINGLE_PRECISION_SUBTRACTION_INSTRUCTION || Operation == X64_DOUBLE_PRECISION_SUBTRACTION_INSTRUCTION)
		{
			return false;
		}

		var first = Parameters[STANDARD_SUBTRACTION_FIRST];
		var second = Parameters[STANDARD_SUBTRACTION_SECOND];

		if (handle.Type == HandleType.REGISTER && first.IsAnyRegister && second.IsConstant)
		{
			Operation = X64_EXTENDED_ADDITION_INSTRUCTION;

			var constant = -(long)second.Value!.To<ConstantHandle>().Value;
			var calculation = ExpressionHandle.CreateAddition(first.Value!, new ConstantHandle(constant));

			Parameters.Clear();
			Parameters.Add(new InstructionParameter(handle, ParameterFlag.DESTINATION));
			Parameters.Add(new InstructionParameter(calculation, ParameterFlag.NONE));

			return true;
		}

		return false;
	}

	public bool RedirectArm64(Handle handle)
	{
		if (Operation == SHARED_STANDARD_SUBTRACTION_INSTRUCTION && handle.Is(HandleType.REGISTER))
		{
			Parameters.First().Value = handle;
			return true;
		}

		if (Operation == ARM64_DECIMAL_SUBSTRACTION_INSTRUCTION && handle.Is(HandleType.MEDIA_REGISTER))
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