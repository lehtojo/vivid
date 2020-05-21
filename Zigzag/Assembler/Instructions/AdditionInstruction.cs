public class AdditionInstruction : DualParameterInstruction
{
	private const string STANDARD_ADDITION_INSTRUCTION = "add";
	private const string EXTENDED_ADDITION_INSTRUCTION = "lea";

	private const string SINGLE_PRECISION_ADDITION_INSTRUCTION = "addss";
	private const string DOUBLE_PRECISION_ADDITION_INSTRUCTION = "addsd";

	public bool Assigns { get; private set; }
	public new Format Type { get; private set; }

	public AdditionInstruction(Unit unit, Result first, Result second, Format type, bool assigns) : base(unit, first, second) 
	{
		Type = type;

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
      	Unit.Scope!.Variables[First.Metadata.Variable] = Result;
			Result.Metadata.Attach(new VariableAttribute(First.Metadata.Variable));
		}
	}
	
	public override void OnBuild()
	{
		// Handle decimal addition separately
		if (Type == global::Format.DECIMAL)
		{
			var instruction = Assembler.Size.Bits == 32 ? SINGLE_PRECISION_ADDITION_INSTRUCTION : DOUBLE_PRECISION_ADDITION_INSTRUCTION;
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
			// Form the calculation parameter
			var calculation = Format(
				"[{0}+{1}]",
				Assembler.Size,
				new InstructionParameter(
					First,
					ParameterFlag.NONE,
					HandleType.CONSTANT,
					HandleType.REGISTER
				),
				new InstructionParameter(
					Second,
					ParameterFlag.NONE,
					HandleType.CONSTANT,
					HandleType.REGISTER
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

			Build($"{EXTENDED_ADDITION_INSTRUCTION} {Result}, {calculation}");
		}
	}

	public override Result? GetDestinationDependency()
	{
		if (First.IsExpiring(Position))
		{
			return First;
		}
		else
		{
			return Result;
		}
	}
}