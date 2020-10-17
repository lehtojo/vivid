public class CastNode : Node, IType, IResolvable
{
	public Node Object => First!;

	public CastNode(Node target, Node type)
	{
		Add(target);
		Add(type);
	}

	public new Type? GetType()
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