/// <summary>
/// Represents a cast node which is used for converting types to other types
/// </summary>
public class CastNode : Node, IResolvable
{
	public Node Object => First!;

	public CastNode(Node target, Node type, Position? position = null)
	{
		Position = position;
		Instance = NodeType.CAST;
		
		Add(target);
		Add(type);
	}

	public bool IsFree()
	{
		var from = Object.GetType();
		var to = GetType();

		return from.GetSupertypeBaseOffset(to) == 0 || to.GetSupertypeBaseOffset(from) == 0;
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

	public Status GetStatus()
	{
		return Status.OK;
	}
}