using System;
using System.Collections.Generic;
using System.Linq;

public static class Resolver
{
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

		var expected_all_types = expected.GetAllSupertypes();
		var actual_all_types = actual.GetAllSupertypes();

		expected_all_types.Insert(0, expected);
		actual_all_types.Insert(0, actual);

		foreach (var type in expected_all_types)
		{
			if (actual_all_types.Contains(type)) return type;
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
		var shared_type = types[0];

		for (var i = 1; i < types.Count; i++)
		{
			shared_type = GetSharedType(shared_type, types[i]);
			if (shared_type == null) return null;
		}

		return shared_type;
	}

	/// <summary>
	/// Returns the types of the child nodes, only if all have types
	/// </summary>
	public static List<Type>? GetTypes(Node node)
	{
		var result = new List<Type>();

		foreach (var iterator in node)
		{
			var type = iterator.TryGetType();
			if (type == null) return null;
			result.Add(type);
		}

		return result;
	}

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

		// Resolve the return type
		var return_type = function.ReturnType;

		if (return_type != null && return_type.IsUnresolved)
		{
			return_type = Resolve(function, return_type);
			if (return_type != null) { function.ReturnType = return_type; }
		}
	}

	/// <summary>
	/// Resolves imports in the specified context
	/// </summary>
	public static void ResolveImports(Context context)
	{
		// Resolve imports
		for (var i = 0; i < context.Imports.Count; i++)
		{
			// Skip resolved imports
			var imported = context.Imports[i];
			if (imported.IsResolved()) continue;

			// Try to resolve the import
			var resolved = Resolve(context, imported);
			if (resolved == null) continue;

			context.Imports[i] = resolved;
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

		// Resolve imports in the current context
		ResolveImports(context);

		var types = Common.GetAllTypes(context);

		// Resolve all the types
		foreach (var type in types)
		{
			ResolveImports(type);
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
			ResolveImplementation(implementation);
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
				Console.WriteLine(Errors.Format(null, e.Message));
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
		var shared_type = GetSharedType(types);
		if (shared_type == null) return;

		variable.Type = shared_type;
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

		foreach (var subcontext in context.Subcontexts.ToArray())
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

			var override_function_overloads = result.Overloads;

			// Take out the expected parameter types
			var expected = new List<Type?>();
			foreach (var parameter in virtual_function.Parameters) { expected.Add(parameter.Type); }

			foreach (var overload in override_function_overloads)
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

	private static List<Status> GetTreeStatuses(Node root)
	{
		var result = new List<Status>();

		foreach (var child in root)
		{
			result.AddRange(GetTreeStatuses(child));

			var status = child is IResolvable resolvable ? resolvable.GetStatus() : Status.OK;
			if (!status.IsProblematic) continue;

			result.Add(status);
		}

		return result;
	}

	private static List<Status> GetTreeReport(Node root)
	{
		var errors = new List<Status>();
		if (root == null) return errors;

		foreach (var status in GetTreeStatuses(root))
		{
			if (status == null || !status.IsProblematic) continue;
			errors.Add(status);
		}

		return errors;
	}

	// Summary: Reports unresolved imports
	private static void GetImportReport(Context context, List<Status> errors)
	{
		// Report unresolved imports
		foreach (var imported in context.Imports)
		{
			if (imported.IsResolved()) continue;
			errors.Add(new Status(imported.Position, "Can not resolve the import"));
		}
	}

	/// <summary>
	/// Returns whether the specified type contains a member of the target type that causes an illegal cycle.
	/// </summary>
	private static bool FindIllegalCyclicMember(Type type, Type target, HashSet<Type> trace)
	{
		// Remember that the current type has been visited, so that we do not get stuck in a loop
		trace.Add(type);

		foreach (var iterator in type.Variables)
		{
			var member_type = iterator.Value.Type;

			// If the member is the target type, return true
			if (ReferenceEquals(member_type, target)) return true;

			// If the member is not a pack or inlining type, skip it
			if (member_type == null || !(member_type.IsPack || member_type.IsInlining)) continue;

			// If the member has already been visited, skip it
			if (trace.Contains(member_type)) continue;

			// If the member is a pack or inlining type, check it recursively
			if (FindIllegalCyclicMember(member_type, target, trace)) return true;
		}

		return false;
	}

	/// <summary>
	/// Reports the specified type if it is illegally cyclic
	/// </summary>
	private static void ReportIllegalCyclicType(Type type, List<Status> errors)
	{
		// Only packs and inlining types might have illegal cycles
		if (!(type.IsPack || type.IsInlining)) return;

		// Attempt to find a member of the type that causes an illegal cycle
		if (!FindIllegalCyclicMember(type, type, new HashSet<Type>())) return;

		errors.Add(new Status(type.Position, "Illegal cyclic type"));
	}

	private static List<Status> GetTypeReport(Type type)
	{
		var errors = new List<Status>();

		// Report unresolved imports
		GetImportReport(type, errors);

		if (type.Parent != null && !(type.Parent.IsGlobal || type.Parent.IsNamespace))
		{
			errors.Add(new Status(type.Position, "Types must be created in global scope or namespace"));
		}

		ReportIllegalCyclicType(type, errors);

		foreach (var variable in type.Variables.Values)
		{
			if (variable.IsResolved) continue;
			errors.Add(new Status(variable.Position, "Can not resolve the type of the member variable"));
		}

		foreach (var initialization in type.Initialization)
		{
			errors.AddRange(GetTreeReport(initialization));
		}

		foreach (var supertype in type.Supertypes)
		{
			if (supertype.IsResolved()) continue;
			errors.Add(new Status(type.Position, "Can not inherit the supertype"));
		}

		return errors;
	}

	private static List<Status> GetFunctionReport(Function function)
	{
		var errors = new List<Status>();
		if (function.IsTemplateFunction) return errors;

		foreach (var parameter in function.Parameters)
		{
			// Explicit parameter types are optional, but they must be resolved if specified
			if (parameter.Type == null || parameter.Type.IsResolved()) continue;
			errors.Add(new Status(parameter.Position, "Can not resolve the type of the parameter " + parameter.Name));
		}

		// Explicit return types are optional, but they must be resolved if specified
		if (function.ReturnType != null)
		{
			if (function.ReturnType.IsUnresolved)
			{
				errors.Add(new Status(function.Start, "Can not resolve the return type"));
			}
			else if (function.ReturnType is ArrayType)
			{
				errors.Add(new Status(function.Start, "Array type is not allowed as a return type"));
			}
		}

		return errors;
	}

	private static List<Status> GetFunctionReport(FunctionImplementation implementation)
	{
		var errors = new List<Status>();

		foreach (var variable in implementation.Locals)
		{
			if (variable.IsResolved) continue;
			errors.Add(new Status(variable.Position, "Can not resolve the type of the variable " + variable.Name));
		}

		if (implementation.ReturnType == null || implementation.ReturnType.IsUnresolved)
		{
			errors.Add(new Status(implementation.Metadata.Start, "Can not resolve the return type"));
		}
		else if (implementation.ReturnType is ArrayType)
		{
			errors.Add(new Status(implementation.Metadata.Start, "Array type is not allowed as a return type"));
		}

		errors.AddRange(GetTreeReport(implementation.Node!));
		return errors;
	}

	public static List<Status> GetReport(Context context, Node root)
	{
		var errors = new List<Status>();

		// Report unresolved imports
		GetImportReport(context, errors);

		// Report errors in defined types
		var types = Common.GetAllTypes(context);

		foreach (var type in types)
		{
			errors.AddRange(GetTypeReport(type));
		}

		// Report errors in function headers
		var functions = Common.GetAllVisibleFunctions(context);

		foreach (var function in functions)
		{
			errors.AddRange(GetFunctionReport(function));
		}

		// Report errors in defined functions
		var implementations = Common.GetAllFunctionImplementations(context);

		foreach (var implementation in implementations)
		{
			errors.AddRange(GetFunctionReport(implementation));
		}

		errors.AddRange(GetTreeReport(root));
		return errors;
	}

	/// <summary>
	/// Finds all static member assignments which should be executed before the entry function
	/// </summary>
	private static List<Node> CollectStaticInitializers(Context context)
	{
		var types = Common.GetAllTypes(context);
		var initializers = types.SelectMany(i => i.Initialization).Select(i => i.Clone()).ToList();

		for (var i = initializers.Count - 1; i >= 0; i--)
		{
			// Look for static member assignments
			var initializer = initializers[i];
			var edited = Analyzer.GetEdited(initializer);

			// Ensure the edited node is a variable node
			if (edited.Instance == NodeType.VARIABLE)
			{
				var member = edited.To<VariableNode>().Variable;

				// Ensure the member variable is static
				if (member.IsStatic)
				{
					edited.Replace(new LinkNode(
						new TypeNode(member.Parent.To<Type>(), member.Position),
						edited.Clone(),
						member.Position
					));

					continue;
				}
			}

			// Remove the initializer, since it is not a static member assignment
			initializers.RemoveAt(i);
		}

		return initializers;
	}

	/// <summary>
	/// Finds the implementations of the allocation and the inheritance functions and registers them to be used
	/// </summary>
	public static void RegisterDefaultFunctions(Context context)
	{
		var allocation_function = context.GetFunction("allocate") ?? throw new ApplicationException("Missing the allocation function, please implement it or include the standard library");
		var deallocation_function = context.GetFunction("deallocate") ?? throw new ApplicationException("Missing the deallocation function, please implement it or include the standard library");
		var inheritance_function = context.GetFunction("internal_is") ?? throw new ApplicationException("Missing the inheritance function, please implement it or include the standard library");
		var initialization_function = context.GetFunction("internal_init");

		Settings.AllocationFunction = allocation_function.GetImplementation(Primitives.CreateNumber(Primitives.LARGE, Format.INT64)) ?? throw new ApplicationException("Missing the allocation function, please implement it or include the standard library");
		Settings.DeallocationFunction = deallocation_function.GetImplementation(new Link()) ?? throw new ApplicationException("Missing the deallocation function, please implement it or include the standard library");
		Settings.InheritanceFunction = inheritance_function.GetImplementation(new Link(), new Link()) ?? throw new ApplicationException("Missing the inheritance function, please implement it or include the standard library");

		// Find all the static member assignments and add them to the application initialization function
		var static_initializers = CollectStaticInitializers(context);

		if (initialization_function != null)
		{
			Settings.InitializationFunction = initialization_function.GetImplementation(new Link()) ?? initialization_function.GetImplementation();
		}
		else if (static_initializers.Any())
		{
			// Application initialization function calls the entry function: init()
			var initialization_function_blueprint = new List<Token>()
			{
				new FunctionToken(new IdentifierToken(Keywords.INIT.Identifier), new ParenthesisToken())
			};

			// Create an application initialization function, which calls the entry function, so that the static member assignments can be executed
			var initialization_function_metadata = new Function(context, Modifier.EXPORTED, "internal_init", initialization_function_blueprint, Settings.AllocationFunction!.Metadata.Start, null);
			context.Declare(initialization_function_metadata);
	
			Settings.InitializationFunction = initialization_function_metadata.Get(Array.Empty<Type>());
		}

		if (static_initializers.Any())
		{
			if (Settings.InitializationFunction == null) throw new ApplicationException("Missing the application initialization function");

			// Add the static initializers to the application initialization function
			for (var i = static_initializers.Count - 1; i >= 0; i--)
			{
				var initializer = static_initializers[i];
				var initializer_destination = Settings.InitializationFunction!.Node!.First;
				Settings.InitializationFunction.Node!.Insert(initializer_destination, initializer);
			}
		}
	}
}