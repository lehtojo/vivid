public class NegateNode : Node, IResolvable
{
	public Node Object => First!;

	public NegateNode(Node target, Position? position = null)
	{
		Add(target);
		Position = position;
		Instance = NodeType.NEGATE;
	}

	public override Type? TryGetType()
	{
		return Object.TryGetType();
	}

	public Node? Resolve(Context context)
	{
		Resolver.Resolve(context, Object);
		return null;
	}

	public Status GetStatus()
	{
		return TryGetType() is Number ? Status.OK : new Status(Position, "Can not resolve the negation operation");
	}

	public override string ToString() => "Negate";
}