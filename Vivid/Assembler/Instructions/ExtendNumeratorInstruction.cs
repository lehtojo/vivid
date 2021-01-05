/// <summary>
/// Extends the sign of the quentient register
/// This instruction works only on architecture x86-64
/// </summary>
public class ExtendNumeratorInstruction : Instruction
{
	public const string X64_INSTRUCTION_32BIT_MODE = "cdq";
	public const string X64_INSTRUCTION_64BIT_MODE = "cqo";

	public ExtendNumeratorInstruction(Unit unit) : base(unit, InstructionType.EXTEND_NUMERATOR) { }

	public override void OnBuild()
	{
		var numerator = Unit.GetNumeratorRegister();
		var remainder = Unit.GetRemainderRegister();

		Build(
			Assembler.Is64bit ? X64_INSTRUCTION_64BIT_MODE : X64_INSTRUCTION_32BIT_MODE,
			new InstructionParameter(
				new Result(new RegisterHandle(remainder), Assembler.Format),
				ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH | ParameterFlag.HIDDEN | ParameterFlag.LOCKED,
				HandleType.REGISTER
			),
			new InstructionParameter(
				new Result(new RegisterHandle(numerator), Assembler.Format),
				ParameterFlag.HIDDEN | ParameterFlag.LOCKED,
				HandleType.REGISTER
			)
		);
	}
}
