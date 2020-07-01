public class CastNode : Node, IType, IResolvable
{
	public CastNode(Node target, Node type)
	{
		Add(target);
		Add(type);
	}

	public new Type? GetType()
	{
		if (Last is IType type)
		{
			return type.GetType();
		}

		return Types.UNKNOWN;
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
		return NodeType.CAST_NODE;
	}

	public Status GetStatus()
	{
		return Status.OK;
	}
}