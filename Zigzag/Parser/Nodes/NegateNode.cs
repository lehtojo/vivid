public class NegateNode : Node
{
	public Node Target => First!;

	public NegateNode(Node target)
	{
		Add(target);
	}

	public override NodeType GetNodeType()
	{
		return NodeType.NEGATE_NODE;
	}
}