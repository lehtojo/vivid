using System;

public class ReturnNode : InstructionNode, IResolvable
{
	private Status CurrentStatus = Status.OK;

	public Node Value => First!;

	public ReturnNode(Node node) : base(Keywords.RETURN)
	{
		Add(node);
	}

	private Type? GetReturnType(Node node)
	{
		if (node is IType type)
		{
			return type.GetType();
		}

		return Types.UNKNOWN;
	}

	public Node? Resolve(Context context)
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
		var function = context.GetFunctionParent() ?? throw new ApplicationException("Return statement was not inside a function");

		var current = function.ReturnType;
		var type = GetReturnType(node ?? throw new ApplicationException("Return statment didn't have a value to return"));

		if (type != Types.UNKNOWN && current != type)
		{
			var shared = (Type?)type;

			if (current != Types.UNKNOWN)
			{
				shared = Resolver.GetSharedType(current, type);
			}

			if (shared == null)
			{
				CurrentStatus = Status.Error($"Type '{type.Name}' isn't compatible with the current return type '{current?.Name ?? "none"}'");
				return null;
			}

			function.ReturnType = type;
			CurrentStatus = Status.OK;
		}

		return null;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.RETURN_NODE;
	}

	public Status GetStatus()
	{
		return CurrentStatus;
	}
}