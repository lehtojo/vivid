public class StringNode : Node, Contextable
{
	public string Text { get; private set; }
	public string Identifier { get; set; }

	public StringNode(string text)
	{
		Text = text;
	}

	public Type GetContext()
	{
		return Types.LINK;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.STRING_NODE;
	}
}