using System;

public class StringNode : Node, IType
{
	public string Text { get; private set; }
	private string? Identifier { get; set; }

	public StringNode(string text)
	{
		Text = text;
	}

	public string GetIdentifier(Unit? unit)
	{
		return Identifier ?? (Identifier = unit?.GetNextString() ?? throw new ApplicationException("String didn't have an identifier"));
	}

	public new Type? GetType()
	{
		return Types.LINK;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.STRING_NODE;
	}
}