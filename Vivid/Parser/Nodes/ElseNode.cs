public class ElseNode : Node, IResolvable
{
	public Node? Predecessor => (Previous?.Is(NodeType.IF, NodeType.ELSE_IF) ?? false) ? Previous : null;

	public ContextNode Body => First!.To<ContextNode>();

	public ElseNode(Context context, Node body, Position? position = null)
	{
		Position = position;
		Instance = NodeType.ELSE;

		Add(new ContextNode(context));

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