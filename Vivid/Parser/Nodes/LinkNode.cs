using System.Linq;

public class LinkNode : OperatorNode
{
	public LinkNode(Node left, Node right) : base(Operators.DOT)
	{
		SetOperands(left, right);
		Instance = NodeType.LINK;
	}

	public LinkNode(Node left, Node right, Position? position) : base(Operators.DOT, position)
	{
		SetOperands(left, right);
		Instance = NodeType.LINK;
	}

	public override Node? Resolve(Context environment)
	{
		// Try to resolve the left node
		Resolver.Resolve(environment, Left);

		// The type of the left node is required
		var primary = Left.TryGetType();

		// Do not try to resolve the right node without the type of the left
		if (primary == null) return null;

		if (Right.Is(NodeType.UNRESOLVED_FUNCTION))
		{
			var function = Right.To<UnresolvedFunction>();

			// First, try to resolve the function normally
			var resolved = function.Resolve(environment, primary);

			if (resolved != null)
			{
				Right.Replace(resolved);
				return null;
			}

			var types = function.Select(i => i.TryGetType()).ToList();

			// Try to form a virtual function call
			resolved = Common.TryGetVirtualFunctionCall(Left, primary, function.Name, function, types, Position);
			if (resolved != null) return resolved;

			// Try to form a lambda function call
			resolved = Common.TryGetLambdaCall(primary, Left, function.Name, function, types);

			if (resolved != null)
			{
				resolved.Position = Position;
				return resolved;
			}
		}
		else if (Right.Is(NodeType.UNRESOLVED_IDENTIFIER))
		{
			Resolver.Resolve(primary, Right);
		}
		else
		{
			/// NOTE: Environment context is required
			/// Consider a situation where the right operand is a function call.
			/// The function arguments need the environment context to be resolved.
			Resolver.Resolve(environment, Right);
		}
		
		return null;
	}

	public override Type? TryGetType()
	{
		return Right.TryGetType();
	}

	/// <summary>
	/// Returns whether the accessed object is accessible based in the specified environment
	/// </summary>
	private bool IsAccessible(FunctionImplementation environment, bool reads)
	{
		// Only variables and function calls can be checked
		if (!Right.Is(NodeType.VARIABLE, NodeType.FUNCTION)) return true;

		var context = Right.Is(NodeType.VARIABLE) ? Right.To<VariableNode>().Variable.Parent : Right.To<FunctionNode>().Function.Parent;
		if (context == null || !context.IsType) return true;

		// Determine which type owns the accessed object
		var owner = (Type)context;

		// Determine the required access level for accessing the object
		var modifiers = Right.Is(NodeType.VARIABLE) ? Right.To<VariableNode>().Variable.Modifiers : Right.To<FunctionNode>().Function.Metadata.Modifiers;
		var required_access_level = modifiers & Modifier.ACCESS_LEVEL_MASK;

		// Determine the access level of the requester
		var requester = environment.FindTypeParent();
		var request_access_level = requester == owner ? Modifier.PRIVATE : (requester != null && requester.IsTypeInherited(owner) ? Modifier.PROTECTED : Modifier.PUBLIC);

		// 1. Objects can always be read when the access level of the requester is higher or equal to the required level.
		// 2. If writing is not restricted, the access level of the requester must be higher or equal to the required level.
		if (reads || !Flag.Has(modifiers, Modifier.READABLE)) return request_access_level >= required_access_level;

		// Writing is restricted, so the requester must have higher access level
		return request_access_level > required_access_level;
	}

	/// <summary>
	/// Returns whether this link represents a static access that is not allowed.
	/// Unallowed static access can be accessing of a non-static member through type.
	/// </summary>
	private bool IsIllegalStaticAccess(FunctionImplementation environment)
	{
		// Require the left operand to be a type node
		if (Left.Instance != NodeType.TYPE) return false;

		var accessed_type = Left.To<TypeNode>().Type;

		var is_inside_static_function = environment.IsStatic;
		var is_inside_accessed_type = environment.Parent!.IsType && (ReferenceEquals(environment.Parent!.To<Type>(), accessed_type) || environment.Parent!.To<Type>().IsTypeInherited(accessed_type));

		var is_accessed_object_static = 
			(Right.Instance == NodeType.VARIABLE && (Right.To<VariableNode>().Variable.IsStatic || Right.To<VariableNode>().Variable.IsConstant)) ||
			(Right.Instance == NodeType.FUNCTION && Right.To<FunctionNode>().Function.IsStatic) ||
			(Right.Is(NodeType.TYPE, NodeType.CONSTRUCTION));

		// If a non-static member variable or function is accessed in static way, return true.
		// Only exception is if we are "inside" the accessed type and not inside a static function.
		// So in other words, non-static members can be accessed through types in the accessed type or if it is inherited.
		return !is_accessed_object_static && !(is_inside_accessed_type && !is_inside_static_function);
	}

	public override Status GetStatus()
	{
		var environment = GetParentContext()?.FindImplementationParent();
		if (environment == null) return Status.OK;

		var reads = !Analyzer.IsEdited(this);

		if (!IsAccessible(environment, reads)) return Status.Error(Right.Position, "Can not access the member here");
		if (IsIllegalStaticAccess(environment)) return Status.Error(Right.Position, "Can not access non-shared member this way");

		return Status.OK;
	}

	public override string ToString() => "Link";
}