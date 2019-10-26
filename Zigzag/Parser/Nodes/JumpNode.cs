public class JumpNode : Node
{
	public Label Label { get; private set; }

	public JumpNode(Label label)
	{
		Label = label;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.JUMP_NODE;
	}
}