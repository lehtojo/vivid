using System;

public class StringNode : Node, ICloneable
{
	public string Text { get; private set; }
	public string? Identifier { get; private set; }

	public StringNode(string text, Position? position)
	{
		Text = text;
		Position = position;
		Instance = NodeType.STRING;
	}

	public string GetIdentifier(Unit unit)
	{
		return Identifier ??= unit.GetNextString() ?? throw new ApplicationException("String did not have an identifier");
	}

	public override Type TryGetType()
	{
		return Types.LINK;
	}

	public new object Clone()
	{
		return new StringNode(Text, Position?.Clone()) { Identifier = Identifier };
	}

	public override bool Equals(object? other)
	{
		return other is StringNode node &&
				base.Equals(other) &&
				Text == node.Text;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Text);
	}
}