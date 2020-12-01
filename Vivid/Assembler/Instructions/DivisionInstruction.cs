using System;

public class DivisionInstruction : DualParameterInstruction
{
	private const string SIGNED_INTEGER_DIVISION_INSTRUCTION = "idiv";

	private const string SINGLE_PRECISION_DIVISION_INSTRUCTION = "divss";
	private const string DOUBLE_PRECISION_DIVISION_INSTRUCTION = "divsd";

	private const string DIVIDE_BY_TWO_INSTRUCTION = "sar";

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

			return new MoveInstruction(Unit, new Result(destination, First.Format), First)
			{
				Type = Assigns ? MoveType.RELOCATE : MoveType.COPY

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
			SIGNED_INTEGER_DIVISION_INSTRUCTION,
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
			SIGNED_INTEGER_DIVISION_INSTRUCTION,
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
		if (Second.IsConstant)
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

	public override void OnBuild()
	{
		// Handle decimal division separately
		if (First.Format.IsDecimal() || Second.Format.IsDecimal())
		{
			var instruction = Assembler.IsTargetX86 ? SINGLE_PRECISION_DIVISION_INSTRUCTION : DOUBLE_PRECISION_DIVISION_INSTRUCTION;
			var flags = Assigns ? ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH : ParameterFlag.NONE;

			Build(
				instruction,
				new InstructionParameter(
					First,
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

				Build(
					DIVIDE_BY_TWO_INSTRUCTION,
					Assembler.Size,
					new InstructionParameter(
						division.Dividend,
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

	public override Result GetDestinationDependency()
	{
		return First;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.DIVISION;
	}
}