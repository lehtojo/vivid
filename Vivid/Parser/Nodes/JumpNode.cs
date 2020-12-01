public class JumpNode : Node
{
	public Label Label { get; private set; }
	public bool IsConditional { get; set; } = false;

	public JumpNode(Label label)
	{
		Label = label;
	}

	public JumpNode(Label label, bool is_conditional)
	{
		Label = label;
		IsConditional = is_conditional;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.JUMP;
	}
}