public class FunctionDefinitionNode : Node
{
   public Function Function { get; private set; }

	public FunctionDefinitionNode(Function function)
	{
      Function = function;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.FUNCTION_DEFINITION_NODE;
	}
}