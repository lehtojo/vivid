public class FunctionDefinitionNode : Node
{
	public Function Function { get; private set; }

	public FunctionDefinitionNode(Function function, Position? position = null)
	{
		Function = function;
		Position = position;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.FUNCTION_DEFINITION;
	}
}