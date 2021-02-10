using System;

/// <summary>
/// Represents a manual call node which is used for lambda and virtual function calls
/// </summary>
public class CallNode : Node
{
	public Node Self => First!;
	public Node Pointer => First!.Next!;
	public Node Parameters => Last!;
	public CallDescriptorType Descriptor { get; private set; }

	public CallNode(Node self, Node pointer, Node parameters, CallDescriptorType descriptor)
	{
		Descriptor = descriptor;
		Instance = NodeType.CALL;

		Add(self);
		Add(pointer);
		Add(new ListNode(parameters.Position));

		foreach (var parameter in parameters)
		{
			Parameters.Add(parameter);
		}
	}

	public override Type? TryGetType()
	{
		return Descriptor.ReturnType;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Descriptor.Identity);
	}
}