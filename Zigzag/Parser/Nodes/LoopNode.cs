public class LoopNode : Node
{
	public Context Context { get; private set; }
	public bool IsForever => First == Last;
	public Node Initialization => First.First;
	public Node Condition => Initialization.Next;
	public Node Action => First.Last;
	public Node Body => Last;

	public LoopNode(Context context, Node steps, Node body)
	{
		Context = context;

		if (steps != null)
		{
			Add(steps);
		}

		Add(body);
	}

	public override NodeType GetNodeType()
	{
		return NodeType.LOOP_NODE;
	}
}
