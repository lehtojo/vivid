using System;
using System.Globalization;

public class DivisionInstruction : DualParameterInstruction
{
	private const string SIGNED_INTEGER_DIVISION_INSTRUCTION = "idiv";

	private const string SINGLE_PRECISION_DIVISION_INSTRUCTION = "divss";
	private const string DOUBLE_PRECISION_DIVISION_INSTRUCTION = "divsd";

	private const string DIVIDE_BY_TWO_INSTRUCTION = "sar";

	public bool IsModulus { get; private set; }
	public bool Assigns { get; private set; }

	public DivisionInstruction(Unit unit, bool modulus, Result first, Result second, Format format, bool assigns) : base(unit, first, second, format)
	{
		IsModulus = modulus;

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
		var register = Unit.GetNumeratorRegister();
		var location = new RegisterHandle(register);

		if (!First.Value.Equals(location))
		{
			Memory.ClearRegister(Unit, location.Register);

			return new MoveInstruction(Unit, new Result(location, First.Format), First)
			{
				Type = Assigns ? MoveType.RELOCATE : MoveType.COPY

			}.Execute();
		}
		else if (!Assigns)
		{
			Memory.ClearRegister(Unit, location.Register);

			return new Result(location, First.Format);
		}

		return First;
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
			  ParameterFlag.WRITE_ACCESS | ParameterFlag.HIDDEN,
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
			  ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS | ParameterFlag.HIDDEN,
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
			var flags = ParameterFlag.DESTINATION | (Assigns ? ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH : ParameterFlag.NONE);

			Build(
			   instruction,
			   new InstructionParameter(
				  First,
				  flags,
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

		if (!IsModulus)
		{
			var division = TryGetConstantDivision();

			if (division != null && IsPowerOfTwo(division.Constant) && division.Constant != 0L)
			{
				var count = new ConstantHandle((long)Math.Log2(division.Constant));
				var flags = ParameterFlag.DESTINATION | (Assigns ? ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH : ParameterFlag.NONE);

				Build(
				   DIVIDE_BY_TWO_INSTRUCTION,
				   Assembler.Size,
				   new InstructionParameter(
					  division.Dividend,
					  flags,
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

		var numerator = CorrectNumeratorLocation();
		var remainder = Unit.Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.REMAINDER))!;

		// Clear the remainder register
		Memory.Zero(Unit, remainder);

		using (RegisterLock.Create(remainder))
		{
			if (IsModulus)
			{
				BuildModulus(numerator);
			}
			else
			{
				BuildDivision(numerator);
			}
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