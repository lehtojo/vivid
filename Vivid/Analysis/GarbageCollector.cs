using System.Collections.Generic;
using System.Linq;
using System;

public class ScopeDestructionDescriptor
{
	public List<StackAddressNode> Allocations { get; } = new();
	public bool IsTerminated { get; set; } = false;
}

public struct VariableAssignmentDescriptor
{
	public Variable Variable { get; }

	/// <summary>
	/// Stores the positions of assignments that assign defined values
	/// </summary>
	public int[] Definitions { get; }

	/// <summary>
	/// Stores the positions of assignments that assign undefined values
	/// </summary>
	public int[] Undefinitions { get; }

	public VariableAssignmentDescriptor(Variable variable, int[] definitions, int[] undefinitions)
	{
		Variable = variable;
		Definitions = definitions;
		Undefinitions = undefinitions;
	}
}

public static class GarbageCollector
{
	/// <summary>
	/// Generates a node tree for linking the specified object
	/// </summary>
	public static Node LinkObject(Context environment, Node value)
	{
		var position = value.Position;

		// Condition: value != 0
		var condition = new OperatorNode(Operators.NOT_EQUALS).SetOperands(value.Clone(), new NumberNode(Parser.Format, 0L));

		// Body:
		// link(value)
		var body = new Node { new ObjectLinkNode(value.Clone()) }; // Corresponds to an atomic operation, which increases the reference count

		// Result:
		// if value != 0 {
		//   link(value)
		// }
		return new IfNode(new Context(environment), condition, body, position, null);
	}

	/// <summary>
	/// Generates a node tree for unlinking the specified object
	/// </summary>
	public static Node UnlinkObject(Context environment, Node value)
	{
		var position = value.Position;

		// Condition: value != 0 and unlink(value) == 0
		var condition = new OperatorNode(Operators.AND, position).SetOperands(
			new OperatorNode(Operators.NOT_EQUALS).SetOperands(value.Clone(), new NumberNode(Parser.Format, 0L)),
			new OperatorNode(Operators.EQUALS).SetOperands(new ObjectUnlinkNode(value.Clone()), new NumberNode(Parser.Format, 0L)) // Corresponds to an atomic operation, which decreases the reference count and tests whether the reference count is zero
		);

		var type = value.GetType();
		var destructor = type.Destructors.GetImplementation() ?? throw new ApplicationException("Missing destructor for type " + type.Name);

		// Body:
		// value.deinit()
		// deallocate(value as link)
		var body = new Node()
		{
			new LinkNode(value.Clone(), new FunctionNode(destructor), position),
			new FunctionNode(Settings.DeallocationFunction!, position).SetArguments(new Node {
				new CastNode(value.Clone(), new TypeNode(new Link())),
			})
		};

		// Result:
		// if value != 0 and unlink(value) == 0 {
		//   value.deinit()
		//   deallocate(value as link)
		// }
		return new IfNode(new Context(environment), condition, body, position, null);
	}

	/// <summary>
	/// Returns whether the specified type is linkable
	/// </summary>
	public static bool IsTypeLinkable(Type type)
	{
		return type.IsUserDefined && !type.IsPack && !type.IsPlain;
	}

	/// <summary>
	/// Returns whether the specified node calls the standard allocator function
	/// </summary>
	private static bool IsAllocationCall(Node node)
	{
		return node.Instance == NodeType.FUNCTION && node.To<FunctionNode>().Function == Settings.AllocationFunction;
	}

