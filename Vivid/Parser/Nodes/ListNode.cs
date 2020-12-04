public class ListNode : Node
{
	public ListNode(Position? position, params Node[] nodes)
	{
		Position = position;
		
		foreach (var node in nodes)
		{
			Add(node);
		}
	}
}