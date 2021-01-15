public class ContentNode : Node
{
	public ContentNode(Position? position = null) 
	{
		Position = position;
	}
	
	public override Type? TryGetType()
	{
		return First?.TryGetType();
	}

	public override NodeType GetNodeType()
	{
		return NodeType.CONTENT;
	}
}
