using System;

public class InlineNode : Node, IType
{
	public InlineNode(Position? position = null)
	{
		Position = position;
	}

	public new Type? GetType()
	{
		return (Last ?? throw new ApplicationException("Found an empty inline node")).TryGetType();
	}

	public override NodeType GetNodeType()
	{
		return NodeType.INLINE;
	}
}
