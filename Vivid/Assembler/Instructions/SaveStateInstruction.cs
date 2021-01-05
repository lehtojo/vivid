using System.Collections.Generic;

/// <summary>
/// Saves the current state of the unit
/// This instruction works on all architectures
/// </summary>
public class SaveStateInstruction : Instruction
{
	public Instruction Perspective { get; private set; }
	public List<VariableState>? State { get; private set; }

	public SaveStateInstruction(Unit unit) : base(unit, InstructionType.SAVE)
	{
		Perspective = this;
	}

	public override void OnBuild()
	{
		// Get state that only contains important variables from the position of the perspective
		State = Unit.GetState(Perspective.Position);
	}
}