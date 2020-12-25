/// <summary>
/// This instruction requests a block of memory from the stack and returns a handle to it.
/// This instruction is works in all architectures
/// </summary>
public class AllocateStackInstruction : Instruction
{
	public int Bytes { get; private set; }

	public AllocateStackInstruction(Unit unit, int bytes) : base(unit) 
	{
		Bytes = bytes;
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

	public override Result? GetDestinationDependency()
	{
		return null;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.ALLOCATE_STACK;
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result };
	}
}