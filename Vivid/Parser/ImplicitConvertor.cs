public static class ImplicitConvertor
{
	public const string IMPLICIT_CONVERTOR_FUNCTION = "from";

	private static bool HasConvertorFunction(Type from, Type to)
	{
		// Member function is a convertor function when
		// - its name is <IMPLICIT_CONVERTOR_FUNCTION>
		// - accepts exactly one argument of source type
		// - it is shared
		// - returns destination type

		// Attempt to get all convertor functions
		if (!to.Functions.ContainsKey(IMPLICIT_CONVERTOR_FUNCTION)) return false;
		var convertors = to.Functions[IMPLICIT_CONVERTOR_FUNCTION];

		// Attempt to find an overload that accepts the source type as its only argument
		var overload = convertors.GetOverload(from);
		if (overload == null) return false;

		// Ensure the overload is shared
		if (!overload.IsStatic) return false;

		// Ensure the overload has an explicit return type that matches the destination type
		return overload.ReturnType == to;
	}

	private static void TryConversion(Node node, Type from, Type to)
	{
		// If the source type is not compatible with the destination type, we can attempt an implicit conversion
		if (Common.Compatible(from, to)) return;

		// Attempt to find a convertor function
		if (!HasConvertorFunction(from, to)) return;

		// Call the shared convertor function with the specified value as its argument
		var call = new UnresolvedFunction(IMPLICIT_CONVERTOR_FUNCTION, node.Position);
		var conversion = new LinkNode(new TypeNode(to, node.Position), call);

		// Replace the specified value with the conversion
		node.Replace(conversion);

		// Pass the value to the call
		call.Add(node);
	}

	public static void Process(Context context, ReturnNode node)
	{
		if (node.First == null) return;

		// Attempt to get the type of the returned value
		var returned_type = node.First.TryGetType();
		if ((returned_type == null) || returned_type.IsUnresolved) return;

		// Find the function we are inside of
		var implementation = context.FindImplementationParent();
		if (implementation == null) return;

		// Get the return type of the function
		var return_type = implementation.ReturnType;
		if (return_type == null || return_type.IsUnresolved) return;

		TryConversion(node.First, returned_type, return_type);
	}

	private static void ProcessAssignmentOperator(Context context, OperatorNode node)
	{
		// Attempt to get the type of the right-hand side
		var right_type = node.Right.TryGetType();
		if (right_type == null || right_type.IsUnresolved) return;

		// Attempt to get the type of the left-hand side
		var left_type = node.Left.TryGetType();
		if (left_type == null || left_type.IsUnresolved) return;

		TryConversion(node.Right, right_type, left_type);
	}

	public static void Process(Context context, OperatorNode node)
	{
		var operation = node.Operator;

		if (operation == Operators.ASSIGN) ProcessAssignmentOperator(context, node);
	}
}