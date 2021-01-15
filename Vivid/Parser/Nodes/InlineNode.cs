using System;

public class InlineNode : Node
{
	public InlineNode(Position? position = null)
	{
		Position = position;
	}

	public override Type? TryGetType()
	{
		return Last?.TryGetType();
	}

	public override NodeType GetNodeType()
	{
		return NodeType.INLINE;
	}
}
