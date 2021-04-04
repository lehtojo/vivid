/// <summary>
/// Exchanges the values between the operands and then moves the sum of the operands to the destination operand.
/// This is used for destructing objects when they are no longer used.
/// This instruction works only on architecture x86-64.
/// </summary>
public class AtomicExchangeAdditionInstruction : DualParameterInstruction
{
	public AtomicExchangeAdditionInstruction(Unit unit, Result first, Result second, Format format) : base(unit, first, second, format, InstructionType.ATOMIC_EXCHANGE_ADDITION) { }

	public override void OnBuild()
	{
		Build(
			Instructions.X64.ATOMIC_EXCHANGE_ADD,
			new InstructionParameter(
				First,
				ParameterFlag.WRITES | ParameterFlag.READS | ParameterFlag.WRITE_ACCESS,
				HandleType.MEMORY
			),
			new InstructionParameter(
				Second,
				ParameterFlag.DESTINATION | ParameterFlag.READS | ParameterFlag.WRITE_ACCESS,
				HandleType.REGISTER
			)
		);
	}
}
