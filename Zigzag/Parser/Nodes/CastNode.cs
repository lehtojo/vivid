public class CastNode : Node, IType, IResolvable
{
	public CastNode(Node target, Node type)
	{
		Add(target);
		Add(type);
	}

	public Type GetType()
	{
		if (Last is IType type)
		{
			return type.GetType();
		}

		return Types.UNKNOWN;
	}

	private void Resolve(Context context, Node node)
	{
		if (node is IResolvable resolvable)
		{
			var resolved = resolvable.Resolve(context);

			if (resolved != null)
			{
				node.Replace(resolved);
			}
		}
	}

	public Node Resolve(Context context)
	{
		Resolve(context, First);
		Resolve(context, Last);

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