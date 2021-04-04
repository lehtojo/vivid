using System.Collections.Generic;
using System.Linq;
using System;

public static class GarbageCollector
{
	/// <summary>
	/// Finds all the subcontexts which are not present in the specified context list
	/// </summary>
	private static List<Context> FindSubcontextsExcept(Context context, Context[] except)
	{
		var contexts = context.Subcontexts.Except(except).ToList();
		var temporary = new List<Context>();

		foreach (var iterator in contexts)
		{
			temporary.AddRange(FindSubcontextsExcept(iterator, except));
		}

		contexts.AddRange(temporary);

		return contexts;
	}

	/// <summary>
	/// Returns all the variables under the specified scope which can be destructed.
	/// The returned variables can not be declared under the specified scope contexts.
	/// </summary>
	private static List<Variable> GetUnlinkableScopeVariables(Context scope, Context[] scopes)
	{
		var contexts = FindSubcontextsExcept(scope, scopes);

		contexts.Add(scope);

		// Collect all the variables which have a destructor
		return contexts.SelectMany(i => i.Variables.Values).Where(i => i.IsLocal && i.Type!.Destructors.Overloads.Any()).ToList();
	}

	/// <summary>
	/// Generates unlink code for all the specified variables and outputs it to the specified destination node
	/// </summary>
	private static void UnlinkAll(Node destination, List<Variable> variables)
	{
		foreach (var variable in variables)
		{
			var implementation = Parser.UnlinkFunction!.Get(variable.Type!) ?? throw new ApplicationException("Missing unlink function overload");
			destination.Add(new FunctionNode(implementation).SetParameters(new Node { new VariableNode(variable) }));
		}
	}

	/// <summary>
	/// Generates all the unlinker code under the specified root node
	/// </summary>
	private static void CreateAllScopeUnlinkers(FunctionImplementation implementation)
	{
		// Skip the following functions since they are internal functions
		if (implementation.Metadata == Parser.LinkFunction || implementation.Metadata == Parser.UnlinkFunction)
		{
			return;
		}

		var root = implementation.Node!;

		// Find all the scopes
		var scopes = root.FindAll(i => i.Is(NodeType.CONTEXT, NodeType.IMPLEMENTATION)).Cast<IContext>().ToList();
		var contexts = scopes.Select(i => i.GetContext()).ToArray();

		// Add the root since it is a scoped node
		scopes.Add((IContext)root);

		var returns = root.FindAll(i => i.Is(NodeType.RETURN)).Cast<ReturnNode>().ToArray();

		foreach (var statement in returns)
		{
			var scope = (IContext)(statement.FindParent(i => i.Is(NodeType.CONTEXT, NodeType.IMPLEMENTATION)) ?? throw new ApplicationException("Missing statement context"));
			var variables = GetUnlinkableScopeVariables(scope.GetContext(), contexts);

			scopes.Remove(scope);

			if (statement.Value != null)
			{
				var source = Analyzer.GetSource(statement.Value)!;
				
				var is_local_variable = source.Is(NodeType.VARIABLE) && source.To<VariableNode>().Variable.IsLocal;
				var is_function_call = source.Is(NodeType.FUNCTION, NodeType.CALL);

				if (!is_local_variable)
				{
					var value = statement.Value;
					var type = value.GetType();

					if (!variables.Any())
					{
						// Link the value if it can be linked
						if (!is_function_call && type.Destructors.Overloads.Any())
						{
							// Remove the value from the return statement
							value.Remove();

							implementation = Parser.LinkFunction!.Get(type) ?? throw new ApplicationException("Missing link function overload");
							implementation.ReturnType = type;

							statement.Add(new FunctionNode(implementation).SetParameters(new Node { value }));
						}

						continue;
					}

					// Remove the value from the return statement
					value.Remove();

					var environment = new ContextInlineNode(new Context(scope.GetContext()));

					// Load the return value into a temporary variable
					var variable = environment.Context.DeclareHidden(type);

					// Link the value if it can be linked
					if (!is_function_call && type.Destructors.Overloads.Any())
					{
						implementation = Parser.LinkFunction!.Get(type) ?? throw new ApplicationException("Missing link function overload");
						implementation.ReturnType = type;

						environment.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
							new VariableNode(variable),
							new FunctionNode(implementation).SetParameters(new Node { value })
						));
					}
					else
					{
						environment.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
							new VariableNode(variable),
							value
						));
					}
					
					// Unlink all the variables
					UnlinkAll(environment, variables);

					// Set the temporary variable as the return value
					statement.Add(new VariableNode(variable));

					// Replace the return statement with the inline node and add the return statement to the end of the inline node
					statement.Replace(environment);
					environment.Add(statement);

