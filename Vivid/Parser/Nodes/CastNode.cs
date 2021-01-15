/// <summary>
/// Represents a cast node which is used for converting types to other types
/// </summary>
public class CastNode : Node, IResolvable
{
	public Node Object => First!;

	public CastNode(Node target, Node type, Position? position = null)
	{
		Position = position;
		Add(target);
		Add(type);
	}

	public override Type? TryGetType()
	{
		return Last?.TryGetType();
	}

	private static void Resolve(Context context, Node node)
	{
		Resolver.Resolve(context, node);
	}

	public Node? Resolve(Context context)
	{
		Resolve(context, First!);
		Resolve(context, Last!);

		return null;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.CAST;
	}

	public Status GetStatus()
	{
		return Status.OK;
	}
}