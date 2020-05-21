
public class ExchangeInstruction : DualParameterInstruction
{
	public const string INSTRUCTION = "xchg";

	public ExchangeInstruction(Unit unit, Result first, Result second) : base(unit, first, second) {}

	public override void OnBuild()
	{
		Build(
			INSTRUCTION,
			Assembler.Size,
			new InstructionParameter(
				First,
				ParameterFlag.DESTINATION | ParameterFlag.RELOCATE_TO_SOURCE,
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