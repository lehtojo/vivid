using System;

public class InlineNode : Node
{
	public bool IsContext { get; protected set; } = false;

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
		return HashCode.Combine(Instance, Position, IsContext);
	}

	public override string ToString() => "Inline";
}
