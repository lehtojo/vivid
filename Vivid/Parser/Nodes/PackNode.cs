public class PackNode : Node
{
	public Type Type { get; }

	public PackNode(Type type)
	{
		Type = type;
		Instance = NodeType.PACK;
	}

	public override Type? TryGetType()
	{
		return Type;
	}
}