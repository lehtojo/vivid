public class ElseNode : Node, IResolvable
{
	public Node? Predecessor => (Previous?.Is(NodeType.IF, NodeType.ELSE_IF) ?? false) ? Previous : null;

	public ScopeNode Body => First!.To<ScopeNode>();

	public ElseNode(Context context, Node body, Position? start, Position? end)
	{
		Position = start;
		Instance = NodeType.ELSE;

		Add(new ScopeNode(context, start, end));

		body.ForEach(i => Body.Add(i));
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

	public Node? Resolve(Context context)
	{
		Resolver.Resolve(Body.Context, Body);

		return null;
	}

	public Status GetStatus()
	{
		return Status.OK;
	}
}