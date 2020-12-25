using System;
using System.Linq;

public class DivisionInstruction : DualParameterInstruction
{
	private const string X64_SIGNED_INTEGER_DIVISION_INSTRUCTION = "idiv";
	private const string ARM64_SIGNED_INTEGER_DIVISION_INSTRUCTION = "sdiv";

	private const string X64_SINGLE_PRECISION_DIVISION_INSTRUCTION = "divss";
	private const string X64_DOUBLE_PRECISION_DIVISION_INSTRUCTION = "divsd";

	private const string ARM64_DECIMAL_DIVISION_INSTRUCTION = "fdiv";

	private const string X64_DIVIDE_BY_POWER_OF_TWO_INSTRUCTION = "sar";
	private const string ARM64_DIVIDE_BY_POWER_OF_TWO_INSTRUCTION = "asr";

	public const string ARM64_MULTIPLICATION_SUBTRACTION_INSTRUCTION = "msub";

	public bool Modulus { get; private set; }
	public bool Assigns { get; private set; }
	public bool Unsigned { get; private set; }

	public DivisionInstruction(Unit unit, bool modulus, Result first, Result second, Format format, bool assigns, bool unsigned) : base(unit, first, second, format)
	{
		Modulus = modulus;
		Unsigned = unsigned;

		if (Assigns = assigns)
		{
			Result.Metadata = First.Metadata;
		}
	}

	/// <summary>
	/// Ensures the numerator value is in the right register
	/// </summary>
	private Result CorrectNumeratorLocation()
	{
		var numerator = Unit.GetNumeratorRegister();
		var remainder = Unit.GetRemainderRegister();

		var destination = new RegisterHandle(numerator);

		if (!First.Value.Equals(destination))
		{
			using (RegisterLock.Create(remainder))
			{
				Memory.ClearRegister(Unit, destination.Register);
			}

			// NOTE: The destination operand must be copied if it's a memory address and this instruction assigns
			return new MoveInstruction(Unit, new Result(destination, First.Format), First)
			{
				Type = Assigns && !First.IsMemoryAddress ? MoveType.RELOCATE : MoveType.COPY

			}.Execute();
		}
		else if (!Assigns)
		{
			if (!First.IsExpiring(Unit.Position))
			{
				Memory.ClearRegister(Unit, destination.Register);
			}

			return new Result(destination, First.Format);
		}

		return First;
	}

	/// <summary>
	/// Ensures the remainder register is ready for division or modulus operation
	/// </summary>
	private void PrepareRemainderRegister()
	{
		var numerator_register = Unit.GetNumeratorRegister();
		var remainder_register = Unit.GetRemainderRegister();

		using var numerator_lock = new RegisterLock(numerator_register);
		using var remainder_lock = new RegisterLock(remainder_register);

		if (Unsigned)
		{
			// Clear the remainder register
			Memory.Zero(Unit, remainder_register);
		}
		else
		{
			Memory.ClearRegister(Unit, remainder_register);
			Unit.Append(new ExtendNumeratorInstruction(Unit));
		}
	}

