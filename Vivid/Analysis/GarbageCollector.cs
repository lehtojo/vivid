using System.Collections.Generic;
using System.Linq;
using System;

public class ScopeDestructionDescriptor
{
	public List<StackAddressNode> Allocations { get; } = new();
	public bool IsTerminated { get; set; } = false;
}

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
	/// Returns all the local variables under the specified scope which can be destructed.
	/// The returned variables can not be declared under the specified scope contexts.
	/// </summary>
	private static List<Variable> GetUnlinkableLocalVariables(Context scope, Context[] scopes)
	{
		var contexts = FindSubcontextsExcept(scope, scopes);

		contexts.Add(scope);

		// Collect all the variables which have a destructor
		return contexts.SelectMany(i => i.Variables.Values).Where(i => i.IsLocal && i.Type!.IsUserDefined).ToList();
	}

	/// <summary>
	/// Returns all the variables which have been initialized when the specified node is executed.
	/// </summary>
	private static List<Variable> GetUnlinkableVariables(Node perspective, Context scope)
	{
		// Collect all the local variables which have a destructor
		var locals = scope.Variables.Values.Where(i => i.IsLocal && i.Type!.IsUserDefined);
		
		// 1. If the scope represents a function implementation, collecting should be stopped here
		// 2. Require the local variables to be initialized before the specified perspective
		if (scope.IsImplementation) return locals.Where(i => i.Writes.Any() && i.Writes.First().IsBefore(perspective)).ToList();

		// Require the local variables to be initialized before the specified perspective
		return locals.Where(i => i.Writes.Any() && i.Writes.First().IsBefore(perspective)).Concat(GetUnlinkableVariables(perspective, scope.Parent!)).ToList();
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
	/// Generates code which destructs all the specified stack allocations
	/// </summary>
	private static void DestructAll(Node destination, IEnumerable<StackAddressNode> allocations)
	{
		foreach (var allocation in allocations)
		{
			var implementation = allocation.Type.Destructors.GetImplementation() ?? throw new ApplicationException("Missing destructor");
			
			// Do not call the destructor if it is empty
			if (implementation.IsEmpty) continue;
			
			destination.Add(new LinkNode(allocation.Clone(), new FunctionNode(implementation)));
		}
	}

	/// <summary>
	/// Registers stack allocations so that they will be destroyed properly
	/// </summary>
	public static void DestructStackAllocations(Node root, Dictionary<Node, ScopeDestructionDescriptor> scopes)
	{
		// Find all the stack allocations inside the specified node
		var allocations = root.FindAll(i => i.Is(NodeType.STACK_ADDRESS)).Distinct();

		foreach (var allocation in allocations)
		{
			var scope = allocation.FindParent(i => ReconstructionAnalysis.IsStatement(i)) ?? throw new ApplicationException("Stack allocation did not have a parent scope");
			var denylist = Analysis.GetDenylist(allocation);

			// Find all the scopes under the parent scope of the allocation and require that they are executed after the allocation
			var subscopes = scopes.Keys.Where(i => i.IsUnder(scope) && i.IsAfter(allocation));

			// Add the stack allocation to the filtered scopes
			scopes[scope].Allocations.Add(allocation.To<StackAddressNode>());

			// Destroy the allocation in subscopes which are terminated by a return statement
			subscopes.Where(i => scopes[i].IsTerminated).ForEach(i => scopes[i].Allocations.Add(allocation.To<StackAddressNode>()));
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
		var scopes = root.FindAll(i => ReconstructionAnalysis.IsScope(i)).ToDictionary(i => i, _ => new ScopeDestructionDescriptor());
		var contexts = scopes.Keys.Select(i => ((IScope)i).GetContext()).ToArray();

		// Add the root since it is a scoped node
		scopes.Add(root, new ScopeDestructionDescriptor());

		var returns = root.FindAll(i => i.Is(NodeType.RETURN)).Cast<ReturnNode>().ToArray();

		// Register scopes which can not be exited
		foreach (var statement in returns)
		{
			var scope = statement.FindParent(i => ReconstructionAnalysis.IsScope(i)) ?? throw new ApplicationException("Return statement did not have a parent scope");
			scopes[scope].IsTerminated = true;
		}

		// Find all the stack allocations and register them to be destructed
		DestructStackAllocations(root, scopes);

		foreach (var statement in returns)
		{
			var scope = statement.FindParent(i => ReconstructionAnalysis.IsScope(i)) ?? throw new ApplicationException("Return statement did not have a parent scope");
			var context = ((IScope)scope).GetContext();
			
			var variables = GetUnlinkableVariables(statement, context);
			var allocations = scopes[scope].Allocations;

			// If the return statement has a value, it might need to be moved, because the destructors must be executed last
			if (statement.Value != null)
			{
				// Get the actual value which will be returned
				var source = Analyzer.GetSource(statement.Value)!;
				
				// Determine which kind of value the return value is
				var is_local_variable = source.Is(NodeType.VARIABLE) && source.To<VariableNode>().Variable.IsLocal;
				var is_function_call = source.Is(NodeType.FUNCTION, NodeType.CALL);

				if (!is_local_variable)
				{
					var value = statement.Value;
					var type = value.GetType();

					// Do not add any special operations if there are no variables to be destructed
					if (!allocations.Any() && !variables.Any())
					{
						// 1. Function calls can not be linked since their return values are already linked
						// 2. The return value needs to be linkable
						if (!is_function_call && type.IsUserDefined)
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

					var environment = new ContextInlineNode(new Context(context));

					// Load the return value into a temporary variable
					var variable = environment.Context.DeclareHidden(type);

					// 1. Function calls can not be linked since their return values are already linked
					// 2. The return value needs to be linkable
					if (!is_function_call && type.IsUserDefined)
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

					// Destruct all the stack allocations
					DestructAll(environment, scopes[scope].Allocations);

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

			// Destruct all the stack allocations
			DestructAll(inline, scopes[scope].Allocations);

			// Replace the return statement with the inline node and add the return statement to the end of the inline node
			statement.Replace(inline);
			inline.Add(statement);
		}

		foreach (var scope in scopes)
		{
			// Skip scopes which are terminated by a return statement
			if (scope.Value.IsTerminated) continue;

			var context = ((IScope)scope.Key).GetContext();
			var variables = GetUnlinkableLocalVariables(context, contexts);

			if (!variables.Any()) continue;

			// Unlink all the variables
			UnlinkAll(scope.Key, variables);

			// Destruct all the stack allocations
			DestructAll(scope.Key, scope.Value.Allocations);
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
			if (source_type.IsUserDefined && !source.Is(NodeType.FUNCTION, NodeType.CALL))
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
			if (!destination_type.IsUserDefined)
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

			// If the return type of the function call is not destructible, then this function call can be skipped
			if (!type.IsUserDefined)
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

	/// <summary>
	/// Generates the function which are used for keeping track of used objects
	/// </summary>
	public static void CreateReferenceCountingFunctions(Context root)
	{
		if (!Analysis.IsGarbageCollectorEnabled) return;
		
		var instance_parameter_name = "a";

		var link = new Function(root, Modifier.DEFAULT, "link", new Position(), new Position());
		link.Start!.File = Parser.AllocationFunction!.Metadata.Start!.File;
		link.Parameters.Add(new Parameter(instance_parameter_name));

		root.Declare(link);

		// Increments the reference count of the passed instance
		// Result:
		// if a != 0 { a..references += 1 }
		// => a
		link.Blueprint.AddRange(new List<Token>
		{
			new KeywordToken(Keywords.IF),
			new IdentifierToken(instance_parameter_name),
			new OperatorToken(Operators.NOT_EQUALS),
			new NumberToken(0),

			new ContentToken
			(
				ParenthesisType.CURLY_BRACKETS,
				new IdentifierToken(instance_parameter_name),
				new OperatorToken(Operators.DOT),
				new IdentifierToken(RuntimeConfiguration.REFERENCE_COUNT_VARIABLE),
				new OperatorToken(Operators.ASSIGN_ADD),
				new NumberToken(1)
			),

			new OperatorToken(Operators.HEAVY_ARROW),
			new IdentifierToken(instance_parameter_name)
		});

		Lexer.RegisterFile(link.Blueprint, link.Start!.File!);

		var unlink = new Function(root, Modifier.DEFAULT, "unlink", new Position(), new Position());
		unlink.Start!.File = Parser.AllocationFunction!.Metadata.Start!.File;
		unlink.Parameters.Add(new Parameter(instance_parameter_name));

		root.Declare(unlink);

		// Result:
		// if a != 0 and exchange_add(a..references, -1) == 1 {
		//  a.deinit()
		//  deallocate(a as link)
		// }
		unlink.Blueprint.AddRange(new List<Token>
		{
			new KeywordToken(Keywords.IF),

			new IdentifierToken(instance_parameter_name),
			new OperatorToken(Operators.NOT_EQUALS),
			new NumberToken(0),

			new OperatorToken(Operators.AND),

			new IdentifierToken(instance_parameter_name),
			new OperatorToken(Operators.DOT),
			new IdentifierToken(RuntimeConfiguration.REFERENCE_COUNT_VARIABLE),
			new OperatorToken(Operators.ATOMIC_EXCHANGE_ADD),
			new NumberToken(-1),
			new OperatorToken(Operators.EQUALS),
			new NumberToken(1),

			new ContentToken
			(
				ParenthesisType.CURLY_BRACKETS,

				new IdentifierToken(instance_parameter_name),
				new OperatorToken(Operators.DOT),
				new FunctionToken
				(
					new IdentifierToken(Keywords.DEINIT.Identifier), 
					new ContentToken()
				),

				new Token(TokenType.END),

				new FunctionToken
				(
					new IdentifierToken(Parser.DeallocationFunction!.Name), 
					new ContentToken
					(
						new IdentifierToken(instance_parameter_name),
						new KeywordToken(Keywords.AS),
						new IdentifierToken(Primitives.LINK)
					)
				)
			)
		});

		Lexer.RegisterFile(unlink.Blueprint, unlink.Start!.File!);

		Parser.LinkFunction = link;
		Parser.UnlinkFunction = unlink;
	}

	/// <summary>
	/// Creates all link and unlink function overloads based on the types in the specified context
	/// </summary>
	public static void CreateAllOverloads(Context root)
	{
		if (!Analysis.IsGarbageCollectorEnabled) return;

		foreach (var type in Common.GetAllTypes(root))
		{
			if (type.IsStatic || !type.IsUserDefined)
			{
				continue;
			}

			Parser.LinkFunction!.Get(type);
			Parser.UnlinkFunction!.Get(type);
		}
	}
}