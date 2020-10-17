using System;

public class StringNode : Node, IType, ICloneable
{
	public string Text { get; private set; }
	public string? Identifier { get; private set; }

	public StringNode(string text)
	{
		Text = text;
	}

	public string GetIdentifier(Unit unit)
	{
		return Identifier ??= unit.GetNextString() ?? throw new ApplicationException("String did not have an identifier");
	}

	public new Type? GetType()
	{
		return Types.LINK;
	}

	public object Clone()
	{
		return new StringNode(Text)
		{
			Identifier = Identifier
		};
	}

	public override NodeType GetNodeType()
	{
		return NodeType.STRING;
	}

	public override bool Equals(object? obj)
	{
		return obj is StringNode node &&
				base.Equals(obj) &&
				Text == node.Text;
	}

	public override int GetHashCode()
	{
		HashCode hash = new HashCode();
		hash.Add(base.GetHashCode());
		hash.Add(Text);
		return hash.ToHashCode();
	}
}