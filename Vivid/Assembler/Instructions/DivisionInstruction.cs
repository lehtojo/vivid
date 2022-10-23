using System;
using System.Linq;

/// <summary>
/// This instruction divides the two specified operand together and outputs a result.
/// This instruction can act as a remainder operation.
/// This instruction is works on all architectures
/// </summary>
public class DivisionInstruction : DualParameterInstruction
{
	public bool Modulus { get; private set; }
	public bool Assigns { get; private set; }
	public new bool Unsigned { get; private set; }

	public DivisionInstruction(Unit unit, bool modulus, Result first, Result second, Format format, bool assigns, bool unsigned) : base(unit, first, second, format, InstructionType.DIVISION)
	{
		Modulus = modulus;
		Unsigned = unsigned;
		Assigns = assigns;
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
			remainder.Lock();
			Memory.ClearRegister(Unit, destination.Register);
			remainder.Unlock();

			if (Assigns && !First.IsMemoryAddress)
			{
				Unit.Add(new MoveInstruction(Unit, new Result(destination, GetSystemFormat(Unsigned)), First) { Type = MoveType.RELOCATE });
				return First;
			}

			return new MoveInstruction(Unit, new Result(destination, GetSystemFormat(Unsigned)), First) { Type = MoveType.COPY }.Add();
		}
		else if (!Assigns)
		{
			if (!First.IsDeactivating())
			{
				Memory.ClearRegister(Unit, destination.Register);
			}

			return new Result(destination, GetSystemFormat(Unsigned));
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

		numerator_register.Lock();
		remainder_register.Lock();

		if (Unsigned)
		{
			// Clear the remainder register
			Memory.Zero(Unit, remainder_register);
		}
		else
		{
			Memory.ClearRegister(Unit, remainder_register);
			Unit.Add(new ExtendNumeratorInstruction(Unit));
		}

		numerator_register.Unlock();
		remainder_register.Unlock();
	}

	/// <summary>
	/// Builds a modulus operation
	/// </summary>
	private void BuildModulus(Result numerator)
	{
		var remainder = new RegisterHandle(Unit.GetRemainderRegister());
		var flags = ParameterFlag.WRITE_ACCESS | ParameterFlag.HIDDEN | ParameterFlag.WRITES | ParameterFlag.READS | ParameterFlag.LOCKED | (Assigns ? ParameterFlag.RELOCATE_TO_DESTINATION : ParameterFlag.NONE);

		Build(
			Unsigned ? Instructions.X64.UNSIGNED_DIVIDE : Instructions.X64.SIGNED_DIVIDE,
			Settings.Size,
			new InstructionParameter(
				numerator,
				flags,
				HandleType.REGISTER
			),
			new InstructionParameter(
				Second,
				ParameterFlag.NONE,
				HandleType.REGISTER,
				HandleType.MEMORY
			),
			new InstructionParameter(
				new Result(remainder, GetSystemFormat(Unsigned)),
				flags | ParameterFlag.DESTINATION,
				HandleType.REGISTER
			)
		);
	}

	/// <summary>
	/// Builds a division operation
	/// </summary>
	private void BuildDivision(Result numerator)
	{
		var remainder = new RegisterHandle(Unit.GetRemainderRegister());
		var flags = ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS | ParameterFlag.HIDDEN | ParameterFlag.READS | ParameterFlag.LOCKED;

		Build(
			Unsigned ? Instructions.X64.UNSIGNED_DIVIDE : Instructions.X64.SIGNED_DIVIDE,
			Settings.Size,
			new InstructionParameter(
				numerator,
				(Assigns ? ParameterFlag.NO_ATTACH : ParameterFlag.NONE) | flags,
				HandleType.REGISTER
			),
			new InstructionParameter(
				Second,
				ParameterFlag.NONE,
				HandleType.REGISTER,
				HandleType.MEMORY
			),
			new InstructionParameter(
				new Result(remainder, GetSystemFormat(Unsigned)),
				ParameterFlag.HIDDEN | ParameterFlag.LOCKED | ParameterFlag.WRITES,
				HandleType.REGISTER
			)
		);
	}

