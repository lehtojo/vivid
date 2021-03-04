public class ContentNode : Node
{
	public ContentNode(Position? position = null)
	{
		Position = position;
		Instance = NodeType.CONTENT;
	}

	public override Type? TryGetType()
	{
		return First?.TryGetType();
	}
}
