public class NotNode : Node, IType
{
	public Node Object => First!;

	public NotNode(Node target)
	{
		Add(target);
	}

	public new Type? GetType()
	{
		return Object is IType x ? x.GetType() : Types.UNKNOWN;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.NOT;
	}
}