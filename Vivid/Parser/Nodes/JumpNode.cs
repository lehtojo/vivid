using System;

public class JumpNode : Node
{
	public Label Label { get; set; }
	public bool IsConditional { get; set; } = false;

	public JumpNode(Label label)
	{
		Instance = NodeType.JUMP;
		Label = label;
	}

	public JumpNode(Label label, bool is_conditional)
	{
		Instance = NodeType.JUMP;
		Label = label;
		IsConditional = is_conditional;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Label.GetName(), IsConditional);
	}
}