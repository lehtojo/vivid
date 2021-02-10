/// <summary>
/// Appends the specified label to the generated assembly
/// This instruction works on all architectures
/// </summary>
public class LabelInstruction : Instruction
{
	public Label Label { get; private set; }

	public LabelInstruction(Unit unit, Label label) : base(unit, InstructionType.LABEL)
	{
		Label = label;
		Description = $"{Label.GetName()}:";
	}

	public override void OnBuild()
	{
		Build($"{Label.GetName()}:");
	}
}