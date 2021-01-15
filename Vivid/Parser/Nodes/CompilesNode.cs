/// <summary>
/// Represents a node which outputs true if the content of the node is compiled successfully otherwise it returns false
/// </summary>
public class CompilesNode : Node
{
	public CompilesNode(Position? position = null)
	{
		Position = position;
	}

	public override Type? TryGetType()
	{
		return Types.BOOL;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.COMPILES;
	}
}