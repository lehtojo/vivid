using System;

/// <summary>
/// Restores the state of the unit
/// This instruction works on all architectures
/// </summary>
public class RestoreStateInstruction : Instruction
{
	public SaveStateInstruction Save { get; private set; }

	public RestoreStateInstruction(Unit unit, SaveStateInstruction save) : base(unit, InstructionType.RESTORE)
	{
		Save = save;
	}

	public override void OnBuild()
	{
		if (Save.State == null)
		{
			throw new InvalidOperationException("Save instruction was not executed before restore instruction");
		}

		Unit.Set(Save.State);
	}
}