using System;

public class ReturnNode : InstructionNode, IResolvable
{
	public ReturnNode(Node node) : base(Keywords.RETURN)
	{
		Add(node);
	}

	private Type GetReturnType(Node node)
	{
		if (node is IType type)
		{
			return type.GetType();
		}

		throw new Exception("Couldn't resolve return type");
	}

	public Node Resolve(Context context)
	{
		// Returned object must be resolved first
		var node = First;

		if (node is IResolvable resolvable)
		{
			var resolved = resolvable.Resolve(context);

			if (resolved != null)
			{
				node.Replace(resolved);
				node = resolved;
			}
		}

		// Find the parent function where the return value can be assigned
		var function = context.GetFunctionParent();

		var current = function.ReturnType;
		var type = GetReturnType(node);

		if (type == Types.UNKNOWN)
		{
			throw new Exception("Couldn't resolve return type");
		}

		if (current != type)
		{
			var shared = type;

			if (current != Types.UNKNOWN)
			{
				shared = Resolver.GetSharedType(current, type);
			}

			if (shared == null)
			{
				throw new Exception($"Type '{type.Name}' isn't compatible with the current return type '{current.Name}'");
			}

			function.ReturnType = type;
		}

		return null;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.RETURN_NODE;
	}

	public Status GetStatus()
	{
		return Status.OK;
	}
}