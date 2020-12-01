using System;

public abstract class TemporaryInstruction : Instruction
{
	public TemporaryInstruction(Unit unit) : base(unit) { }

	public override Result? GetDestinationDependency()
	{
		throw new ApplicationException("Tried to build a temporary instruction");
	}

	public override Result[] GetResultReferences()
	{
		throw new ApplicationException("Tried to build a temporary instruction");
	}

	public override int GetStackOffsetChange()
	{
		throw new ApplicationException("Tried to build a temporary instruction");
	}

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