	/// <summary>
	/// Finds all the subcontexts which are not present in the specified context list
	/// </summary>
	private static List<Context> FindSubcontextsExcept(Context context, Context[] except)
	{
		var contexts = context.Subcontexts.Where(i => !i.IsImplementation && !i.IsFunction).Except(except).ToList();
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
	private static List<Variable> GetUnlinkableLocalVariables(StatementFlow flow, Dictionary<Variable, VariableAssignmentDescriptor> initializations, Context scope, Context[] scopes, Node perspective)
	{
		var contexts = FindSubcontextsExcept(scope, scopes);
		contexts.Add(scope);

		// Collect all the variables which have a destructor
		var locals = contexts.SelectMany(i => i.Variables.Values).Where(i => i.IsPredictable && IsTypeLinkable(i.Type!)).ToList();

		// Return all the variables that are initialized before the specified perspective
		return locals.Where(i => i.Writes.Any() && IsInitializedBefore(flow, flow.IndexOf(perspective), initializations[i])).ToList();
	}

	/// <summary>
	/// Returns all the variables which have been initialized when the specified node is executed.
	/// </summary>
	private static List<Variable> GetUnlinkableVariables(StatementFlow flow, Dictionary<Variable, VariableAssignmentDescriptor> initializations, Context scope, Node perspective, Context? until = null)
	{
		// Collect all the local variables which have a destructor
		var locals = scope.Variables.Values.Where(i => i.IsPredictable && IsTypeLinkable(i.Type!));
		
		// 1. If the scope represents a function implementation, collecting should be stopped here
		// 2. Require the local variables to be initialized before the specified perspective
		if (scope.IsImplementation || ReferenceEquals(scope, until)) return locals.Where(i => i.Writes.Any() && IsInitializedBefore(flow, flow.IndexOf(perspective), initializations[i])).ToList();

		// Require the local variables to be initialized before the specified perspective
		return locals.Where(i => i.Writes.Any() && IsInitializedBefore(flow, flow.IndexOf(perspective), initializations[i]))
			.Concat(GetUnlinkableVariables(flow, initializations, scope.Parent!, perspective, until)).ToList();
	}

	/// <summary>
	/// Generates link code for all the specified variables and outputs it to the specified destination node
	/// </summary>
	private static void LinkAll(Context context, Node destination, List<Variable> variables)
	{
		foreach (var variable in variables)
		{
			destination.Add(LinkObject(context, new VariableNode(variable)));
		}
	}

	/// <summary>
	/// Generates unlink code for all the specified variables and outputs it to the specified destination node
	/// </summary>
	private static void UnlinkAll(Context context, Node destination, List<Variable> variables)
	{
		foreach (var variable in variables)
		{
			destination.Add(UnlinkObject(context, new VariableNode(variable)));
		}
	}

	/// <summary>
	/// Generates code which destructs all the specified stack allocations
	/// </summary>
	public static void DestructAll(Node destination, IEnumerable<StackAddressNode> allocations)
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
	/// Generates code that destructs the specified node based on the specified type
	/// </summary>
	public static void Destruct(Node destination, Node value, Type type)
	{
		var implementation = type.Destructors.GetImplementation() ?? throw new ApplicationException("Missing destructor");

		// Do not call the destructor if it is empty
		if (implementation.IsEmpty) return;

		destination.Add(new LinkNode(value, new FunctionNode(implementation)));
	}

	/// <summary>
	/// Registers stack allocations so that they will be destroyed properly
	/// </summary>
	public static void DestructStackAllocations(Node root, Dictionary<Node, ScopeDestructionDescriptor> scopes)
	{
		// Find all the stack allocations inside the specified node
		var allocations = root.FindAll(NodeType.STACK_ADDRESS).Distinct();

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
	/// Handles garbage collection at the specified return statement.
	/// Garbage collection unlinks the specified variables and destructs the specified stack allocations.
	/// </summary>
	/// <param name="context">Context of the specified scope</param>
	/// <param name="scope">The scope of the return statement</param>
	/// <param name="scopes">Information regarding garbage collection about scopes</param>
	/// <param name="terminator">The return statement to process</param>
	/// <param name="unlinkables">List of unlinkable variables that must be unlinked before returning</param>
	/// <param name="allocations">List of allocations that must be destructed before returning</param>
	private static void ProcessReturnTermination(Context context, Node scope, Dictionary<Node, ScopeDestructionDescriptor> scopes, ReturnNode terminator, List<Variable> linkables, List<Variable> unlinkables, List<StackAddressNode> allocations)
	{
		var value = terminator.Value!;
		var type = value.GetType();

		// Determine whether the returned value is a function call
		var is_value_function_call = Common.IsFunctionCall(Analyzer.GetSource(value));

		// Do not add any special operations if there are no variables to be destructed
		if (!allocations.Any() && !unlinkables.Any() && !linkables.Any())
		{
			// 1. Function calls can not be linked, because their return values are already linked
			// 2. Ensure the returned value is linkable
			if (is_value_function_call || !IsTypeLinkable(type)) return;

			// Link the returned value
			terminator.Insert(LinkObject(context, value));
			return;
		}

		// Remove the value from the return statement
		value.Remove();

		// Load the return value into a temporary variable
		var variable = context.DeclareHidden(type);

		// Create a temporary node where the generated nodes will be placed
		var container = new Node();

		container.Add(new OperatorNode(Operators.ASSIGN).SetOperands(new VariableNode(variable), value));

		// 1. Function calls can not be linked, because their return values are already linked
		// 2. Ensure the returned value is linkable
		if (!is_value_function_call && IsTypeLinkable(type))
		{
			// Link the returned value
			container.Add(LinkObject(context, value));
		}

		// Link returned variables
		LinkAll(context, container, linkables);

		// Unlink all the variables
		UnlinkAll(context, container, unlinkables);

		// Destruct all the stack allocations
		DestructAll(container, scopes[scope].Allocations);

		// Set the temporary variable as the return value
		terminator.Add(new VariableNode(variable));

		// Add the generated nodes before the return statement
		terminator.InsertChildren(container);
	}

	/// <summary>
	/// Handles garbage collection at the specified scope that does not have a return value.
	/// Garbage collection unlinks the specified variables and destructs the specified stack allocations.
	/// </summary>
	private static void ProcessScopeWithoutReturnValue(
		StatementFlow flow,
		Dictionary<Variable, VariableAssignmentDescriptor> initializations,
		Context context,
		Context until,
		Node perspective,
		ScopeDestructionDescriptor descriptor,
		Node destination
	) {
		// Since the scope has a return value and complex expressions are extracted from it,
		// we can safely unlink all the variables and destruct all the stack allocations before it
		var variables = GetUnlinkableVariables(flow, initializations, context, perspective, until);
		if (!variables.Any()) return;

		// Unlink all the variables
		UnlinkAll(context, destination, variables);

		// Destruct all the stack allocations
		DestructAll(destination, descriptor.Allocations);
	}

	/// <summary>
	/// Handles garbage collection at the specified scope that does not have a return value.
	/// Garbage collection unlinks the specified variables and destructs the specified stack allocations.
	/// </summary>
	/// <param name="scopes">Information regarding garbage collection about scopes</param>
	/// <param name="scope">The scope to process</param>
	private static void ProcessScopeWithoutReturnValue(StatementFlow flow, Context[] contexts, Dictionary<Variable, VariableAssignmentDescriptor> initializations, Node scope, ScopeDestructionDescriptor descriptor)
	{
		// Since the scope has a return value and complex expressions are extracted from it,
		// we can safely unlink all the variables and destruct all the stack allocations before it
		var context = ((IScope)scope).GetContext();
		var variables = GetUnlinkableLocalVariables(flow, initializations, context, contexts, scope);
		if (!variables.Any()) return;

		// Unlink all the variables
		UnlinkAll(context, scope, variables);

		// Destruct all the stack allocations
		DestructAll(scope, descriptor.Allocations);
	}

	/// <summary>
	/// Handles garbage collection at the specified scope that has a return value.
	/// Garbage collection unlinks the specified variables and destructs the specified stack allocations.
	/// </summary>
	/// <param name="scopes">Information regarding garbage collection about scopes</param>
	/// <param name="scope">The scope to process</param>
	private static void ProcessScopeWithReturnValue(StatementFlow flow, Context[] contexts, Dictionary<Variable, VariableAssignmentDescriptor> initializations, Node scope, ScopeDestructionDescriptor descriptor)
	{
		// Since the scope has a return value and complex expressions are extracted from it,
		// we can safely unlink all the variables and destruct all the stack allocations before it
		var context = ((IScope)scope).GetContext();
		var variables = GetUnlinkableLocalVariables(flow, initializations, context, contexts, scope);
		if (!variables.Any()) return;

		var container = new Node();

		// Unlink all the variables
		UnlinkAll(context, container, variables);

		// Destruct all the stack allocations
		DestructAll(container, descriptor.Allocations);

		// If there is nothing to insert before the return value, do nothing
		if (container.First == null) return;

		var return_value = scope.Last!;

		// If the return value is complex, we need to extract it into a temporary variable
		if (!return_value.Is(NodeType.VARIABLE, NodeType.NUMBER))
		{
			// Remove the return value from the scope, because it will be replaced by a temporary variable
			return_value.Remove();

			// Create a temporary variable, which will hold the result of the return value
			// Determine the result of the return value by using another conditional statement
			var variable = context.DeclareHidden(Primitives.CreateBool());
			var position = return_value.Position;

			// Initialize the temporary variable to false
			scope.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(variable),
				new CastNode(new NumberNode(Parser.Format, 0L, position), new TypeNode(variable.Type!))
			));

			// Create a conditional statement with the return value as the condition
			var conditional = new IfNode(new Context(return_value.GetParentContext()), return_value, new Node(), position, null);

			// If the condition is true, set the temporary variable to true
			var assignment = new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(variable),
				new CastNode(new NumberNode(Parser.Format, 1L, position), new TypeNode(variable.Type!))
			);

			conditional.Body.Add(assignment);

			// Add the conditional statement to the scope
			scope.Add(conditional);

			// Add the temporary variable as the return value of the scope
			return_value = new VariableNode(variable);
			scope.Add(return_value);
		}

