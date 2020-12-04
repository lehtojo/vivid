public class ContentNode : Node, IType
{
	public ContentNode(Position? position = null) 
	{
		Position = position;
	}
	
	public new Type? GetType()
	{
		var type = First as IType;
		return type?.GetType();
	}

	public override NodeType GetNodeType()
	{
		return NodeType.CONTENT;
	}
}
