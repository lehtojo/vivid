using System;

public class MultiplicationInstruction : DualParameterInstruction
{
	private const string SIGNED_INTEGER_MULTIPLICATION_INSTRUCTION = "imul";
	private const string UNSIGNED_INTEGER_MULTIPLICATION_INSTRUCTION = "mul";

	private const string SINGLE_PRECISION_MULTIPLICATION_INSTRUCTION = "mulss";
	private const string DOUBLE_PRECISION_MULTIPLICATION_INSTRUCTION = "mulsd";

	public const string EXTENDED_MULTIPLICATION_INSTRUCTION = "lea";
	public const string MULTIPLY_BY_TWO_INSTRUCTION = "sal";

	public bool Assigns { get; private set; }
	public new Format Type { get; private set; }

	public MultiplicationInstruction(Unit unit, Result first, Result second, Format type, bool assigns) : base(unit, first, second)
	{
		Type = type;

		if (Assigns = assigns)
		{
			Result.Metadata = First.Metadata;
		}
	}

	public override void OnSimulate()
	{
		if (Assigns && First.Metadata.IsPrimarilyVariable)
		{
      	Unit.Scope!.Variables[First.Metadata.Variable] = Result;
			Result.Metadata.Attach(new VariableAttribute(First.Metadata.Variable));
		}
	}

	private bool IsPowerOfTwo(long x)
	{
    	return (x & (x - 1)) == 0;
	}

	private bool IsConstantValidForExtendedMultiplication(long x)
	{
    	return IsPowerOfTwo(x) && x <= 8;
	}

	private class ConstantMultiplication
	{
		public Result Other;
		public long Constant;

		public ConstantMultiplication(Result other, Result constant)
		{
			Other = other;
			Constant = (long)constant.Value.To<ConstantHandle>().Value;
		}
	}

	private ConstantMultiplication? TryGetConstantMultiplication()
	{
		if (First.Value.Type == HandleType.CONSTANT)
		{
			return new ConstantMultiplication(Second, First);
		}
		else if (Second.Value.Type == HandleType.CONSTANT)
		{
			return new ConstantMultiplication(First, Second);
		}
		else
		{
			return null;
		}
	}

	public override void OnBuild()
	{
		var flags = ParameterFlag.DESTINATION | (Assigns ? ParameterFlag.WRITE_ACCESS : ParameterFlag.NONE);

		// Handle decimal multiplication separately
		if (Type == global::Format.DECIMAL)
		{
			var instruction = Assembler.Size.Bits == 32 ? SINGLE_PRECISION_MULTIPLICATION_INSTRUCTION : DOUBLE_PRECISION_MULTIPLICATION_INSTRUCTION;

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

		var constant_multiplication = TryGetConstantMultiplication();

		if (constant_multiplication != null)
		{
			var value = constant_multiplication.Constant;

			if (value > 0)
			{
				if (IsPowerOfTwo(value))
				{
					var count = new ConstantHandle((long)Math.Log2(value));

					Build(
						MULTIPLY_BY_TWO_INSTRUCTION,
						new InstructionParameter(
							constant_multiplication.Other,
							flags,
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
				else if (IsConstantValidForExtendedMultiplication(value - 1))
				{
					var count = new ConstantHandle(value - 1);

					var calculation = Format(
						"[{0}*{1}+{0}]",
						Assembler.Size,
						new InstructionParameter(
							constant_multiplication.Other,
							ParameterFlag.NONE,
							HandleType.REGISTER
						),
						new InstructionParameter(
							new Result(count),
							ParameterFlag.NONE,
							HandleType.CONSTANT
						)
					);

					if (Result.Value.Type != HandleType.REGISTER)
					{
						// Get a new register for the result
						Memory.GetRegisterFor(Unit, Result);
					}
					else
					{
						Result.Value.To<RegisterHandle>().Register.Handle = Result;
					}

					Build($"{EXTENDED_MULTIPLICATION_INSTRUCTION} {Result}, {calculation}");
					return;
				}
			}
		}

		Build(
			SIGNED_INTEGER_MULTIPLICATION_INSTRUCTION,
			Assembler.Size,
			new InstructionParameter(
				First,
				flags,
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
		var constant_multiplication = TryGetConstantMultiplication();

		if (constant_multiplication != null && constant_multiplication.Constant > 0 && IsConstantValidForExtendedMultiplication(constant_multiplication.Constant - 1))
		{
			return Result;
		}

		return First;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.MULTIPLICATION;
	}
}