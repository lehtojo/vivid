public class UndefinedNode : Node
{
	public Type Type { get; }
	public Format Format { get; }

	public UndefinedNode(Type type, Format format)
	{
		Type = type;
		Format = format;
		Instance = NodeType.UNDEFINED;
	}

	public override Type? TryGetType()
	{
		return Type;
	}
}