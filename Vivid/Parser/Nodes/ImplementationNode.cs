public class ImplementationNode : Node, IContext
{
	public Context Context { get; private set; }

	public ImplementationNode(FunctionImplementation implementation, Position? position = null)
	{
		Context = implementation;
		Position = position;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.IMPLEMENTATION;
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