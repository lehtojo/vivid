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

	public MultiplicationInstruction(Unit unit, Result first, Result second, Format format, bool assigns) : base(unit, first, second, format)
	{
		if (Assigns = assigns)
		{
			Result.Metadata = First.Metadata;
		}
	}

	public override void OnSimulate()
	{
		if (Assigns && First.Metadata.IsPrimarilyVariable)
		{
			Unit.Set(First.Metadata.Variable, Result);
		}
	}

	private static bool IsPowerOfTwo(long x)
	{
    	return (x & (x - 1)) == 0;
	}

	private static bool IsConstantValidForExtendedMultiplication(long x)
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
		
		return Second.Value.Type == HandleType.CONSTANT ? new ConstantMultiplication(First, Second) : null;
	}

	public override void OnBuild()
	{
		var flags = ParameterFlag.DESTINATION | (Assigns ? ParameterFlag.WRITE_ACCESS : ParameterFlag.NONE);

		// Handle decimal multiplication separately
		if (Result.Format.IsDecimal())
		{
			var instruction = Assembler.IsTargetX86 ? SINGLE_PRECISION_MULTIPLICATION_INSTRUCTION : DOUBLE_PRECISION_MULTIPLICATION_INSTRUCTION;

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
							new Result(count, Assembler.Format),
							ParameterFlag.NONE,
							HandleType.CONSTANT
						)
					);

					return;
				}
				else if (IsConstantValidForExtendedMultiplication(value - 1))
				{
					// Example: imul rax, 3 => lea ..., [rax*2+rax]
					var calculation = new CalculationHandle
					(
						constant_multiplication.Other, 
						(int)value - 1, 
						constant_multiplication.Other, 
						0
					);

					Build(
						EXTENDED_MULTIPLICATION_INSTRUCTION,
						new InstructionParameter(
							Result,
							ParameterFlag.DESTINATION,
							HandleType.REGISTER
						),
						new InstructionParameter(
							new Result(calculation, Result.Format),
							ParameterFlag.NONE,
							HandleType.CALCULATION
						)
					);

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
		if (Result.Format.IsDecimal())
		{
			return First;
		}

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