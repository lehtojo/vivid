public class ElseNode : Node, IResolvable, IContext
{
	public Node? Predecessor => (Previous?.Is(NodeType.IF, NodeType.ELSE_IF) ?? false) ? Previous : null;

	public Context Context { get; set; }
	public Node Body => First!;

	public ElseNode(Context context, Node body, Position? position = null)
	{
		Context = context;
		Position = position;

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
		Resolver.Resolve(Context, Body);

		return null;
	}

	public Status GetStatus()
	{
		return Status.OK;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.ELSE;
	}

	public void SetContext(Context context)
	{
		Context = context;
	}

	public Context GetContext()
	{
		return Context;
	}
}