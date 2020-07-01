using System;

public class DivisionInstruction : DualParameterInstruction
{
	private const string SIGNED_INTEGER_DIVISION_INSTRUCTION = "idiv";
	private const string UNSIGNED_INTEGER_DIVISION_INSTRUCTION = "div";

	private const string SINGLE_PRECISION_DIVISION_INSTRUCTION = "divss";
	private const string DOUBLE_PRECISION_DIVISION_INSTRUCTION = "divsd";

	private const string DIVIDE_BY_TWO_INSTRUCTION = "sar";

	public bool IsModulus { get; private set; }
	public bool Assigns { get; private set; }
	public new Format Type { get; private set; }

	public DivisionInstruction(Unit unit, bool modulus, Result first, Result second, Format type, bool assigns) : base(unit, first, second)
	{
		IsModulus = modulus;
		Type = type;

		if (Assigns = assigns)
		{
			Result.Metadata = First.Metadata;
		}
	}

	private Result CorrectDenominatorLocation()
	{
		var register = Unit.Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.DENOMINATOR)) ?? throw new ApplicationException("Architecture didn't have denominator register");
		var location = new RegisterHandle(register);

		if (!First.Value.Equals(location))
		{
			Memory.ClearRegister(Unit, location.Register);

         var move = new MoveInstruction(Unit, new Result(location), First)
         {
            Type = Assigns ? MoveType.RELOCATE : MoveType.COPY
         };

         return move.Execute();
		}

		return First;
	}

	private void BuildModulus(Result denominator)
	{
		var destination = new RegisterHandle(Unit.Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.REMAINDER))!);

		Build(
			SIGNED_INTEGER_DIVISION_INSTRUCTION,
			Assembler.Size,
			new InstructionParameter(
				denominator,
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
				new Result(destination),
				ParameterFlag.WRITE_ACCESS | ParameterFlag.DESTINATION | ParameterFlag.HIDDEN,
				HandleType.REGISTER
			)
		);
	}

	private void BuildDivision(Result denominator)
	{
		Build(
			SIGNED_INTEGER_DIVISION_INSTRUCTION,
			Assembler.Size,
			new InstructionParameter(
				denominator,
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

	public override void OnSimulate()
	{
		if (Assigns && First.Metadata.IsPrimarilyVariable)
		{
      	Unit.Scope!.Variables[First.Metadata.Variable] = Result;
			Result.Metadata.Attach(new VariableAttribute(First.Metadata.Variable));
		}
	}

	private class ConstantDivision
	{
		public Result Other;
		public long Constant;

		public ConstantDivision(Result other, Result constant)
		{
			Other = other;
			Constant = (long)constant.Value.To<ConstantHandle>().Value;
		}
	}

	private ConstantDivision? TryGetConstantDivision()
	{
		if (First.Value.Type == HandleType.CONSTANT)
		{
			return new ConstantDivision(Second, First);
		}
		else if (Second.Value.Type == HandleType.CONSTANT)
		{
			return new ConstantDivision(First, Second);
		}
		else
		{
			return null;
		}
	}

	private bool IsPowerOfTwo(long x)
	{
    	return (x & (x - 1)) == 0;
	}

	public override void OnBuild()
	{
		// Handle decimal division separately
		if (Type == global::Format.DECIMAL)
		{
			var instruction = Assembler.Size.Bits == 32 ? SINGLE_PRECISION_DIVISION_INSTRUCTION : DOUBLE_PRECISION_DIVISION_INSTRUCTION;
			var flags = ParameterFlag.DESTINATION | (Assigns ? ParameterFlag.WRITE_ACCESS : ParameterFlag.NONE);

			Build(
				instruction,
				Assembler.Size,
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
			var constant_multiplication = TryGetConstantDivision();

			if (constant_multiplication != null && IsPowerOfTwo(constant_multiplication.Constant))
			{
				var count = new ConstantHandle((long)Math.Log2(constant_multiplication.Constant));

				Build(
					DIVIDE_BY_TWO_INSTRUCTION,
					new InstructionParameter(
						constant_multiplication.Other,
						ParameterFlag.DESTINATION | (Assigns ? ParameterFlag.WRITE_ACCESS : ParameterFlag.NONE),
						HandleType.REGISTER
					),
					new InstructionParameter(
						new Result(count),
						ParameterFlag.NONE,
						HandleType.CONSTANT
					)
				);

				return;
			}
		}

		var denominator = CorrectDenominatorLocation();
		var remainder = Unit.Registers.Find(r => Flag.Has(r.Flags, RegisterFlag.REMAINDER))!;

		// Clear the remainder register
		Memory.Zero(Unit, remainder);
		
		using (new RegisterLock(remainder))
		{
			if (IsModulus)
			{
				BuildModulus(denominator);
			}
			else
			{
				BuildDivision(denominator);
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