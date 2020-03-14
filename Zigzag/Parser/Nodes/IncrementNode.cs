public class IncrementNode : Node
{
	public bool Post { get; private set; }
	public Node Object => First!;

	public IncrementNode(Node destination, bool post = false)
	{
		Add(destination);
		Post = post;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.INCREMENT_NODE;
	}
}
