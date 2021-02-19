/// <summary>
/// Determines the PC-relative address to the specified identifier
/// This instruction is works only on architecture Arm64
/// </summary>
public class GetRelativeAddressInstruction : Instruction
{
	public DataSectionHandle Handle { get; private set; }

	public GetRelativeAddressInstruction(Unit unit, DataSectionHandle handle) : base(unit, InstructionType.GET_RELATIVE_ADDRESS)
	{
		// Copy the data section handle and the only the address
		Handle = (DataSectionHandle)handle.Finalize();
		Handle.GlobalOffsetTable = true;
		Handle.Address = true;

		Result.Format = Assembler.Format;
	}

	public override void OnBuild()
	{
		Memory.GetResultRegisterFor(Unit, Result, false);
		Result.Format = Assembler.Format;

		Build(
			Instructions.Arm64.LOAD_RELATIVE_ADDRESS,
			new InstructionParameter(
				Result,
				ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS,
				HandleType.REGISTER
			),
			new InstructionParameter(
				new Result(Handle, Assembler.Format),
				ParameterFlag.BIT_LIMIT_64 | ParameterFlag.ALLOW_ADDRESS,
				HandleType.MEMORY
			)
		);
	}
}