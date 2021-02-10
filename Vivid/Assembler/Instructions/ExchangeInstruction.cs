/// <summary>
/// Exchanges the locations of the specified values
/// This instruction works only on architecture x86-64
/// </summary>
public class ExchangeInstruction : DualParameterInstruction
{
	public const string INSTRUCTION = "xchg";

	public bool IsSafe { get; private set; }

	public ExchangeInstruction(Unit unit, Result first, Result second, bool is_safe) : base(unit, first, second, first.Format, InstructionType.EXCHANGE)
	{
		IsSafe = is_safe;
		IsUsageAnalyzed = false;
	}

	public override void OnBuild()
	{
		Build(
			INSTRUCTION,
			Assembler.Size,
			new InstructionParameter(
				First,
				ParameterFlag.DESTINATION | ParameterFlag.RELOCATE_TO_SOURCE | ParameterFlag.READS | (IsSafe ? ParameterFlag.NONE : ParameterFlag.WRITE_ACCESS),
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