/// <summary>
/// Exchanges the locations of the specified values.
/// This instruction works only on architecture x86-64.
/// </summary>
public class ExchangeInstruction : DualParameterInstruction
{
	public ExchangeInstruction(Unit unit, Result first, Result second) : base(unit, first, second, first.Format, InstructionType.EXCHANGE)
	{
		IsUsageAnalyzed = false;
	}

	public override void OnBuild()
	{
		Build(
			Instructions.X64.EXCHANGE,
			Assembler.Size,
			new InstructionParameter(
				First,
				ParameterFlag.DESTINATION | ParameterFlag.RELOCATE_TO_SOURCE | ParameterFlag.READS | ParameterFlag.WRITE_ACCESS,
				HandleType.REGISTER
			),
			new InstructionParameter(
				Second,
				ParameterFlag.SOURCE | ParameterFlag.RELOCATE_TO_DESTINATION | ParameterFlag.WRITES | ParameterFlag.READS,
				HandleType.REGISTER
			)
		);
	}
}