public class PackNode : Node
{
	public Type Type { get; set; }

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