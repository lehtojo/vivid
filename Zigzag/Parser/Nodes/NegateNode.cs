public class NegateNode : Node
{
	public NegateNode(Node @object)
	{
		Add(@object);
	}

	public override NodeType GetNodeType()
	{
		return NodeType.NEGATE_NODE;
	}
}
