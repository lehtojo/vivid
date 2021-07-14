public class ListNode : Node
{
	public ListNode(Position? position, params Node[] nodes)
	{
		Position = position;
		Instance = NodeType.LIST;

		foreach (var node in nodes)
		{
			Add(node);
		}
	}

	public override string ToString() => "List";
}