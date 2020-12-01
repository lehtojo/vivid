using System.Linq;

public class SubtractionInstruction : DualParameterInstruction
{
	private const string EXTENDED_ADDITION_INSTRUCTION = "lea";

	private const string STANDARD_SUBTRACTION_INSTRUCTION = "sub";

	private const int STANDARD_SUBTRACTION_FIRST = 0;
	private const int STANDARD_SUBTRACTION_SECOND = 1;

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

	public override void OnBuild()
	{
		var flags = ParameterFlag.DESTINATION | (Assigns ? ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH : ParameterFlag.NONE);

		// Handle decimal division separately
		if (First.Format.IsDecimal() || Second.Format.IsDecimal())
		{
			var instruction = Assembler.IsTargetX86 ? SINGLE_PRECISION_SUBTRACTION_INSTRUCTION : DOUBLE_PRECISION_SUBTRACTION_INSTRUCTION;

			Build(
				instruction,
				new InstructionParameter(
					First,
					ParameterFlag.READS | flags,
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
				First.Size,
				new InstructionParameter(
					First,
					ParameterFlag.READS | flags,
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

		Build(
			STANDARD_SUBTRACTION_INSTRUCTION,
			Assembler.Size,
			new InstructionParameter(
				First,
				ParameterFlag.READS | ParameterFlag.DESTINATION,
				HandleType.REGISTER
			),
			new InstructionParameter(
				First,
				ParameterFlag.HIDDEN,
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

	public override bool Redirect(Handle handle)
	{
		if (Operation == SINGLE_PRECISION_SUBTRACTION_INSTRUCTION || Operation == DOUBLE_PRECISION_SUBTRACTION_INSTRUCTION || Assigns)
		{
			return false;
		}

		var first = Parameters[STANDARD_SUBTRACTION_FIRST];
		var second = Parameters[STANDARD_SUBTRACTION_SECOND];

		if (handle.Type == HandleType.REGISTER && first.IsAnyRegister && second.IsConstant)
		{
			Operation = EXTENDED_ADDITION_INSTRUCTION;

			var constant = -(long)second.Value!.To<ConstantHandle>().Value;
			var calculation = CalculationHandle.CreateAddition(first.Value!, new ConstantHandle(constant));

			Parameters.Clear();
			Parameters.Add(new InstructionParameter(handle, ParameterFlag.DESTINATION));
			Parameters.Add(new InstructionParameter(calculation, ParameterFlag.NONE));

			return true;
		}

		return false;
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