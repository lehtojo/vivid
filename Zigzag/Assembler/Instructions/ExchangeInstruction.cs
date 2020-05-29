
public class ExchangeInstruction : DualParameterInstruction
{
	public const string INSTRUCTION = "xchg";

	public bool IsSafe { get; private set; }

	public ExchangeInstruction(Unit unit, Result first, Result second, bool is_safe) : base(unit, first, second) 
	{
		IsSafe = is_safe;
	}

	public override void OnBuild()
	{
		Build(
			INSTRUCTION,
			Assembler.Size,
			new InstructionParameter(
				First,
				ParameterFlag.DESTINATION | ParameterFlag.RELOCATE_TO_SOURCE | (IsSafe ? ParameterFlag.NONE : ParameterFlag.WRITE_ACCESS),
				HandleType.REGISTER
			),
			new InstructionParameter(
				Second,
				ParameterFlag.SOURCE | ParameterFlag.RELOCATE_TO_DESTINATION,
				HandleType.REGISTER
			)
		);
	}

	public override Result? GetDestinationDependency()
	{
		return null;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.EXCHANGE;
	}
}