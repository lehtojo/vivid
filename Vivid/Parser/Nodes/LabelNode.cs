public class LabelNode : Node
{
	public Label Label { get; private set; }

	public LabelNode(Label label, Position? position = null)
	{
		Label = label;
		Position = position;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.LABEL;
	}
}