/// <summary>
/// This instruction requests a block of memory from the stack and returns a handle to it.
/// This instruction is works on all architectures
/// </summary>
public class AllocateStackInstruction : Instruction
{
	public string Identity { get; private set; }
	public int Bytes { get; private set; }

	public AllocateStackInstruction(Unit unit, StackAddressNode node) : base(unit, InstructionType.ALLOCATE_STACK)
	{
		Identity = node.Identity;
		Bytes = node.Bytes;
		IsAbstract = true;
	}

	public override void OnBuild()
	{
		Result.Value = new StackAllocationHandle(Unit, Bytes, Identity);
		Result.Format = Settings.Format;
	}
}