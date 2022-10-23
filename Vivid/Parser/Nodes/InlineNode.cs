using System;

public class InlineNode : Node
{
	public InlineNode(Position? position = null)
	{
		Position = position;
		Instance = NodeType.INLINE;
	}

	public override Type? TryGetType()
	{
		return Last?.TryGetType();
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position);
	}

	public override string ToString() => "Inline";
}
