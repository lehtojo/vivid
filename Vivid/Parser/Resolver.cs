using System;
using System.Collections.Generic;
using System.Linq;

public static class Resolver
{
	private const string YELLOW = "\x1B[1;33m";
	private const string RESET = "\x1B[0m";

	/// <summary>
	/// Tries to resolve the specified type if it is unresolved
	/// </summary>
	public static Type? Resolve(Context context, Type type)
	{
		return type is UnresolvedType unresolved ? unresolved.TryResolveType(context) : null;
	}

	/// <summary>
	/// Tries to resolve the given node tree
	/// </summary>
	public static void Resolve(Context context, Node node)
	{
		var resolved = ResolveTree(context, node);

		if (resolved != null)
		{
			node.Replace(resolved);
		}
	}

	/// <summary>
	/// Tries to resolve supertypes which were not found previously
	/// </summary>
	private static void ResolveSupertypes(Context context, Type type)
	{
		for (var i = type.Supertypes.Count - 1; i >= 0; i--)
		{
			var supertype = type.Supertypes[i];

			// Skip supertypes which are already resolved
			if (!supertype.IsUnresolved)
			{
				continue;
			}

			var resolved = Resolve(context, supertype);

			// Skip the supertype if it could not be resolved or if it is not allowed to be inherited
			if (resolved == null || !type.IsInheritingAllowed(resolved))
			{
				continue;
			}

			// Replace the old unresolved supertype with the resolved one
			type.Supertypes.RemoveAt(i);
			type.Supertypes.Insert(i, resolved);
		}
	}

	/// <summary>
	/// Tries to resolve the problems in the given context
	/// </summary>
	public static void ResolveContext(Context context)
	{
		context.Update(true);

		ResolveVariables(context);

		var types = new List<Type>(context.Types.Values);
		var overloads = (List<Function>?)null;

		foreach (var type in types)
		{
			ResolveSupertypes(context, type);
			ResolveVariables(type);
			ResolveContext(type);

			foreach (var iterator in type.Initialization)
			{
				Resolve(type, iterator);
			}

			// Virtual functions do not have return types defined sometimes, the return types of those virtual functions are dependent on their default implementations
			foreach (var virtual_function in type.Virtuals.Values.SelectMany(i => i.Overloads).Cast<VirtualFunction>())
			{
				if (virtual_function.ReturnType != null) continue;
				
				// Find all overrides with the same name as the virtual function
				overloads = type.GetOverride(virtual_function.Name)?.Overloads;
				if (overloads == null) continue;

				// Take out the expected parameter types
				var expected = virtual_function.Parameters.Select(i => i.Type).ToList();

				foreach (var overload in overloads)
				{
					// Ensure the actual parameter types match the expected types
					var actual = overload.Parameters.Select(i => i.Type).ToList();
					if (actual.Count != expected.Count || !actual.SequenceEqual(expected)) continue;

					if (!overload.Implementations.Any()) continue;

					// Now the current overload must be the default implementation for the virtual function
					virtual_function.ReturnType = overload.Implementations.First().ReturnType;
					break;
				}
			}
		}

		overloads = Common.GetAllVisibleFunctions(context).ToList();

		// Resolve parameter types
		foreach (var function in overloads)
		{
			foreach (var parameter in function.Parameters)
			{
				if (parameter.Type == null || !parameter.Type.IsUnresolved) continue;
				
				var type = parameter.Type.To<UnresolvedType>().TryResolveType(context);

				if (!Equals(type, null))
				{
					parameter.Type = type;
				}
			}

			// Resolve virtual function return types
			if (function is not VirtualFunction virtual_function || virtual_function.ReturnType == null || !virtual_function.ReturnType.IsUnresolved) continue;

			// Update the return type only if it is resolved
			var resolved = Resolver.Resolve(context, virtual_function.ReturnType);
			if (resolved == null) continue;

			virtual_function.ReturnType = resolved;
		}

		foreach (var implementation in Common.GetAllFunctionImplementations(context))
		{
			ResolveVariables(implementation);

			// Check if the implementation has a return type and if it is unresolved
			if (implementation.ReturnType != null)
			{
				if (implementation.ReturnType.IsUnresolved)
				{
					var type = implementation.ReturnType!.To<UnresolvedType>().TryResolveType(implementation);

					if (type != null)
					{
						implementation.ReturnType = type;
					}
				}
			}
			else
			{
				ResolveReturnType(implementation);
			}

			if (implementation.Node != null)
			{
				if (!implementation.Metadata!.IsImported && implementation.Node.Find(NodeType.RETURN) == null)
				{
					implementation.ReturnType = Primitives.CreateUnit();
				}

				ResolveTree(implementation, implementation.Node!);
			}

			// Resolve short functions
			ResolveContext(implementation);
		}
	}

	/// <summary>
	/// Tries to resolve the problems in the node tree
	/// </summary>
	private static Node? ResolveTree(Context context, Node node)
	{
		if (node is IResolvable resolvable)
		{
			try
			{
				return resolvable.Resolve(context) ?? node;
			}
			catch (Exception e)
			{
				if (node.Position != null)
				{
					Console.WriteLine($"{YELLOW}Internal warning{RESET}: {Errors.FormatPosition(node.Position)} {e.Message}");
				}
				else
				{
					Console.WriteLine($"{YELLOW}Internal warning{RESET}: {e.Message}");
				}
			}

			return null;
		}

		var iterator = node.First;

		while (iterator != null)
		{
			var resolved = ResolveTree(context, iterator);

			if (resolved != null)
			{
				iterator.Replace(resolved);
			}

			iterator = iterator.Next;
		}

		return node;
	}

