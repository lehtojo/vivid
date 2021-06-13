public class NotNode : Node
{
	public Node Object => First!;

	public NotNode(Node target, Position? position)
	{
		Add(target);
		Position = position;
		Instance = NodeType.NOT;
	}

	public override Type? TryGetType()
	{
		return Object.TryGetType();
	}

	public override string ToString() => "Not";
}