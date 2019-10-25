public class CastNode : Node, Contextable, Resolvable
{
	public CastNode(Node target, Node type)
	{
		Add(target);
		Add(type);
	}

	public Type GetContext()
	{
		if (Last is Contextable contextable)
		{
			return contextable.GetContext();
		}

		return Types.UNKNOWN;
	}

	private void Resolve(Context context, Node node)
	{
		if (node is Resolvable resolvable)
		{
			Node resolved = resolvable.Resolve(context);
			node.Replace(resolved);
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
}