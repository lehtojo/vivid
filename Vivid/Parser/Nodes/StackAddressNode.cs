using System;

public class StackAddressNode : Node
{
	public Type Type { get; }
	public string Identity { get; set; }
	public int Bytes => Math.Max(Type.ContentSize, 1);

	public StackAddressNode(Context context, Type type, Position? position)
	{
		Type = type;
		Identity = context.CreateStackAddress();
		Instance = NodeType.STACK_ADDRESS;
	}

	public override Type? TryGetType()
	{
		return Type;
	}

	public override bool Equals(object? other)
	{
		return other is StackAddressNode node && Identity == node.Identity;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Identity, Bytes);
	}

	public override string ToString() => $"Stack Allocation ({Bytes} bytes)";
}