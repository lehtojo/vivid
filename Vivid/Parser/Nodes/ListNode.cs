public class ListNode : Node
{
	public ListNode(params Node[] nodes)
	{
		foreach (var node in nodes)
		{
			Add(node);
		}
	}
}