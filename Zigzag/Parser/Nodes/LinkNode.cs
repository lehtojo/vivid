public class LinkNode : OperatorNode, IResolvable, IType
{
	public LinkNode(Node left, Node right) : base(Operators.DOT)
	{
		SetOperands(left, right);
	}

	private Type? GetNodeContext(Node node)
	{
		if (node is IType type)
		{
			return type.GetType();
		}

		return null;
	}

	public Node? Resolve(Context environment)
	{
		if (Left is IResolvable a)
		{
			var resolved = a.Resolve(environment);

			if (resolved != null)
			{
				Left.Replace(resolved);
				First = resolved;
			}
		}

		if (Right is IResolvable b)
		{
			var context = GetNodeContext(Left);

			if (context == Types.UNKNOWN)
			{
				return null;
			}

			Node? resolved;

			if (Right.GetNodeType() == NodeType.UNRESOLVED_FUNCTION)
			{
				var function = (UnresolvedFunction)Right;
				resolved = function.Solve(environment, context);
			}
			else
			{
				resolved = b.Resolve(context);
			}

			if (resolved != null)
			{
				Right.Replace(resolved);
				Last = resolved;
			}
		}

		return null;
	}

	public override Type? GetType()
	{
		return GetNodeContext(Right);
	}

	public override NodeType GetNodeType()
	{
		return NodeType.LINK_NODE;
	}

	public Status GetStatus()
	{
		return Status.OK;
	}
}