		// Add the generated nodes before the return value
		return_value.InsertChildren(container);
	}

	/// <summary>
	/// Determines whether the specified return value is complex or not.
	/// Complex return values are those that must be computed using a separate statement before unlinking variables.
	/// If the return value is not complex, it consists of constants and local variables, so it can be computed after unlinking variables.
	/// If the return value is not complex, this function also removes returned local variables from the unlinking list.
	/// </summary>
	private static bool PrepareReturnValue(Node source, List<Variable> linkables, List<Variable> unlinkables)
	{
		// Returned parameters need to be linked
		if (source.Instance == NodeType.VARIABLE && source.To<VariableNode>().Variable.IsParameter && source.To<VariableNode>().Variable.Type!.IsPack)
		{
			linkables.Add(source.To<VariableNode>().Variable);
			return false; // The return value is not complex
		}

		// Returned local variables do not need to be linked, instead they can be removed from the unlinking list
		if (source.Instance == NodeType.VARIABLE && source.To<VariableNode>().Variable.IsLocal)
		{
			// If the return value is a local variable, it can be removed from the unlinking list
			unlinkables.Remove(source.To<VariableNode>().Variable);
			return false; // The return value is not complex
		}

		if (source.Instance == NodeType.PACK)
		{
			// If the return value contains memory accesses or function calls, flag the return value as complex
			var is_complex = source.Find(NodeType.CALL, NodeType.FUNCTION, NodeType.LINK, NodeType.ACCESSOR) != null;

			// Find all the parameters that are returned, they can be linked
			var parameters = source.FindAll(i => i.Instance == NodeType.VARIABLE && i.To<VariableNode>().Variable.IsParameter && i.Parent!.Instance == NodeType.PACK);

			foreach (var parameter in parameters)
			{
				linkables.Add(parameter.To<VariableNode>().Variable);
			}

			// Find all the local variables that are returned, they can be removed from the unlinking list
			var locals = source.FindAll(i => i.Instance == NodeType.VARIABLE && i.To<VariableNode>().Variable.IsLocal && i.Parent!.Instance == NodeType.PACK);

			foreach (var local in locals)
			{
				unlinkables.Remove(local.To<VariableNode>().Variable);
			}

			return is_complex; // Return whether the return value is complex
		}

		// Assume the return value is complex, this should be safe
		return !source.Is(NodeType.NUMBER, NodeType.STRING);
	}

	/// <summary>
	/// Generates all the unlinker code under the specified root node
	/// </summary>
	private static void CreateAllScopeUnlinkers(FunctionImplementation implementation)
	{
		var root = implementation.Node!;
		var flow = new StatementFlow(root);
		var assignment_descriptors = GetLinkableVariableAssignmentDescriptors(implementation, flow, root);

		// Find all the scopes
		var scopes = root.FindAll(i => ReconstructionAnalysis.IsScope(i)).ToDictionary(i => i, _ => new ScopeDestructionDescriptor());
		var contexts = scopes.Keys.Select(i => ((IScope)i).GetContext()).ToArray();

		// Add the root since it is a scoped node
		scopes.Add(root, new ScopeDestructionDescriptor());

		var returns = root.FindAll(NodeType.RETURN).Cast<ReturnNode>().ToArray();
		var controls = root.FindAll(NodeType.COMMAND).Cast<CommandNode>().ToArray();

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
			// Find the scope and context of the terminator
			var scope = statement.FindParent(i => ReconstructionAnalysis.IsScope(i)) ?? throw new ApplicationException("Return statement did not have a parent scope");
			var context = ((IScope)scope).GetContext();

			// Collect all unlinkable variables and destructible allocations in the scope
			var linkables = new List<Variable>();
			var unlinkables = GetUnlinkableVariables(flow, assignment_descriptors, context, statement);
			var allocations = scopes[scope].Allocations;

			// If the return statement has a value, it might need to be moved, because the destructors must be executed last
			if (statement.Value != null)
			{
				// Get the actual value which will be returned
				var source = Analyzer.GetSource(statement.Value)!;
				var is_complex = PrepareReturnValue(source, linkables, unlinkables);

				if (is_complex)
				{
					ProcessReturnTermination(context, scope, scopes, statement, linkables, unlinkables, allocations);
					continue;
				}
			}

			var container = new Node();

			// Link returned variables
			LinkAll(context, container, linkables);

			// Unlink all the variables
			UnlinkAll(context, container, unlinkables);

			// Destruct all the stack allocations
			DestructAll(container, scopes[scope].Allocations);

			// Add the generated nodes before the return statement
			statement.InsertChildren(container);
		}

		foreach (var control in controls)
		{
			var loop = control.FindParent(NodeType.LOOP);
			if (loop == null) throw new ApplicationException("Loop control statement did not have a parent loop");

			// Get the the last context included in garbage collection at the control node (trace contexts up from the control node)
			var until = loop.To<LoopNode>().Body.Context;

			// Get the scope descriptor of the scope that contains the control statement
			var scope = control.FindParent(NodeType.SCOPE);
			if (scope == null) throw new ApplicationException("Loop control statement did not have a parent scope");

			// Get the context of the scope that contains the control statement
			var context = ((IScope)scope).GetContext();

			var container = new Node();
			ProcessScopeWithoutReturnValue(flow, assignment_descriptors, context, until, control, scopes[scope], container);

			// Insert the generated nodes before the control statement
			control.InsertChildren(container);
		}

		foreach (var iterator in scopes)
		{
			var scope = iterator.Key.To<ScopeNode>();
			var descriptor = iterator.Value;

			// Skip scopes which are terminated by a return statement
			if (descriptor.IsTerminated) continue;

			if (scope.IsValueReturned) ProcessScopeWithReturnValue(flow, contexts, assignment_descriptors, scope, descriptor);
			else ProcessScopeWithoutReturnValue(flow, contexts, assignment_descriptors, scope, descriptor);
		}
	}

	/// <summary>
	/// Returns whether specified variable can be initialized (not always) at the specified position.
	/// Variable is considered initialized at the specified perspective, if it can be reached from a defining assignment directly (not indirectly) without hitting undefining assignments.
	/// </summary>
	private static bool IsInitializedBefore(StatementFlow flow, int position, VariableAssignmentDescriptor descriptor)
	{
		// Assume parameters are always initialized
		if (descriptor.Variable.IsParameter) return true;

		var obstacles = descriptor.Undefinitions;
		var denylist = new SortedSet<int>();

		foreach (var assignment in descriptor.Definitions)
		{
			// If the specified position represents one of the assignments, do not compare it
			if (assignment >= position) continue;

			// If the specified position is reachable from the assignment, the variable can be initialized at the position
			var result = flow.GetExecutablePositions(assignment, obstacles, new List<int> { position }, denylist, int.MaxValue);
			if (result == null) throw new ApplicationException("Failed to determine if variable can be initialized at the specified position");
			if (result.Count > 0) return true;

			denylist.Clear();
		}

		return false;
	}

	/// <summary>
	/// Returns the indices of all assignments, which initialize the specified variable
	/// </summary>
	private static VariableAssignmentDescriptor GetVariableAssignmentDescriptor(StatementFlow flow, Variable variable, VariableDescriptor descriptor)
	{
		var definitions = new List<int>();
		var undefinitions = new List<int>();

		foreach (var assignment in descriptor.Writes)
		{
			var position = flow.IndexOf(assignment.Node);

			if (assignment.Value.Instance != NodeType.UNDEFINED && !Common.IsZero(assignment.Value))
			{
				definitions.Add(position);
			}
			else
			{
				undefinitions.Add(position);
			}
		}

		return new VariableAssignmentDescriptor(variable, definitions.ToArray(), undefinitions.ToArray());
	}

	/// <summary>
	/// Returns assignment descriptors for all variables in the specified context
	/// </summary>
	private static Dictionary<Variable, VariableAssignmentDescriptor> GetLinkableVariableAssignmentDescriptors(FunctionImplementation implementation, StatementFlow flow, Node root)
	{
		var descriptors = GeneralAnalysis.GetVariableDescriptors(implementation, root);
		var assignment_descriptors = new Dictionary<Variable, VariableAssignmentDescriptor>();

		foreach (var iterator in descriptors)
		{
			var variable = iterator.Key;
			var descriptor = iterator.Value;
			if (!IsTypeLinkable(variable.Type!)) continue;

			// Get an assignment descriptor for the variable
			assignment_descriptors.Add(variable, GetVariableAssignmentDescriptor(flow, variable, descriptor));
		}

		return assignment_descriptors;
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
	/// 

	/// <summary>
	/// Returns whether the specified node is a local variable that is not initialized at its position
	/// </summary>
	public static bool IsUninitializedLocalVariable(StatementFlow flow, Dictionary<Variable, VariableAssignmentDescriptor> assignment_descriptors, Node value)
	{
		// 1. Ensure the value is a local variable
		var is_local_variable = value.Instance == NodeType.VARIABLE && value.To<VariableNode>().Variable.IsPredictable;
		if (!is_local_variable) return false;

		// 2. Ensure the variable is not initialized at its position
		var variable = value.To<VariableNode>().Variable;
		var position = flow.IndexOf(value);
		var assignment_descriptor = assignment_descriptors[variable];

		return !IsInitializedBefore(flow, position, assignment_descriptor);
	}

	/// <summary>
	/// Returns whether the specified value represents a returned member of a pack.
	/// If a local pack is accessed with a link node, it must be a return value from a function call.
	/// </summary>
	public static bool IsReturnedPackMember(Node node)
	{
		var root = node.GetBottomLeft();
		return root != null && root.Instance == NodeType.VARIABLE && root.Parent!.Instance == NodeType.LINK && root.GetType().IsPack;
	}

	/// <summary>
	/// Do the following transformations to the specified function:
	/// Case 1:
	/// $destination = b
	/// =>
	/// link(b)
	/// unlink($destination) # This is removed, if the destination is a local variable and it is not initialized
	/// $destination = b
	/// 
	/// Complex memory source or destination:
	/// x.y. ... .z or x[y]
	/// 
	/// Case 2:
	/// $destination = $complex-memory-source
	/// =>
	/// x = $complex-memory-source
	/// link(x)
	/// unlink($destination) # This is removed, if the destination is a local variable and it is not initialized
	/// $destination = x
	/// 
	/// Case 3:
	/// $destination = f(...)
	/// =>
	/// x = f(...)
	/// unlink($destination)
	/// $destination = x
	/// 
	/// If the destination is a local variable, it must be initialized. If it is not initialized, then do nothing.
	/// </summary>
	private static void CreateAllScopeLinkers(FunctionImplementation implementation)
	{
		var root = implementation.Node!;

		// Create a statement flow for determining whether variables are initialized before certain positions
		var flow = new StatementFlow(root);
		var assignment_descriptors = GetLinkableVariableAssignmentDescriptors(implementation, flow, root);

		// Find all assignments in order to inspect local variables
		var assignments = root.FindAll(i => i.Is(Operators.ASSIGN));

		foreach (var assignment in assignments)
		{
			var destination = assignment.Left;
			var value = assignment.Right;
			var source = Analyzer.GetSource(value)!;

			// Do not link if the source represents an undefined value
			if (source.Instance == NodeType.UNDEFINED) continue;

			var destination_type = destination.GetType();
			var value_type = value.GetType();

			// Skip if the destination is not linkable
			if (!IsTypeLinkable(destination_type)) continue;

			var environment = assignment.GetParentContext();

			// Case 3:
			if (Common.IsFunctionCall(source))
			{
				// If the destination is a local variable, it must be initialized. If it is not initialized, then do nothing.
				if (IsUninitializedLocalVariable(flow, assignment_descriptors, destination)) continue;

				// 1. Create a temporary variable, which will store the returned value of the function call
				var temporary = environment.DeclareHidden(value_type);

				// 2. Replace the function call with the temporary variable: $destination = f(...) => $destination = x
				value.Replace(new VariableNode(temporary, value.Position));

				// 3. Create the statement, which stores the returned value in the temporary variable: x = f(...)
				assignment.Insert(new OperatorNode(Operators.ASSIGN).SetOperands(new VariableNode(temporary, assignment.Position), value));

				// 4. Create the statement, which unlinks the destination: unlink($destination)
				assignment.Insert(UnlinkObject(environment, destination));
				continue;
			}

			// Case 2:
			if (source.Instance != NodeType.VARIABLE)
			{
				// Returned pack members should not be linked
				if (IsReturnedPackMember(source)) continue;

				// 1. Create a temporary variable, which will store the source
				var temporary = environment.DeclareHidden(value_type);

				// 2. Replace the source with the temporary variable: $destination = $complex-memory-source => $destination = x
				source.Replace(new VariableNode(temporary, source.Position));

				// 3. Create the statement, which stores the source in the temporary variable: x = $complex-memory-source
				assignment.Insert(new OperatorNode(Operators.ASSIGN).SetOperands(new VariableNode(temporary, assignment.Position), source));

				// 4. Link the temporary variable before unlinking the destination: link(x)
				assignment.Insert(LinkObject(environment, new VariableNode(temporary, assignment.Position)));

				// Do not unlink the destination if it is a local variable and it is not initialized
				if (IsUninitializedLocalVariable(flow, assignment_descriptors, destination)) continue;

				// 5. Create the statement, which unlinks the destination: unlink($destination)
				assignment.Insert(UnlinkObject(environment, destination));
				continue;
			}

			// Case 1:
			// 1. Create the statement, which links the source: link(b)
			assignment.Insert(LinkObject(environment, source));

			// Do not unlink the destination if it is a local variable and it is not initialized
			if (IsUninitializedLocalVariable(flow, assignment_descriptors, destination)) continue;

			// 2. Create the statement, which unlinks the destination: unlink($destination)
			assignment.Insert(UnlinkObject(environment, destination));
		}
	}

	/// <summary>
	/// Links the parameters in the specified function, if they are edited in the function body.
	/// </summary>
	private static void LinkEditedParameters(FunctionImplementation implementation)
	{
		var root = implementation.Node!;
		var descriptors = GeneralAnalysis.GetVariableDescriptors(implementation, root);

		foreach (var parameter in implementation.Parameters)
		{
			// 1. Skip unlinkable parameters
			// 2. Skip if the parameter is not edited
			if (!IsTypeLinkable(parameter.Type!) || descriptors[parameter].Writes.Count == 0) continue;

			// 3. Link the parameter at the beginning of the function
			root.Insert(root.First, LinkObject(implementation, new VariableNode(parameter, implementation.Metadata.Start)));
		}
	}

	/// <summary>
	/// Generates garbage collecting for the specified function implementation
	/// </summary>
	public static void Generate(FunctionImplementation implementation)
	{
		#warning TODO: Unlink discarded objects
		CreateAllScopeUnlinkers(implementation);
		CreateAllScopeLinkers(implementation);
		LinkEditedParameters(implementation);
	}

	/// <summary>
	/// Creates all required functions from the specified context
	/// </summary>
	public static void CreateAllRequiredOverloads(Context root)
	{
		if (!Analysis.IsGarbageCollectorEnabled) return;

		foreach (var type in Common.GetAllTypes(root))
		{
			// Create the destructor of the current type if it is linkable
			if (type.IsStatic || !IsTypeLinkable(type)) continue;

			type.Destructors.GetImplementation();
		}
	}
}