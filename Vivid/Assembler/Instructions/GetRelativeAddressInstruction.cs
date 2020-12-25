/// <summary>
/// Determines the PC-relative address to the specified identifier
/// This instruction is works only on architecture Arm64
/// </summary>
public class GetRelativeAddressInstruction : Instruction
{
	public const string ARM64_RELATIVE_ADDRESS_INSTRUCTION = "adrp";

	public DataSectionHandle Handle { get; private set; }

	public GetRelativeAddressInstruction(Unit unit, DataSectionHandle handle) : base(unit)
	{
		// Copy the data section handle and the only the address
		Handle = (DataSectionHandle)handle.Finalize();
		Handle.Address = true;

		Result.Format = Assembler.Format;
	}

	public override void OnBuild()
	{
		Memory.GetResultRegisterFor(Unit, Result, false);
		Result.Format = Assembler.Format;

		Build(
			ARM64_RELATIVE_ADDRESS_INSTRUCTION,
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

		if (Handle.Offset != 0)
		{
			// Add the handle offset
			Unit.Append(new AdditionInstruction(Unit, Result, new Result(new ConstantHandle(Handle.Offset), Assembler.Format), Assembler.Format, true), true);
		}
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result };
	}

	public override Result? GetDestinationDependency()
	{
		return Result;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.GET_RELATIVE_ADDRESS;
	}
}