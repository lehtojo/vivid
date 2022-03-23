public class ObjectUnlinkNode : Node
{
	public Node Value => First!;

	public ObjectUnlinkNode(Node value)
	{
		Add(value);
		Instance = NodeType.OBJECT_UNLINK;
	}

	public override Type? TryGetType()
	{
		return Primitives.CreateBool();
	}
}