public class ImplementationNode : Node, IContext
{
	public FunctionImplementation Implementation { get; private set; }

	public ImplementationNode(FunctionImplementation implementation)
	{
		Implementation = implementation;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.IMPLEMENTATION_NODE;
	}

   public Context GetContext()
   {
		return Implementation;
   }
}