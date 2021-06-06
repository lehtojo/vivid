public class ContentNode : Node, IResolvable
{
	public ContentNode(Position? position = null)
	{
		Position = position;
		Instance = NodeType.CONTENT;
	}

	public Node? Resolve(Context context)
	{
		foreach (var node in this) Resolver.Resolve(context, node);
		return null;
	}

	public override Type? TryGetType()
	{
		return First?.TryGetType();
	}

	public Status GetStatus()
	{
		return First != null ? Status.OK : Status.Error(Position, "Empty parenthesis are not allowed");
	}
}
