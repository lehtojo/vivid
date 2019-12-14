public class ContentNode : Node, IType
{
	public Type GetType()
	{
		var type = First as IType;
		return type.GetType();
	}

	public override NodeType GetNodeType()
	{
		return NodeType.CONTENT_NODE;
	}
}
