public class ContentNode : Node, Contextable
{
	public Type GetContext()
	{
		Contextable contextable = (Contextable)First;
		return contextable.GetContext();
	}

	public override NodeType GetNodeType()
	{
		return NodeType.CONTENT_NODE;
	}
}
