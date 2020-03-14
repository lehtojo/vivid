public class ElseNode : Node
{
	public Context Context { get; set; }
	public Node? Body => First;

	public ElseNode(Context context, Node body)
	{
		Context = context;
		Add(body);
	}

	public override NodeType GetNodeType()
	{
		return NodeType.ELSE_NODE;
	}
}