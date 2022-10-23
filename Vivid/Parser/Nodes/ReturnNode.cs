public class ReturnNode : Node
{
	public Node? Value => First;

	public ReturnNode(Node? node, Position? position)
	{
		Instance = NodeType.RETURN;
		Position = position;

		// Add the return value, if it exists
		if (node != null) Add(node);
	}

	public override string ToString() => "Return";
}