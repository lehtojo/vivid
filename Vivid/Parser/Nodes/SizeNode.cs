public class SizeNode : Node, IType
{
	public Type Type { get; private set; }

	public SizeNode(Type type)
	{
		Type = type;
	}

	public new Type GetType()
	{
		return Types.LARGE;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.SIZE;
	}
}