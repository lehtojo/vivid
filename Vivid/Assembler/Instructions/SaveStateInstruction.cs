using System.Collections.Generic;

/// <summary>
/// Saves the current state of the unit
/// This instruction works on all architectures
/// </summary>
public class SaveStateInstruction : Instruction
{
	public Instruction Perspective { get; private set; }
	public List<VariableState>? State { get; private set; }

	public SaveStateInstruction(Unit unit) : base(unit)
	{
		Perspective = this;
	}

	public override void OnBuild()
	{
		// Get state that only contains important variables from the position of the perspective
		State = Unit.GetState(Perspective.Position);
	}

	public override Result? GetDestinationDependency()
	{
		return null;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.SAVE;
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result };
	}
}