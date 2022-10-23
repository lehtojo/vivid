using System;
using System.Collections.Generic;
using System.Linq;

public static class Resolver
{
	private const string YELLOW = "\x1B[1;33m";
	private const string RESET = "\x1B[0m";

	/// <summary>
	/// Tries to resolve the specified array type
	/// </summary>
	public static Type? ResolveArrayType(Context environment, ArrayType type)
	{
		type.Resolve(environment);
		if (type.IsResolved()) return type;
		return null;
	}

	/// <summary>
	/// Tries to resolve the specified type if it is unresolved
	/// </summary>
	public static Type? Resolve(Context context, Type type)
	{
		if (type.IsResolved()) return null;

		// Resolve array types, because their sizes need to be determined at compile time and they can be dependent on expressions
		if (type is ArrayType) return ResolveArrayType(context, (ArrayType)type);

		return type.To<UnresolvedType>().ResolveOrNull(context);
	}

	/// <summary>
	/// Tries to resolve the specified node tree
	/// </summary>
	public static void Resolve(Context context, Node node)
	{
		var result = ResolveTree(context, node);
		if (result == null) return;
		node.Replace(result);
	}

	/// <summary>
	/// Resolves the parameters of the specified function
	/// </summary>
	public static void Resolve(Function function)
	{
		// Resolve the parameters
		foreach (var parameter in function.Parameters)
		{
			var type = parameter.Type;
			if (type == null || type.IsResolved()) continue;

			type = Resolve(function, type);
			if (type == null) continue;

			parameter.Type = type;
		}
	}

	/// <summary>
	/// Tries to resolve supertypes which were not found previously
	/// </summary>
	public static void ResolveSupertypes(Context context, Type type)
	{
		for (var i = type.Supertypes.Count - 1; i >= 0; i--)
		{
			var supertype = type.Supertypes[i];
			if (!supertype.IsUnresolved) continue;

			// Try to resolve the supertype
			var resolved = Resolve(context, supertype);

			// Skip the supertype if it could not be resolved or if it is not allowed to be inherited
			if (resolved == null || !type.IsInheritingAllowed(resolved)) continue;

			// Replace the old unresolved supertype with the resolved one
			type.Supertypes[i] = resolved;
		}
	}

	/// <summary>
	/// Tries to resolve the specified function implementation
	/// </summary>
	public static void ResolveImplementation(FunctionImplementation implementation)
	{
		ResolveReturnType(implementation);
		ResolveVariables(implementation);

		if (implementation.Node == null) return;
		ResolveTree(implementation, implementation.Node);
	}

	/// <summary>
	/// Tries to resolve every problem in the specified context
	/// </summary>
	public static void ResolveContext(Context context)
	{
		var functions = Common.GetAllVisibleFunctions(context);
		foreach (var function in functions) { Resolve(function); }

		var types = Common.GetAllTypes(context);

		// Resolve all the types
		foreach (var type in types)
		{
			ResolveSupertypes(context, type);

			// Resolve all member variables
			foreach (var iterator in type.Variables)
			{
				Resolve(iterator.Value);
			}

			// Resolve all initializations
			foreach (var initialization in type.Initialization)
			{
				Resolve(type, initialization);
			}

			// Resolve array types, because their sizes need to be determined at compile time and they can be dependent on expressions
			if (type is ArrayType) ResolveArrayType(type.Parent!, type.To<ArrayType>());

			ResolveVirtualFunctions(type);
		}

		var implementations = Common.GetAllFunctionImplementations(context);

		// Resolve all implementation variables and node trees
		foreach (var implementation in implementations)
		{
			ResolveReturnType(implementation);
			ResolveVariables(implementation);

			if (implementation.Node == null) continue;
			ResolveTree(implementation, implementation.Node);
		}

		// Resolve constants
		ResolveVariables(context);
	}

	/// <summary>
	/// Tries to resolve problems in the node tree
	/// </summary>
	private static Node? ResolveTree(Context context, Node node)
	{
		// If the node is unresolved, try to resolve it
		if (node is IResolvable resolvable)
		{
			try
			{
				return resolvable.Resolve(context);
			}
			catch (Exception e)
			{
				Console.WriteLine($"{YELLOW}Internal{RESET}: {e.Message}");
			}

			return null;
		}

		foreach (var child in node)
		{
			Resolve(context, child);
		}

		return null;
	}

	/// <summary>
	/// Returns the number type that should be the outcome when using the two given numbers together
	/// </summary>
	private static Type GetSharedNumber(Number a, Number b)
	{
		var bits = Math.Max(a.Bits, b.Bits);
		var signed = !a.IsUnsigned || !b.IsUnsigned;
		var is_decimal = a.Format.IsDecimal() || b.Format.IsDecimal();
		return Primitives.CreateNumber(bits, signed, is_decimal);
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
		if (expected == null || actual == null) return null;
		if (Equals(expected, actual)) return expected;

		// Do not allow implicit conversions between links and non-links
		if ((expected is Link) ^ (actual is Link)) return null;

		if (expected is Number && actual is Number)
		{
			return GetSharedNumber(expected.To<Number>(), actual.To<Number>());
		}

		var expected_all_types = GetAllTypes(expected);
		var actual_all_types = GetAllTypes(actual);

		foreach (var supertype in expected_all_types)
		{
			if (actual_all_types.Contains(supertype)) return supertype;
		}

		return null;
	}

