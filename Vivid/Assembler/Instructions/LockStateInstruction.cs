/// <summary>
/// Sets the lock state of the specified variable
/// This instruction works on all architectures
/// </summary>
public class LockStateInstruction : Instruction
{
	public Register Register { get; private set; }
	public bool IsLocked { get; private set; }

	public static LockStateInstruction Lock(Unit unit, Register register)
	{
		return new LockStateInstruction(unit, register, true);
	}

	public static LockStateInstruction Unlock(Unit unit, Register register)
	{
		return new LockStateInstruction(unit, register, false);
	}

	private LockStateInstruction(Unit unit, Register register, bool locked) : base(unit, InstructionType.LOCK_STATE)
	{
		Register = register;
		IsLocked = locked;
		IsAbstract = true;
		Description = (IsLocked ? "Lock" : "Unlock") + $" '{register.Partitions[0]}'";

		register.IsLocked = locked;
	}

	public override void OnBuild()
	{
		Register.IsLocked = IsLocked;
	}
}