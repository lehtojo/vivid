public class SubtractionInstruction : DualParameterInstruction
{
	private const string STANDARD_SUBTRACTION_INSTRUCTION = "sub";

	private const string SINGLE_PRECISION_SUBTRACTION_INSTRUCTION = "subss";
	private const string DOUBLE_PRECISION_SUBTRACTION_INSTRUCTION = "subsd";

	public bool Assigns { get; private set; }
	
	public SubtractionInstruction(Unit unit, Result first, Result second, Format format, bool assigns) : base(unit, first, second, format)
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

	public override void OnBuild()
	{
		// Handle decimal division separately
		if (Result.Format.IsDecimal())
		{
			var instruction = Assembler.IsTargetX86 ? SINGLE_PRECISION_SUBTRACTION_INSTRUCTION : DOUBLE_PRECISION_SUBTRACTION_INSTRUCTION;
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

		if (Assigns)
		{
			Build(
				STANDARD_SUBTRACTION_INSTRUCTION,
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
				STANDARD_SUBTRACTION_INSTRUCTION,
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

	public override Result GetDestinationDependency()
	{
		return First;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.SUBTRACT;
	}
}