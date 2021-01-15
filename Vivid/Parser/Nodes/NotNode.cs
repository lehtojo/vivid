public class NotNode : Node
{
	public Node Object => First!;

	public NotNode(Node target, Position? position)
	{
		Add(target);
		Position = position;
	}

	public override Type? TryGetType()
	{
		return Object.TryGetType();
	}

	public override NodeType GetNodeType()
	{
		return NodeType.NOT;
	}
}