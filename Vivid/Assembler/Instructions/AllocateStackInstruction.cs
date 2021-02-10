/// <summary>
/// This instruction requests a block of memory from the stack and returns a handle to it.
/// This instruction is works on all architectures
/// </summary>
public class AllocateStackInstruction : Instruction
{
	public int Bytes { get; private set; }

	public AllocateStackInstruction(Unit unit, int bytes) : base(unit, InstructionType.ALLOCATE_STACK) 
	{
		Bytes = bytes;
		IsAbstract = true;
	}

	public override void OnSimulate()
	{
		Result.Value = new InlineHandle(Unit, Bytes);
		Result.Format = Assembler.Format;
	}

	public override void OnBuild()
	{
		OnSimulate();
	}
}