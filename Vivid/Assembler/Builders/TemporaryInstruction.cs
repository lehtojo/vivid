using System;

public abstract class TemporaryInstruction : Instruction
{
	public TemporaryInstruction(Unit unit, InstructionType type) : base(unit, type) { }

	public override void OnBuild()
	{
		throw new ApplicationException("Tried to build a temporary instruction");
	}

	public override void OnPostBuild()
	{
		throw new ApplicationException("Tried to build a temporary instruction");
	}

	public override void OnSimulate()
	{
		throw new ApplicationException("Tried to build a temporary instruction");
	}
}