public class StringNode : Node, IType
{
	public string Text { get; private set; }
	public string Identifier { get; set; }

	public StringNode(string text)
	{
		Text = text;
	}

	public Type GetType()
	{
		return Types.LINK;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.STRING_NODE;
	}
}