	/// <summary>
	/// Returns the shared type between the types
	/// </summary>
	/// <returns>Success: Shared type between the types, Failure: null</returns>
	public static Type? GetSharedType(IReadOnlyList<Type?> types)
	{
		if (types.Count == 0) return null;
		if (types.Count == 1) return types[0];

		var current = types[0];

		for (var i = 1; i < types.Count; i++)
		{
			if (current == null) break;
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
			if (type == null) return null;
			types.Add(type);

			iterator = iterator.Next;
		}

		return types;
	}

	/// <summary>
	/// Tries to resolve the given variable by going through its references
	/// </summary>	
	private static void Resolve(Variable variable)
	{
		if (variable.Type != null)
		{
			// If the variable is already resolved, there is no need to do anything
			if (variable.Type.IsResolved()) return;

			// Try to resolve the variable type
			var resolved = Resolve(variable.Parent, variable.Type);
			if (resolved == null) return;

			variable.Type = resolved;
			return;
		}

		var types = new List<Type>();

		foreach (var usage in variable.Usages)
		{
			var parent = usage.Parent;
			if (parent == null) continue;

			if (parent.Is(Operators.ASSIGN))
			{
				// The usage must be the destination
				if (parent.First != usage) continue;
			}
			else if (parent.Instance == NodeType.LINK)
			{
				// The usage must be the destination
				if (parent.Last != usage) continue;

				parent = parent.Parent;
				if (parent == null || !parent.Is(Operators.ASSIGN)) continue;
			}
			else
			{
				continue;
			}

			// Get the assignment type from the source operand
			var type = parent.Right.TryGetType();
			if (type == null) continue;

			types.Add(type);
		}

		// Get the shared type between the references
		var shared = GetSharedType(types);
		if (shared == null) return;

		variable.Type = shared;
	}

	/// <summary>
	/// Tries to resolve all the variables in the given context
	/// </summary>
	private static void ResolveVariables(Context context)
	{
		foreach (var variable in context.Variables.Values)
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
		// Do not resolve the return type if it is already resolved.
		// This also prevents virtual function overrides from overriding the return type, enforced by the virtual function declaration
		if (implementation.ReturnType != null)
		{
			// Try to resolve the return type
			var resolved = Resolve(implementation, implementation.ReturnType);
			if (resolved == null) return;

			// Update the return type, since we resolved it
			implementation.ReturnType = resolved;
			return;
		}

		var statements = implementation.Node!.FindAll(NodeType.RETURN).Cast<ReturnNode>();

		// If there are no return statements, the return type of the implementation must be unit
		if (!statements.Any())
		{
			implementation.ReturnType = Primitives.CreateUnit();
			return;
		}

		// If any of the return statements does not have a return value, the return type must be unit
		if (statements.Any(i => i.Value == null))
		{
			implementation.ReturnType = Primitives.CreateUnit();
			return;
		}

		// Collect all return statement value types
		var types = statements.Select(i => i.Value!.TryGetType()).ToList();
		if (types.Any(i => i == null || i.IsUnresolved)) return;

		var type = Resolver.GetSharedType(types!);
		if (type == null) return;

		implementation.ReturnType = type;
	}

	/// <summary>
	/// Resolves return types of the virtual functions declared in the specified type
	/// </summary>
	private static void ResolveVirtualFunctions(Type type)
	{
		var overloads = new List<VirtualFunction>();
		foreach (var iterator in type.Virtuals) { overloads.AddRange(iterator.Value.Overloads.Cast<VirtualFunction>()); }

		// Virtual functions do not have return types defined sometimes, the return types of those virtual functions are dependent on their default implementations
		foreach (var virtual_function in overloads)
		{
			if (virtual_function.ReturnType != null) continue;

			// Find all overrides with the same name as the virtual function
			var result = type.GetOverride(virtual_function.Name);
			if (result == null) continue;
			var virtual_function_overloads = result.Overloads;

			// Take out the expected parameter types
			var expected = new List<Type?>();
			foreach (var parameter in virtual_function.Parameters) { expected.Add(parameter.Type); }

			foreach (var overload in virtual_function_overloads)
			{
				// Ensure the actual parameter types match the expected types
				var actual = overload.Parameters.Select(i => i.Type).ToList();

				if (actual.Count != expected.Count) continue;

				var skip = false;

				for (var i = 0; i < expected.Count; i++)
				{
					if (Equals(expected[i], actual[i])) continue;
					skip = true;
					break;
				}

				if (skip || overload.Implementations.Count == 0) continue;

				// Now the current overload must be the default implementation for the virtual function
				virtual_function.ReturnType = overload.Implementations.First().ReturnType;
				break;
			}
		}
	}
}