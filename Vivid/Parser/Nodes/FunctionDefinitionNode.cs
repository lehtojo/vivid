using System;

public class FunctionDefinitionNode : Node
{
	public Function Function { get; private set; }

	public FunctionDefinitionNode(Function function, Position? position = null)
	{
		Function = function;
		Position = position;
		Instance = NodeType.FUNCTION_DEFINITION;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Function);
	}
}