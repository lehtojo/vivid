public class NegateNode : Node, IType
{
	public Node Object => First!;

	public NegateNode(Node target, Position? position = null)
	{
		Add(target);
		Position = position;
	}

	public new Type? GetType()
	{
		return Object is IType x ? x.GetType() : Types.UNKNOWN;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.NEGATE;
	}
}