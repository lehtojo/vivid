using System;

public class StackAddressNode : Node
{
	public string Identity { get; set; }
	public int Alignment { get; set; }
	public int Bytes { get; set; }

	public StackAddressNode(Context context, int bytes)
	{
		Identity = context.CreateStackAddress();
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
		return HashCode.Combine(Instance, Position, Identity, Alignment, Bytes);
	}
}