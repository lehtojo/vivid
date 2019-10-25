using System;

public class ReturnNode : InstructionNode, Resolvable
{
	public ReturnNode(Node @object) : base(Keywords.RETURN)
	{
		Add(@object);
	}

	private Type GetReturnType(Node node)
	{
		if (node is Contextable contextable)
		{
			return contextable.GetContext();
		}

		throw new Exception("Couldn't resolve return type");
	}

	public Node Resolve(Context context)
	{
		// Returned object must be resolved first
		Node node = First;

		if (node is Resolvable resolvable)
		{
			Node resolved = resolvable.Resolve(context);

			if (resolved != null)
			{
				node.Replace(resolved);
				node = resolved;
			}
		}

		// Find the parent function where the return value can be assigned
		Function function = context.GetFunctionParent();

		Type current = function.ReturnType;
		Type type = GetReturnType(node);

		if (type == null)
		{
			throw new Exception("Couldn't resolve return type");
		}

		if (current != type)
		{
			Type shared = type;

			if (current != null)
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
}