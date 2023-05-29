public class ContextNode : Node
{
	public Context Context { get; private set; }

	public ContextNode(Context context, Position position)
	{
		Instance = NodeType.CONTEXT;
		Context = context;
		Position = position;
	}

	public override string ToString()
	{
		return "Context " + Context.Identity;
	}
}