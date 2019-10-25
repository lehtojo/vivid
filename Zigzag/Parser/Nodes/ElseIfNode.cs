public class ElseIfNode : IfNode
{
	public ElseIfNode(Context context, Node condition, Node body) : base(context, condition, body) {}

	public override NodeType GetNodeType()
	{
		return NodeType.ELSE_IF_NODE;
	}
}