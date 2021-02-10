using System;

public class ImplementationNode : Node, IContext
{
	public Context Context { get; private set; }

	public ImplementationNode(FunctionImplementation implementation, Position? position = null)
	{
		Context = implementation;
		Position = position;
		Instance = NodeType.IMPLEMENTATION;
	}

	public void SetContext(Context context)
	{
		Context = context;
	}

	public Context GetContext()
	{
		return Context;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Context.Identity);
	}
}