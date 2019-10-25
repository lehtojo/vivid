public class LoopNode : Node
{
	public Context Context { get; private set; }
	public bool IsForever => First == Last;
	public Node Start => First.First;
	public Node Condition => Start.Next;
	public Node Action => First.Last;
	public Node Body => Last;

	public LoopNode(Context context, Node condition, Node body)
	{
		Context = context;

		if (condition != null)
		{
			Add(condition);
		}

		Add(body);
	}

	public override NodeType GetNodeType()
	{
		return NodeType.LOOP_NODE;
	}
}
