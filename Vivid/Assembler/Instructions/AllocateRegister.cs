public class AllocateRegisterInstruction : Instruction
{
	public Format Format { get; }

	public AllocateRegisterInstruction(Unit unit, Format format) : base(unit, InstructionType.ALLOCATE_REGISTER)
	{
		Format = format;
	}

	public override void OnBuild()
	{
		var register = Memory.GetNextRegister(Unit, Format.IsDecimal(), Trace.GetDirectives(Unit, Result), true);
		Result.Value = new RegisterHandle(register);
		Result.Format = Format;
		register.Handle = Result;
	}
}