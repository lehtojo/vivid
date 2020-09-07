public class AdditionInstruction : DualParameterInstruction
{
	private const string STANDARD_ADDITION_INSTRUCTION = "add";
	private const string EXTENDED_ADDITION_INSTRUCTION = "lea";

	private const string SINGLE_PRECISION_ADDITION_INSTRUCTION = "addss";
	private const string DOUBLE_PRECISION_ADDITION_INSTRUCTION = "addsd";

	public bool Assigns { get; private set; }

	public AdditionInstruction(Unit unit, Result first, Result second, Format format, bool assigns) : base(unit, first, second, format) 
	{
		if (Assigns = assigns)
		{
			Result.Metadata = First.Metadata;
		}
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.ADDITION;
	}

	public override void OnSimulate()
	{
		if (Assigns && First.Metadata.IsPrimarilyVariable)
		{
			Unit.Set(First.Metadata.Variable, Result);
		}
	}
	
	public override void OnBuild()
	{
		if (Result.Format.IsDecimal())
		{
			var instruction = Assembler.IsTargetX86 ? SINGLE_PRECISION_ADDITION_INSTRUCTION : DOUBLE_PRECISION_ADDITION_INSTRUCTION;
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

		if (First.IsExpiring(Position) || Assigns)
		{
			if (Assigns)
			{
				Build(
					STANDARD_ADDITION_INSTRUCTION,
					Assembler.Size,
					new InstructionParameter(
						First,
						ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS,
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
			}
			else
			{
				Build(
					STANDARD_ADDITION_INSTRUCTION,
					Assembler.Size,
					new InstructionParameter(
						First,
						ParameterFlag.DESTINATION,
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
		}
		else
		{
			var calculation = CalculationHandle.CreateAddition(First, Second);

			Build(
				EXTENDED_ADDITION_INSTRUCTION,
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
		}
	}

	public override Result? GetDestinationDependency()
	{
		return (First.IsExpiring(Position) || Assigns) ? First : Result;
	}
}