public class CompilesNode : Node, IType
{
	public CompilesNode(Position? position = null)
	{
		Position = position;
	}

	public new Type GetType()
	{
		return Types.BOOL;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.COMPILES;
	}
}