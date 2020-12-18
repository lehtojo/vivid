using System.Collections.Generic;
using System.Linq;

public class HasNode : Node, IType, IResolvable
{
	public const string RUNTIME_HAS_VALUE_FUNCTION_IDENTIFIER = "has_value";
	public const string RUNTIME_GET_VALUE_FUNCTION_IDENTIFIER = "get_value";

	public const string RUNTIME_HAS_VALUE_FUNCTION_HEADER = "has_value(): bool";
	public const string RUNTIME_GET_VALUE_FUNCTION_HEADER = "get_value(): any";

	public Node Source => First!;
	public VariableNode Result => Last!.To<VariableNode>();

	public HasNode(Node source, VariableNode result, Position position)
	{
		Position = position;
		
		Add(source);
		Add(result);
	}

	public Node? Resolve(Context environment)
	{
		Resolver.Resolve(environment, Source);

		var type = Source.TryGetType();

		if (type == Types.UNKNOWN || type.IsUnresolved)
		{
			return null;
		}

		var has_value_function = type.GetFunction(RUNTIME_HAS_VALUE_FUNCTION_IDENTIFIER)?.GetImplementation(new List<Type>());

		if (has_value_function == null || has_value_function.ReturnType != Types.BOOL)
		{
			return null;
		}

		var get_value_function = type.GetFunction(RUNTIME_GET_VALUE_FUNCTION_IDENTIFIER)?.GetImplementation(new List<Type>());

		if (get_value_function == null)
		{
			return null;
		}

		var inline_context = new Context(environment);

		var source_variable = inline_context.DeclareHidden(type);
		var result_variable = inline_context.DeclareHidden(Types.BOOL);

		// Declare the result variable at the start of the function
		var declaration = new DeclareNode(Result.Variable);
		var root = FindParent(i => i.Parent == null) ?? throw Errors.Get(Position!, "Could not find the root node");

		root.First().Insert(declaration);

		// Set the result variable equal to false
		var initialization = new OperatorNode(Operators.ASSIGN).SetOperands(
			new VariableNode(result_variable),
			new NumberNode(Parser.Format, 0L)
		);

		// Load the source into a variable
		var load = new OperatorNode(Operators.ASSIGN).SetOperands(
			new VariableNode(source_variable),
			Source
		);

		// First the function 'has_value(): bool' must return true in order to call the function 'get_value(): any'
		var condition = new LinkNode(new VariableNode(source_variable), new FunctionNode(has_value_function));

		// If the function 'has_value(): bool' returns true, load the value using the function 'get_value(): any' and set the result variable equal to true
		var body = new Node {
			new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(Result.Variable),
				new LinkNode(new VariableNode(source_variable), new FunctionNode(get_value_function))
			),
			new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(result_variable),
				new NumberNode(Parser.Format, 1L)
			),
		};

		var assignment_context = new Context(environment);
		var assignment = new IfNode(assignment_context, condition, body);

		return new ContextInlineNode(inline_context, Position) {
			initialization,
			load,
			assignment,
			new VariableNode(result_variable)
		};
	}

	public new Type GetType()
	{
		return Types.BOOL;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.HAS;
	}

	public Status GetStatus()
	{
		var type = Source.TryGetType();

		if (type == Types.UNKNOWN || type.IsUnresolved)
		{
			return Status.Error(Source.Position, "Could not resolve the type of the inspected object");
		}

		return Status.Error(Position, $"Ensure the inspected object has the following functions '{RUNTIME_HAS_VALUE_FUNCTION_HEADER}' and '{RUNTIME_GET_VALUE_FUNCTION_HEADER}'");
	}
}