public class ElseIfNode : IfNode
{
	public ElseIfNode(Context context, Node condition, Node body) : base(context, condition, body) { }

	public IfNode GetRoot()
	{
		var iterator = Predecessor;

		while (!iterator!.Is(NodeType.IF))
		{
			iterator = iterator.To<ElseIfNode>().Predecessor;
		}

		return iterator.To<IfNode>();
	}

	public override NodeType GetNodeType()
	{
		return NodeType.ELSE_IF;
	}
}