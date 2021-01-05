using System.Linq;

public class LinkNode : OperatorNode, IResolvable, IType
{
	public Node Object => First!;
	public Node Member => Last!;

	public LinkNode(Node left, Node right) : base(Operators.DOT)
	{
		SetOperands(left, right);
	}

	public LinkNode(Node left, Node right, Position? position) : base(Operators.DOT, position)
	{
		SetOperands(left, right);
	}

	public override Node? Resolve(Context environment)
	{
		// Try to resolve the left node
		Resolver.Resolve(environment, Left);

		// The type of the left node is required
		var primary = Left.TryGetType();

		// Do not try to resolve the right node without the type of the left
		if (primary == null)
		{
			return null;
		}

		if (Right.Is(NodeType.UNRESOLVED_FUNCTION))
		{
			var function = Right.To<UnresolvedFunction>();

			// First, try to resolve the function normally
			var resolved = function.Solve(environment, primary);

			if (resolved != null)
			{
				Right.Replace(resolved);
				return null;
			}

			var types = function.Select(i => i.TryGetType()).ToList();

			// Try to form a virtual function call
			resolved = Common.TryGetVirtualFunctionCall(Left, primary, function.Name, function, types);

			if (resolved != null)
			{
				resolved.Position = Position;
				return resolved;
			}

			// Try to form a lambda function call
			resolved = Common.TryGetLambdaCall(primary, Left, function.Name, function, types);

			if (resolved != null)
			{
				resolved.Position = Position;
				return resolved;
			}
		}
		else
		{
			Resolver.Resolve(primary, Right);
		}

		return null;
	}

	public override Type? GetType()
	{
		return Right.TryGetType();
	}

	public override NodeType GetNodeType()
	{
		return NodeType.LINK;
	}

	public override Status GetStatus()
	{
		return Status.OK;
	}
}