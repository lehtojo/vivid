using System;

public class RestoreStateInstruction : Instruction
{
	public SaveStateInstruction Save { get; private set; }

	public RestoreStateInstruction(Unit unit, SaveStateInstruction save) : base(unit)
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

	public override Result? GetDestinationDependency()
	{
		return null;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.RESTORE;
	}

	public override Result[] GetResultReferences()
	{
		return new Result[] { Result };
	}
}