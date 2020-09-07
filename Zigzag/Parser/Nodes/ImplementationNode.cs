public class ImplementationNode : Node, IContext
{
	public Context Context { get; private set; }

	public ImplementationNode(FunctionImplementation implementation)
	{
		Context = implementation;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.IMPLEMENTATION_NODE;
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