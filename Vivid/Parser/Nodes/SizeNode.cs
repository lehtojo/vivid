using System;

public class SizeNode : Node
{
	public Type Type { get; private set; }

	public SizeNode(Type type)
	{
		Type = type;
		Instance = NodeType.SIZE;
	}

	public override Type? TryGetType()
	{
		return Types.LARGE;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Type.Identity);
	}
}