public class ElseIfNode : IfNode
{
	public ElseIfNode(Context context, Node condition, Node body, Position? position = null) : base(context, condition, body, position) 
	{
		Instance = NodeType.ELSE_IF;
	}

	public IfNode GetRoot()
	{
		var iterator = Predecessor;

		while (!iterator!.Is(NodeType.IF))
		{
			iterator = iterator.To<ElseIfNode>().Predecessor;
		}

		return iterator.To<IfNode>();
	}
}