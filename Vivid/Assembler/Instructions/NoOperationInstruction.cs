/// <summary>
/// This instruction does nothing. However, this instruction is used for stopping the debugger.
/// This instruction is works on all architectures
/// </summary>
public class NoOperationInstruction : Instruction
{
	public NoOperationInstruction(Unit unit) : base(unit, InstructionType.NO_OPERATION)
	{
		Operation = Instructions.Shared.NOP;
		IsBuilt = true;
	}
}