					continue;
				}
				else
				{
					// Do not unlink the variable since it is returned
					variables.Remove(source.To<VariableNode>().Variable);
				}
			}

			var inline = new InlineNode();

			// Unlink all the variables
			UnlinkAll(inline, variables);

			// Replace the return statement with the inline node and add the return statement to the end of the inline node
			statement.Replace(inline);
			inline.Add(statement);
		}

		foreach (var scope in scopes)
		{
			var variables = GetUnlinkableScopeVariables(scope.GetContext(), contexts);

			if (!variables.Any())
			{
				continue;
			}

			// Unlink all the variables
			UnlinkAll((Node)scope, variables);
		}
	}

	/// <summary>
	/// Generates all the linker code under the specified root node.
	/// NOTE: The following can not be done (linking must be done before unlinking)
	/// a = Object()
	/// a = a or f(a) where function f returns back the specified argument
	/// =>
	/// unlink(a) <- Here the reference count would reach zero before linking
	/// a = link(f())
	///
	/// NOTE: Here are examples of correctly generated code
	/// Example:
	/// a = f() <- Here it is required that this is not a declaration
	/// =>
	/// x = a
	/// a = f()
	/// unlink(x)
	///
	/// Example:
	/// a.b.c = f()
	/// =>
	/// x = a.b
	/// y = x.c
	/// x.c = f()
	/// unlink(y)
	///
	/// Example:
	/// a.b[i.j] = f()
	/// =>
	/// x = a.b
	/// y = i.j
	/// z = x[y]
	/// x[y] = f()
	/// unlink(z)
	/// 
	/// Example:
	/// a = b.c
	/// =>
	/// x = a
	/// a = link(b.c)
	/// unlink(x)
	/// </summary>
	private static void CreateAllScopeLinkers(FunctionImplementation implementation)
	{
		// Skip the following functions since they are internal functions
		if (implementation.Metadata == Parser.LinkFunction || implementation.Metadata == Parser.UnlinkFunction)
		{
			return;
		}

		var root = implementation.Node!;

		var descriptors = GeneralAnalysis.GetVariableDescriptors(implementation, root);
		var assigns = root.FindAll(i => i.Is(Operators.ASSIGN));

		foreach (var assign in assigns)
		{
			var destination = assign.Left;
			var source = Analyzer.GetSource(assign.Right)!;

			/// TODO: No need to link the destination when the assignment declares a local variable
			var destination_type = destination.GetType();
			var source_type = source.GetType();

			// 1. Require the source to be linkable
			// 2. Do not link the source value if it is a function call
			if (source_type.Destructors.Overloads.Any() && !source.Is(NodeType.FUNCTION, NodeType.CALL))
			{
				implementation = Parser.LinkFunction!.Get(source_type) ?? throw new ApplicationException("Missing link function overload");
				implementation.ReturnType = source_type;

				source.Replace(new FunctionNode(implementation).SetParameters(new Node { source.Clone() }));
			}

			var primitive = destination.Is(NodeType.VARIABLE);

			if (primitive)
			{
				var variable = destination.To<VariableNode>().Variable;

				if (variable.IsPredictable)
				{
					var descriptor = descriptors[variable];

					// If the current assignment is the first, it means the current assignment declares the variable
					if (descriptor.Writes.First().Node == assign)
					{
						continue;
					}
				}
			}

			// If the destination can not be unlinked, skip it
			if (!destination_type.Destructors.Overloads.Any())
			{
				continue;
			}

			var inline = new InlineNode();
			var environment = assign.GetParentContext();

			// Complex destinations need inlining
			if (!primitive)
			{
				// Inline the destination because it is complex
				var steps = ReconstructionAnalysis.InlineDestination(environment, destination);

				steps.ForEach(i => inline.Add(i));

				// Reload the destination since the inline function call above has modified it
				destination = assign.Left;
			}

			// Save the value of the destination before assigning
			var temporary = environment.DeclareHidden(destination_type);

			inline.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(temporary),
				destination.Clone()
			));

			assign.Replace(inline);

			// Execute the assignment after saving the old value of the destination
			inline.Add(assign);

			// Unlink the old value
			implementation = Parser.UnlinkFunction!.Get(destination_type) ?? throw new ApplicationException("Missing link function overload");
			inline.Add(new FunctionNode(implementation).SetParameters(new Node { new VariableNode(temporary) }));
		}
	}
	
	/// <summary>
	/// Store the results of function calls, for example, into temporary variables so that they will be destructed.
	/// Before:
	/// ignore(create_object()) <- Here the created object will never be destructed since the outer function ignores it
	/// After:
	/// ignore({a = create_object(), a}) <- Here the created object is saved to a local variable and passed as a parameter. The local variable is destructed later.
	/// </summary>
	private static void CreateIntermediateResults(Node root)
	{
		// Find all function calls
		var calls = root.FindAll(i => i.Is(NodeType.FUNCTION, NodeType.CALL));
		
		foreach (var call in calls)
		{
			var node = call;

			if (node.Parent != null && node.Parent.Is(NodeType.LINK) && node.Parent.Right == node)
			{
				node = node.Parent;
			}

			var type = call.GetType();

			// If the return type of the function call is not destructable, then this function call can be skipped
			if (!type.Destructors.Overloads.Any())
			{
				continue;
			}

			var parent = node.FindParent(i => !i.Is(NodeType.CAST)) ?? throw new ApplicationException("Reference did not have a valid parent");

			// If the return value of the function call is copied to a destination, then this function call can be skipped
			if (parent.Is(Operators.ASSIGN) && (parent.Right == call || call.IsUnder(parent.Right)))
			{
				continue;
			}

			// Create a temporary variable which will store the result of the function call
			var variable = node.GetParentContext().DeclareHidden(type);
			var inline = new InlineNode(call.Position);

			node.Replace(inline);
			
			// Store the return value to the temporary variable
			inline.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(variable),
				node
			));

			// Set the return value of the inline node to be the temporary variable
			inline.Add(new VariableNode(variable));
		}
	}

	/// <summary>
	/// Generates garbage collecting for the specified function implementation
	/// </summary>
	public static void Generate(FunctionImplementation implementation)
	{
		CreateIntermediateResults(implementation.Node!);
		CreateAllScopeUnlinkers(implementation);
		CreateAllScopeLinkers(implementation);
	}
}