using System;

public class StackAddressNode : Node
{
	public int Alignment { get; set; }
	public int Bytes { get; set; }

	public StackAddressNode(int bytes)
	{
		Instance = NodeType.STACK_ADDRESS;
		Alignment = 0;
		Bytes = bytes;
	}

	public override Type? TryGetType()
	{
		return Types.LINK;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Alignment, Bytes);
	}
}