public class CallNode : Node, IType
{
	public Node Self => First!;
	public Node Pointer => First!.Next!;
	public Node Parameters => Last!;
	public CallDescriptorType Descriptor { get; private set; }

	public CallNode(Node self, Node pointer, Node parameters, CallDescriptorType descriptor)
	{
		Descriptor = descriptor;

		Add(self);
		Add(pointer);
		Add(new ListNode(parameters.Position));

		foreach (var parameter in parameters)
		{
			Parameters.Add(parameter);
		}
	}

	public new Type? GetType()
	{
		return Descriptor.ReturnType;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.CALL;
	}
}