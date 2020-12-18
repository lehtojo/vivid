using System;

public class ReturnNode : InstructionNode, IResolvable
{
	public Node? Value => First;

	public ReturnNode(Node? node) : base(Keywords.RETURN, null)
	{
		if (node != null)
		{
			Add(node);
		}
	}

	public ReturnNode(Node? node, Position? position) : base(Keywords.RETURN, position)
	{
		if (node != null)
		{
			Add(node);
		}
	}

	public Node? Resolve(Context context)
	{
		// Find the parent function where the return value can be assigned
		var function = context.GetFunctionParent() ?? throw new ApplicationException("Return statement was not inside a function");

		if (Value == null)
		{
			return null;
		}

		Resolver.Resolve(context, Value);

		//var current = function.ReturnType;
		var type = Value?.TryGetType();

		if (type == Types.UNKNOWN)
		{
			return null;
		}

		return null;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.RETURN;
	}

	public Status GetStatus()
	{
		if (Value == null)
		{
			return Status.OK;
		}

		// Find the parent function where the return value can be assigned
		var function = GetParentContext()!.GetFunctionParent();

		if (function == null)
		{
			return Status.Error(Position, "Return statement was not inside a function");
		}

		var expected = function.ReturnType;
		var actual = Value.TryGetType();

		if (actual == null)
		{
			return Status.Error(Position, "Could not resolve the return value");
		}

		if (Resolver.GetSharedType(expected, actual) == null)
		{
			return Status.Error(Position, $"Type '{actual}' is not compatible with the current return type '{expected?.ToString() ?? "none"}'");
		}

		return Status.OK;
	}
}