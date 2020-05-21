public class NegateNode : Node, IType
{
	public Node Target => First!;

	public NegateNode(Node target)
	{
		Add(target);
	}

	public new Type? GetType()
	{
		return Target is IType x ? x.GetType() : Types.UNKNOWN;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.NEGATE_NODE;
	}
}