	private class ConstantDivision
	{
		public Result Dividend;
		public long Number;

		public ConstantDivision(Result dividend, Result number)
		{
			Dividend = dividend;
			Number = (long)number.Value.To<ConstantHandle>().Value;
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

	public void OnBuildX64()
	{
		// Handle decimal division separately
		if (First.Format.IsDecimal() || Second.Format.IsDecimal())
		{
			var flags = Assigns ? ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH : ParameterFlag.NONE;
			var result = Memory.LoadOperand(Unit, First, true, Assigns);
			var types = Second.Format.IsDecimal() ? new[] { HandleType.MEDIA_REGISTER, HandleType.MEMORY } : new[] { HandleType.MEDIA_REGISTER };

			Build(
				Instructions.X64.DOUBLE_PRECISION_DIVIDE,
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

		if (!Modulus)
		{
			var division = TryGetConstantDivision();

			if (division != null && Common.IsPowerOfTwo(division.Number) && division.Number != 0L)
			{
				var count = new ConstantHandle((long)Math.Log2(division.Number));
				var flags = Assigns ? ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH : ParameterFlag.NONE;
				var instruction = Unsigned ? Instructions.X64.SHIFT_RIGHT_UNSIGNED : Instructions.X64.SHIFT_RIGHT;
				var operand = Memory.LoadOperand(Unit, division.Dividend, false, Assigns);

				Build(
					Instructions.X64.SHIFT_RIGHT,
					Settings.Size,
					new InstructionParameter(
						operand,
						ParameterFlag.DESTINATION | ParameterFlag.READS | flags,
						HandleType.REGISTER
					),
					new InstructionParameter(
						new Result(count, Settings.Format),
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

		numerator_register.Lock();
		remainder_register.Lock();

		if (Modulus)
		{
			BuildModulus(numerator);
		}
		else
		{
			BuildDivision(numerator);
		}

		numerator_register.Unlock();
		remainder_register.Unlock();
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
		var division = new DivisionInstruction(Unit, false, first, Second, Result.Format, false, Unsigned).Add();

		if (Assigns)
		{
			Build(
				Instructions.Arm64.MULTIPLY_SUBTRACT,
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

		Memory.GetResultRegisterFor(Unit, Result, Unsigned, false);

		Build(
			Instructions.Arm64.MULTIPLY_SUBTRACT,
			Settings.Size,
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
	}

	public void OnBuildArm64()
	{
		var is_decimal = First.Format.IsDecimal() || Second.Format.IsDecimal();
		var instruction = is_decimal ? Instructions.Arm64.DECIMAL_DIVIDE : (Unsigned ? Instructions.Arm64.UNSIGNED_DIVIDE : Instructions.Arm64.SIGNED_DIVIDE);
		var base_register_type = is_decimal ? HandleType.MEDIA_REGISTER : HandleType.REGISTER;
		var types = is_decimal ? new[] { HandleType.MEDIA_REGISTER } : new[] { HandleType.REGISTER };

		var division = TryGetConstantDivision();
		var second = Second;

		if (!is_decimal && division != null && Common.IsPowerOfTwo(division.Number) && division.Number != 0)
		{
			second = new Result(new ConstantHandle((long)Math.Log2(division.Number)), Settings.Format);
			types = is_decimal ? new[] { HandleType.CONSTANT, HandleType.MEDIA_REGISTER } : new[] { HandleType.CONSTANT, HandleType.REGISTER };
			instruction = Instructions.Arm64.SHIFT_RIGHT;
		}

		if (Assigns)
		{
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
					second,
					ParameterFlag.NONE,
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
	}

	public override void OnBuild()
	{
		if (Assigns && First.IsMemoryAddress)
		{
			Unit.Add(new MoveInstruction(Unit, First, Result), true);
		}

		if (Settings.IsX64)
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
		if (Operation == Instructions.Arm64.SIGNED_DIVIDE && handle.Is(HandleType.REGISTER))
		{
			Parameters.First().Value = handle;
			return true;
		}

		if (Operation == Instructions.Arm64.DECIMAL_DIVIDE && handle.Is(HandleType.MEDIA_REGISTER))
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

		return false;
	}
}