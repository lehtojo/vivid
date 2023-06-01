public class ReturnNode : Node, IResolvable
{
	public Node? Value => First;

	public ReturnNode(Node? node, Position? position)
	{
		Instance = NodeType.RETURN;
		Position = position;

		// Add the return value, if it exists
		if (node != null) Add(node);
	}

	public Node? Resolve(Context context)
	{
		if (First != null) Resolver.Resolve(context, First);

		// Process implicit conversions
		ImplicitConvertor.Process(context, this);
		return null;
	}

	public Status GetStatus()
	{
		// Find the environment context
		var context = FindContext();
		if (context == null) return Status.OK;

		// Look for the function we are inside of
		var implementation = context.GetContext().FindImplementationParent();
		if (implementation == null) return Status.OK;

		// If this statement has a return value, try to get its type
		var return_value_type = (Type?)null;
		if (First != null) { return_value_type = First.TryGetType(); }

		// Illegal return statements:
		// - Return statement does not have a return value even though the function has a return type
		// - Return statement does have a return value, but the function does not return a value
		// Unit type represents no return type. Exceptionally allow returning units when the return type is unit.
		var has_return_type = !Primitives.IsPrimitive(implementation.ReturnType, Primitives.UNIT);
		var has_return_value = First != null && !Primitives.IsPrimitive(return_value_type, Primitives.UNIT);

		if (has_return_type == has_return_value)
		{
			if (has_return_type && !Common.Compatible(return_value_type, implementation.ReturnType)) return new Status(Position, "Type of the returned value is not compatible with the return type");

			return Status.OK;
		}

		if (has_return_type) return new Status(Position, "Can not return without a value, because the function has a return type");
		return new Status(Position, "Can not return with a value, because the function does not return a value");
	}

	public override string ToString() => "Return";
}