	/// <summary>
	/// Returns the number type that should be the outcome when using the two given numbers together
	/// </summary>
	private static Type GetSharedNumber(Number a, Number b)
	{
		return a.Bits > b.Bits ? a : b;
	}

	/// <summary>
	/// Return all supertypes of the specified type and output them to the specified supertype list.
	/// The output supertypes are in priority order.
	/// </summary>
	public static List<Type> GetAllTypes(Type type)
	{
		var batch = new List<Type>(type.Supertypes);
		var supertypes = new List<Type>(type.Supertypes);

		while (batch.Any())
		{
			var copy = new List<Type>(batch);

			batch.Clear();
			batch.AddRange(copy.SelectMany(i => i.Supertypes));

			supertypes.AddRange(batch);
		}

		supertypes.Insert(0, type);

		return supertypes;
	}

	/// <summary>
	/// Returns the shared type between the types
	/// </summary>
	/// <returns>Success: Shared type between the types, Failure: null</returns>
	public static Type? GetSharedType(Type? expected, Type? actual)
	{
		if (Equals(expected, actual)) return expected;
		if (expected == null || actual == null) return null;

		if (expected is Number x && actual is Number y)
		{
			if (expected.Format.IsDecimal()) return expected;
			if (actual.Format.IsDecimal()) return actual;

			return GetSharedNumber(x, y);
		}

		var expected_supertypes = GetAllTypes(expected);
		var actual_supertypes = GetAllTypes(actual);

		foreach (var supertype in expected_supertypes)
		{
			if (actual_supertypes.Contains(supertype))
			{
				return supertype;
			}
		}

		return null;
	}

	/// <summary>
	/// Returns the shared type between the types
	/// </summary>
	/// <returns>Success: Shared type between the types, Failure: null</returns>
	public static Type? GetSharedType(IReadOnlyList<Type> types)
	{
		if (types.Count == 0) return null;
		if (types.Count == 1) return types[0];

		var current = types[0];

		for (var i = 1; i < types.Count; i++)
		{
			if (current == null)
			{
				break;
			}

			current = GetSharedType(current, types[i]);
		}

		return current;
	}

	/// <summary>
	/// Returns the types of the child nodes of the given node
	/// </summary>
	/// <returns>Success: Types of the child nodes, Failure: null</returns>
	public static List<Type>? GetTypes(Node node)
	{
		var types = new List<Type>();
		var iterator = node.First;

		while (iterator != null)
		{
			var type = iterator.TryGetType();

			if (type == null || type.IsUnresolved)
			{
				// This operation must be aborted since type list cannot contain unresolved types
				return null;
			}

			types.Add(type);

			iterator = iterator.Next;
		}

		return types;
	}

	/// <summary>
	/// Tries to get the assign type from the given assign operation
	///</summary>
	private static Type? TryGetTypeFromAssignOperation(Node assign)
	{
		var operation = assign.To<OperatorNode>();

		// Try to resolve the type using the right operand if the operation node represents an assignment
		if (operation.Operator == Operators.ASSIGN)
		{
			return operation.Right.TryGetType();
		}

		return null;
	}

	/// <summary>
	/// Tries to resolve the given variable by going through its references
	/// </summary>	
	private static void Resolve(Variable variable)
	{
		var types = new List<Type>();

		// Try resolving the type of the variable from its references
		foreach (var reference in variable.References)
		{
			var parent = reference.Parent;
			if (parent == null) continue;

			if (parent.Instance == NodeType.OPERATOR) // Locals
			{
				// Reference must be the destination in assign operation in order to resolve the type
				if (parent.First != reference) continue;

				var type = TryGetTypeFromAssignOperation(parent);

				if (type != null)
				{
					types.Add(type);
				}
			}
			else if (parent.Instance == NodeType.LINK) // Members
			{
				// Reference must be the destination in assign operation in order to resolve the type
				if (parent.Last != reference) continue;

				parent = parent.Parent;

				if (parent == null || !parent.Is(NodeType.OPERATOR)) continue;

				var type = TryGetTypeFromAssignOperation(parent!);

				if (type != null)
				{
					types.Add(type);
				}
			}
		}

		// Get the shared type between the references
		var shared = GetSharedType(types);

		if (shared != null)
		{
			// Now the type is resolved
			variable.Type = shared;
		}
	}

	/// <summary>
	/// Tries to resolve all the variables in the given context
	/// </summary>
	private static void ResolveVariables(Context context)
	{
		foreach (var variable in context.Variables.Values.Where(i => i.Type == null))
		{
			Resolve(variable);
		}

		foreach (var subcontext in context.Subcontexts)
		{
			ResolveVariables(subcontext);
		}
	}

	/// <summary>
	/// Tries to resolve the return type of the specified implementation based on its return statements
	/// </summary>
	private static void ResolveReturnType(FunctionImplementation implementation)
	{
		if (implementation.Node == null) return;

		var statements = implementation.Node.FindAll(NodeType.RETURN).Cast<ReturnNode>();

		if (statements.Any(i => i.Value == null))
		{
			implementation.ReturnType = Primitives.CreateUnit();
			return;
		}

		var types = statements.Select(i => i.Value!.TryGetType()).ToList();

		if (types.Any(i => i == null || i.IsUnresolved))
		{
			return;
		}

		var type = Resolver.GetSharedType(types!);

		if (type == null)
		{
			return;
		}

		implementation.ReturnType = type;
	}
}