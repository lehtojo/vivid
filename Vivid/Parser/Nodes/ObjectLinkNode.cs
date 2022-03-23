public class ObjectLinkNode : Node
{
	public Node Value => First!;

	public ObjectLinkNode(Node value)
	{
		Add(value);
		Instance = NodeType.OBJECT_LINK;
	}
}