	/// <summary>
	/// Builds a modulus operation
	/// </summary>
	private void BuildModulus(Result numerator)
	{
		var destination = new RegisterHandle(Unit.Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.REMAINDER))!);

		Build(
			X64_SIGNED_INTEGER_DIVISION_INSTRUCTION,
			Assembler.Size,
			new InstructionParameter(
				numerator,
				ParameterFlag.WRITE_ACCESS | ParameterFlag.HIDDEN | ParameterFlag.READS,
				HandleType.REGISTER
			),
			new InstructionParameter(
				Second,
				ParameterFlag.NONE,
				HandleType.REGISTER,
				HandleType.MEMORY
			),
			new InstructionParameter(
				new Result(destination, Assembler.Format),
				ParameterFlag.WRITE_ACCESS | ParameterFlag.DESTINATION | ParameterFlag.HIDDEN,
				HandleType.REGISTER
			)
		);
	}

	/// <summary>
	/// Builds a division operation
	/// </summary>
	private void BuildDivision(Result numerator)
	{
		Build(
			X64_SIGNED_INTEGER_DIVISION_INSTRUCTION,
			Assembler.Size,
			new InstructionParameter(
				numerator,
				ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS | ParameterFlag.HIDDEN | ParameterFlag.READS,
				HandleType.REGISTER
			),
			new InstructionParameter(
				Second,
				ParameterFlag.NONE,
				HandleType.REGISTER,
				HandleType.MEMORY
			)
		);
	}

	private class ConstantDivision
	{
		public Result Dividend;
		public long Constant;

		public ConstantDivision(Result dividend, Result constant)
		{
			Dividend = dividend;
			Constant = (long)constant.Value.To<ConstantHandle>().Value;
		}
	}

	/// <summary>
	/// Tries to express the current instructions as a division instruction where the divisor is a constant
	/// </summary>
	private ConstantDivision? TryGetConstantDivision()
	{
		if (Second.IsConstant && !Second.Format.IsDecimal())
		{
			return new ConstantDivision(First, Second);
		}
		else
		{
			return null;
		}
	}

	private static bool IsPowerOfTwo(long x)
	{
		return (x & (x - 1)) == 0;
	}

	public void OnBuildX64()
	{
		// Handle decimal division separately
		if (First.Format.IsDecimal() || Second.Format.IsDecimal())
		{
			var instruction = Assembler.Is32bit ? X64_SINGLE_PRECISION_DIVISION_INSTRUCTION : X64_DOUBLE_PRECISION_DIVISION_INSTRUCTION;
			var flags = Assigns ? ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH : ParameterFlag.NONE;
			var result = Memory.LoadOperand(Unit, First, true, Assigns);

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

		if (!Modulus)
		{
			var division = TryGetConstantDivision();

			if (division != null && IsPowerOfTwo(division.Constant) && division.Constant != 0L)
			{
				var count = new ConstantHandle((long)Math.Log2(division.Constant));
				var flags = Assigns ? ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH : ParameterFlag.NONE;
				var result = Memory.LoadOperand(Unit, division.Dividend, false, Assigns);

				Build(
					X64_DIVIDE_BY_POWER_OF_TWO_INSTRUCTION,
					Assembler.Size,
					new InstructionParameter(
						result,
						ParameterFlag.DESTINATION | ParameterFlag.READS | flags,
						HandleType.REGISTER
					),
					new InstructionParameter(
						new Result(count, Assembler.Format),
						ParameterFlag.NONE,
						HandleType.CONSTANT
					)
				);

				return;
			}
		}

		var numerator_register = Unit.GetNumeratorRegister();
		var remainder_register = Unit.GetRemainderRegister();

		var numerator = CorrectNumeratorLocation();

		PrepareRemainderRegister();

		using var numerator_lock = new RegisterLock(numerator_register);
		using var remainder_lock = new RegisterLock(remainder_register);

		if (Modulus)
		{
			BuildModulus(numerator);
		}
		else
		{
			BuildDivision(numerator);
		}
	}

	public void BuildModulusArm64()
	{
		// Formula: a % b = a - (a / b) * b

		// Example:
		// a: x0
		// b: x1
		//
		// sdiv x2, x0, x1
		// msub x0, x2, x1, x0 (assigns)

		var first = Memory.LoadOperand(Unit, First, false, Assigns);
		var division = new DivisionInstruction(Unit, false, first, Second, Format, false, Unsigned).Execute();

		if (Assigns)
		{
			Build(
				ARM64_MULTIPLICATION_SUBTRACTION_INSTRUCTION,
				new InstructionParameter(
					first,
					ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH,
					HandleType.REGISTER
				),
				new InstructionParameter(
					division,
					ParameterFlag.NONE,
					HandleType.REGISTER
				),
				new InstructionParameter(
					Second,
					ParameterFlag.NONE,
					HandleType.REGISTER
				),
				new InstructionParameter(
					first,
					ParameterFlag.NONE,
					HandleType.REGISTER
				)
			);

			return;
		}

		Memory.GetResultRegisterFor(Unit, Result, false);

		Build(
			ARM64_MULTIPLICATION_SUBTRACTION_INSTRUCTION,
			Assembler.Size,
			new InstructionParameter(
				Result,
				ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS,
				HandleType.REGISTER
			),
			new InstructionParameter(
				division,
				ParameterFlag.NONE,
				HandleType.REGISTER
			),
			new InstructionParameter(
				Second,
				ParameterFlag.NONE,
				HandleType.REGISTER
			),
			new InstructionParameter(
				first,
				ParameterFlag.NONE,
				HandleType.REGISTER
			)
		);

		return;
	}

	public void OnBuildArm64()
	{
		var is_decimal = First.Format.IsDecimal() || Second.Format.IsDecimal();
		var instruction = is_decimal ? ARM64_DECIMAL_DIVISION_INSTRUCTION : ARM64_SIGNED_INTEGER_DIVISION_INSTRUCTION;
		var base_register_type = is_decimal ? HandleType.MEDIA_REGISTER : HandleType.REGISTER;
		var types = is_decimal ? new[] { HandleType.MEDIA_REGISTER } : new[] { HandleType.REGISTER };

		var division = TryGetConstantDivision();
		var second = Second;

		if (!is_decimal && division != null && IsPowerOfTwo(division.Constant) && division.Constant != 0)
		{
			second = new Result(new ConstantHandle((long)Math.Log2(division.Constant)), Assembler.Format);
			types = is_decimal ? new[] { HandleType.CONSTANT, HandleType.MEDIA_REGISTER } : new[] { HandleType.CONSTANT, HandleType.REGISTER };
			instruction = ARM64_DIVIDE_BY_POWER_OF_TWO_INSTRUCTION;
		}

		if (Assigns)
		{
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
					second,
					ParameterFlag.NONE,
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
				ParameterFlag.DESTINATION,
				base_register_type
			),
			new InstructionParameter(
				First,
				ParameterFlag.NONE,
				base_register_type
			),
			new InstructionParameter(
				second,
				ParameterFlag.NONE,
				types
			)
		);

		return;
	}

	public override void OnBuild()
	{
		if (Assigns && First.IsMemoryAddress)
		{
			Unit.Append(new MoveInstruction(Unit, First, Result), true);
		}

		if (Assembler.IsX64)
		{
			OnBuildX64();
		}
		else if (Modulus)
		{
			BuildModulusArm64();
		}
		else
		{
			OnBuildArm64();
		}
	}

	public bool RedirectArm64(Handle handle)
	{
		if (Operation == ARM64_SIGNED_INTEGER_DIVISION_INSTRUCTION && handle.Is(HandleType.REGISTER))
		{
			Parameters.First().Value = handle;
			return true;
		}

		if (Operation == ARM64_DECIMAL_DIVISION_INSTRUCTION && handle.Is(HandleType.MEDIA_REGISTER))
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

		return false;
	}

	public override Result GetDestinationDependency()
	{
		return First;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.DIVISION;
	}
}