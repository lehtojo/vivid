public class ArrayAllocationNode : Node, IType
{
    public Type Type { get; private set; }
    public Node Length => First!;

	public ArrayAllocationNode(Type type, Node length)
	{
        Type = type;
		Add(length);
	}

    public new Type? GetType()
    {
        return Type;
    }

	public override NodeType GetNodeType()
	{
		return NodeType.ARRAY_ALLOCATION;
	}
}