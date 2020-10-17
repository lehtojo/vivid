public class ContextNode : Node, IResolvable, IContext
{
	public Context Context { get; private set; }

	public ContextNode(Context context)
	{
		Context = context;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.CONTEXT;
	}

	public Context GetContext()
	{
		return Context;
	}

	public Status GetStatus()
	{
		return Status.OK;
	}

	public Node? Resolve(Context context)
	{
		foreach (var iterator in this)
		{
			Resolver.Resolve(Context, iterator);
		}

		return null;
	}

	public void SetContext(Context context)
	{
		Context = context;
	}
}