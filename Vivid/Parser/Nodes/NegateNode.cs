public class NegateNode : Node, IResolvable
{
	public Node Object => First!;

	public NegateNode(Node target, Position? position = null)
	{
		Add(target);
		Position = position;
	}

	public override Type? TryGetType()
	{
		return Object.TryGetType();
	}

	public override NodeType GetNodeType()
	{
		return NodeType.NEGATE;
	}

	public Node? Resolve(Context context)
	{
		Resolver.Resolve(context, Object);
		return null;
	}

	public Status GetStatus()
	{
		// Ensure the object is a number
		return TryGetType() is Number ? Status.OK : Status.Error(Position, "Could not resolve the negation operation");
	}
}