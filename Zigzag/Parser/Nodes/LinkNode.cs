using System;

public class LinkNode : OperatorNode, Resolvable, Contextable
{
	public LinkNode(Node left, Node right) : base(Operators.DOT)
	{
		SetOperands(left, right);
	}

	private Type GetNodeContext(Node node)
	{
		if (node is Contextable contextable)
		{
			return contextable.GetContext();
		}

		return null;
	}

	public Node Resolve(Context @base)
	{
		if (Left is Resolvable a)
		{
			Node resolved = a.Resolve(@base);

			if (resolved != null)
			{
				Left.Replace(resolved);
				First = resolved;
			}
		}

		if (Right is Resolvable b)
		{
			Context context = GetNodeContext(Left);

			if (context == Types.UNKNOWN)
			{
				throw new Exception("Couldn't resolve the type of the left hand side");
			}

			Node resolved;

			if (Right.GetNodeType() == NodeType.UNRESOLVED_FUNCTION)
			{
				UnresolvedFunction function = (UnresolvedFunction)Right;
				resolved = function.Solve(@base, context);
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

	public override Type GetContext()
	{
		return GetNodeContext(Right);
	}

	public override NodeType GetNodeType()
	{
		return NodeType.LINK_NODE;
	}
}