public class IfNode : Node
{
	public Context Context { get; set; }
	public Node Successor { get; private set; }

	public Node Condition => First;
	public Node Body => Last;

	public IfNode(Context context, Node condition, Node body)
	{
		Context = context;

		Add(condition);
		Add(body);
	}

	public void SetSuccessor(Node successor)
	{
		Successor = successor;
		Insert(Last, Successor);
	}

	public override NodeType GetNodeType()
	{
		return NodeType.IF_NODE;
	}
}