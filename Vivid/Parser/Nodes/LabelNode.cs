using System;

public class LabelNode : Node
{
	public Label Label { get; set; }

	public LabelNode(Label label, Position? position = null)
	{
		Label = label;
		Position = position;
		Instance = NodeType.LABEL;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Label.GetName());
	}
}