public class SizeNode : Node
{
	public Type Type { get; private set; }

	public SizeNode(Type type)
	{
		Type = type;
	}

	public override Type? TryGetType()
	{
		return Types.LARGE;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.SIZE;
	}
}