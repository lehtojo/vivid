public class StackAddressNode : Node, IType
{
	public int Alignment { get; set; }
	public int Bytes { get; set; }

	public StackAddressNode(int bytes)
	{
		Alignment = 0;
		Bytes = bytes;
	}

	public new Type? GetType()
	{
		return Types.LINK;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.STACK_ADDRESS;
	}
}