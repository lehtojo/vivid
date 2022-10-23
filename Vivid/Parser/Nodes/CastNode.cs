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

		var a = from.GetSupertypeBaseOffset(to);
		var b = to.GetSupertypeBaseOffset(from);

		// 1. Return true if both of the types have nothing in common: a == null && b == null
		// 2. If either a or b is zero, no offset is required, so the cast is free: a == 0 || b == 0
		// Result: (a == null && b == null) || (a == 0 || b == 0)
		if (a == null) return b == null || b == 0;
		return a == 0 || b == 0;
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
		// If the casted object is a pack construction:
		// - Set the target type of the pack construction to the target type of this cast
		// - Replace this cast node with the pack construction by returning it
		if (First!.Instance == NodeType.PACK_CONSTRUCTION)
		{
			First.To<PackConstructionNode>().Type = Last!.To<TypeNode>().Type;
			return First;
		}

		Resolve(context, First!);
		Resolve(context, Last!);

		return null;
	}

	public Status GetStatus()
	{
		return Status.OK;
	}

	public override string ToString() => "Cast";
}