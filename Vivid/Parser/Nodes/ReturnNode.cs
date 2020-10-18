using System;

public class ReturnNode : InstructionNode, IResolvable
{
	private Status CurrentStatus = Status.Error("Return type was not resolved");

	public Node? Value => First;

	public ReturnNode(Node? node) : base(Keywords.RETURN)
	{
		if (node != null)
		{
			Add(node);
		}
	}

	public Node? Resolve(Context context)
	{
		// Returned object must be resolved first
		var node = First;

		// Find the parent function where the return value can be assigned
		var function = context.GetFunctionParent() ?? throw new ApplicationException("Return statement was not inside a function");

		if (node == null)
		{
			CurrentStatus = Status.OK;
			function.ReturnType = Types.UNIT;
			return null;
		}

		Resolver.Resolve(context, node);

		if (node == null)
		{
			throw new ApplicationException("Return statement did not have a value to return");
		}

		var current = function.ReturnType;
		var type = node?.TryGetType();

		if (type == Types.UNKNOWN)
		{
			CurrentStatus = Status.Error("Could not resolve return type");
			return null;
		}

		// If the current return type is not registered, the type of the return value can be set as the current return type
		if (current == Types.UNKNOWN)
		{
			function.ReturnType = type;
			CurrentStatus = Status.OK;
			return null;
		}

		if (current.Equals(type))
		{
			CurrentStatus = Status.OK;
			return null;
		}

		// Try to find a supertype that both the current return type and the type of the return value inherit
		var shared = Resolver.GetSharedType(current, type);

		if (shared == null)
		{
			CurrentStatus = Status.Error($"Type '{type}' is not compatible with the current return type '{current?.ToString() ?? "none"}'");
			return null;
		}

		// Update the return type to match the shared supertype
		function.ReturnType = shared;
		CurrentStatus = Status.OK;
		return null;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.RETURN;
	}

	public Status GetStatus()
	{
		return CurrentStatus;
	}
}