public class AdditionInstruction : DualParameterInstruction
{
	private const string STANDARD_ADDITION_INSTRUCTION = "add";

	private const int STANDARD_ADDITION_FIRST = 0;
	private const int STANDARD_ADDITION_SECOND = 1;

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

	public override void OnBuild()
	{
		if (First.Format.IsDecimal() || Second.Format.IsDecimal())
		{
			var instruction = Assembler.IsTargetX86 ? SINGLE_PRECISION_ADDITION_INSTRUCTION : DOUBLE_PRECISION_ADDITION_INSTRUCTION;

			// NOTE: Changed the parameter flag to none because any attachment could override the contents of the destination register and the variable should move to an appropriate register attaching the variable there
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

		if (Assigns)
		{
			Build(
				STANDARD_ADDITION_INSTRUCTION,
				First.Size,
				new InstructionParameter(
					First,
					ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH | ParameterFlag.READS,
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

			return;
		}

		if (First.IsExpiring(Position))
		{
			Build(
				STANDARD_ADDITION_INSTRUCTION,
				Assembler.Size,
				new InstructionParameter(
					First,
					ParameterFlag.DESTINATION | ParameterFlag.READS,
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

			return;
		}

		var calculation = ExpressionHandle.CreateAddition(First, Second);

		Build(
			EXTENDED_ADDITION_INSTRUCTION,
			Assembler.Size,
			new InstructionParameter(
				Result,
				ParameterFlag.DESTINATION,
				HandleType.REGISTER
			),
			new InstructionParameter(
				new Result(calculation, Assembler.Format),
				ParameterFlag.NONE,
				HandleType.EXPRESSION
			)
		);
	}

	public override bool Redirect(Handle handle)
	{
		if (Operation == SINGLE_PRECISION_ADDITION_INSTRUCTION || Operation == DOUBLE_PRECISION_ADDITION_INSTRUCTION || Assigns)
		{
			return false;
		}

		var first = Parameters[STANDARD_ADDITION_FIRST];
		var second = Parameters[STANDARD_ADDITION_SECOND];

		if (handle.Type == HandleType.REGISTER && first.IsAnyRegister && (second.IsAnyRegister || second.IsConstant))
		{
			Operation = EXTENDED_ADDITION_INSTRUCTION;

			var calculation = ExpressionHandle.CreateAddition(first.Value!, second.Value!);

			Parameters.Clear();
			Parameters.Add(new InstructionParameter(handle, ParameterFlag.DESTINATION));
			Parameters.Add(new InstructionParameter(calculation, ParameterFlag.NONE));

			return true;
		}

		return false;
	}

	public override Result? GetDestinationDependency()
	{
		return (First.IsExpiring(Position) || Assigns) ? First : Result;
	}
}