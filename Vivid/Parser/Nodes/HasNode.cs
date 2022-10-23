public class HasNode : Node, IResolvable
{
	public const string RUNTIME_HAS_VALUE_FUNCTION_HEADER = "has_value(): bool";
	public const string RUNTIME_GET_VALUE_FUNCTION_HEADER = "get_value(): any";

	public Node Source => First!;
	public VariableNode Output => Last!.To<VariableNode>();

	public HasNode(Node source, VariableNode output, Position position)
	{
		Position = position;
		Instance = NodeType.HAS;

		Add(source);
		Add(output);
	}

	public Node? Resolve(Context environment)
	{
		Resolver.Resolve(environment, Source);

		// Continue if the type of the source object can be extracted
		var type = Source.TryGetType();
		if (type == null || type.IsUnresolved) return null;

		// Continue if the source object has the required getter function
		var get_value_function = type.GetFunction(ReconstructionAnalysis.RUNTIME_GET_VALUE_FUNCTION_IDENTIFIER)?.GetImplementation();
		if (get_value_function == null || get_value_function.ReturnType == null || get_value_function.ReturnType.IsUnresolved) return null;

		// Set the type of the output variable to the return type of the getter function
		Output.Variable.Type = get_value_function.ReturnType;
		return null;
	}

	public override Type TryGetType()
	{
		return Primitives.CreateBool();
	}

	public Status GetStatus()
	{
		var type = Source.TryGetType();
		if (type == null || type.IsUnresolved) return Status.Error(Source.Position, "Can not resolve the type of the inspected object");

		var has_value_function_overloads = type.GetFunction(ReconstructionAnalysis.RUNTIME_HAS_VALUE_FUNCTION_IDENTIFIER);
		if (has_value_function_overloads == null) return Status.Error(Source.Position, "Inspected object does not have a \'has_value(): bool\' function");

		var has_value_function = has_value_function_overloads.GetImplementation();
		if (has_value_function == null || !Primitives.IsPrimitive(has_value_function.ReturnType, Primitives.BOOL)) return Status.Error(Source.Position, "Inspected object does not have a \'has_value(): bool\' function");

		var get_value_function_overloads = type.GetFunction(ReconstructionAnalysis.RUNTIME_GET_VALUE_FUNCTION_IDENTIFIER);
		if (get_value_function_overloads == null) return Status.Error(Source.Position, "Inspected object does not have a \'get_value(): any\' function");

		var get_value_function = get_value_function_overloads.GetImplementation();
		if (get_value_function == null || get_value_function.ReturnType == null || get_value_function.ReturnType.IsUnresolved) return Status.Error(Source.Position, "Inspected object does not have a \'get_value(): any\' function");

		return Status.OK;
	}
}