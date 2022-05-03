using System;
using System.Collections.Generic;
using System.Linq;

public class VariableWrite
{
	public Node Node { get; set; }
	public List<Node> Dependencies { get; } = new List<Node>();
	public List<Node> Assignable { get; } = new List<Node>();
	public bool IsDeclaration { get; set; } = false;
	public Node Value => Node.Last!;

	public VariableWrite(Node node)
	{
		Node = node;
	}
}

public class VariableDescriptor
{
	public List<VariableWrite> Writes { get; } = new List<VariableWrite>();
	public List<Node> Reads { get; }

	public VariableDescriptor(Variable variable, List<Node> reads, List<Node> writes)
	{
		Writes = writes.Select(i => new VariableWrite(i)).ToList();
		Reads = reads;

		if (Writes.Any()) { Writes[0].IsDeclaration = variable.IsLocal; }
	}
}

public static class GeneralAnalysis
{
	/// <summary>
	/// Finds all nodes which pass the specified filter, favoring right side of assignment operations first
	/// </summary>
	private static List<Node> FindAll(Node root, NodeType type)
	{
		var nodes = new List<Node>();
		var iterator = (IEnumerable<Node>)root;

		if (root.Is(OperatorType.ACTION))
		{
			iterator = iterator.Reverse();
		}

		foreach (var i in iterator)
		{
			if (i.Instance == type)
			{
				nodes.Add(i);
			}

			nodes.AddRange(FindAll(i, type));
		}

		return nodes;
	}

	/// <summary>
	/// Returns all the nodes which are edited from the specified set of nodes
	/// </summary>
	private static List<Node> GetWrites(IEnumerable<Node> references)
	{
		return references.Where(i => Analyzer.IsEdited(i.To<VariableNode>())).ToList();
	}

	/// <summary>
	/// Produces a descriptor for the specified variable from the specified set of variable nodes
	/// </summary>
	private static VariableDescriptor GetVariableDescriptor(Variable variable, Dictionary<Variable, Node[]> nodes)
	{
		if (!nodes.TryGetValue(variable, out Node[]? all)) return new VariableDescriptor(variable, new List<Node>(), new List<Node>());
		
		var reads = new List<Node>(all);
		var writes = GetWrites(reads);

		for (var i = 0; i < writes.Count; i++)
		{
			for (var j = 0; j < reads.Count; j++)
			{
				if (!ReferenceEquals(writes[i], reads[j])) continue;
				reads.RemoveAt(j);
				break;
			}
		}

		return new VariableDescriptor(variable, reads, writes.Select(i => Analyzer.GetEditor(i)).ToList());
	}

	/// <summary>
	/// Produces descriptors for all the variables defined in the specified function implementation
	/// </summary>
	public static Dictionary<Variable, VariableDescriptor> GetVariableDescriptors(FunctionImplementation implementation, Node root)
	{
		var nodes = FindAll(root, NodeType.VARIABLE).GroupBy(i => i.To<VariableNode>().Variable).ToDictionary(i => i.Key, i => i.ToArray());
		var variables = implementation.Locals.Concat(implementation.Variables.Values).Distinct();

		return new Dictionary<Variable, VariableDescriptor>
		(
			variables.Select(i => new KeyValuePair<Variable, VariableDescriptor>(i, GetVariableDescriptor(i, nodes)))
		);
	}

	/// <summary>
	/// Registers all dependencies for the specified variable writes
	/// </summary>
	private static void RegisterWriteDependencies(VariableDescriptor descriptor, StatementFlow flow)
	{
		descriptor.Writes.ForEach(i => i.Dependencies.Clear());

		// Get the indices of all writes
		var obstacles = descriptor.Writes.Select(i => flow.IndexOf(i.Node)).ToArray();

		// Group all reads by their 'statement index'
		var nodes = new Dictionary<int, List<Node>>();

		foreach (var read in descriptor.Reads)
		{
			var index = flow.IndexOf(read);

			if (nodes.ContainsKey(index))
			{
				nodes[index].Add(read);
			}
			else
			{
				nodes[index] = new List<Node> { read };
			}
		}

		var positions = nodes.Keys.ToList();

		for (var i = 0; i < descriptor.Writes.Count; i++)
		{
			var start = obstacles[i];

			var executable = flow.GetExecutablePositions(start + 1, obstacles, new List<int>(positions), new SortedSet<int>());
			if (executable == null) { executable = positions; }

			if (executable.Any())
			{
				descriptor.Writes[i].Dependencies.AddRange(executable.SelectMany(i => nodes[i]));
			}
		}
	}

	/// <summary>
	/// Returns the variables on which the value of the specified write is dependent
	/// </summary>
	private static Variable[] GetWriteDependencies(Node write)
	{
		var value = write.Right;

		if (value.Is(NodeType.VARIABLE))
		{
			var variable = value.To<VariableNode>().Variable;
			if (variable.IsPredictable && !variable.IsConstant) return new[] { variable };
		}

		return value.FindAll(i =>
		{
			if (!i.Is(NodeType.VARIABLE)) return false;

			var variable = i.To<VariableNode>().Variable;
			return variable.IsPredictable && !variable.IsConstant && !variable.IsSelfPointer;

		}).Select(i => i.To<VariableNode>().Variable).ToArray();
	}

	/// <summary>
	/// Returns all variable nodes from the specified root while taking into account if the specified root is a variable node
	/// </summary>
	private static List<VariableNode> GetAllVariableUsages(Node root)
	{
		var usages = (List<Node>?)null;
		if (root.Instance == NodeType.VARIABLE) { usages = new List<Node>() { root }; }
		else { usages = root.FindAll(NodeType.VARIABLE); }
		return usages.Cast<VariableNode>().Where(i => i.Variable.IsPredictable).ToList();
	}

	/// <summary>
	/// Removes all local variable usages from the specified node tree 'from' and adds the new usages from the specified node tree 'to'
	/// </summary>
	private static void UpdateVariableUsages(Dictionary<Variable, VariableDescriptor> descriptors, Node from, Node? to)
	{
		// Find all variable usages from the node tree 'from' and remove them from descriptors
		var previous_usages = GetAllVariableUsages(from);

		foreach (var usage in previous_usages)
		{
			var descriptor = descriptors[usage.To<VariableNode>().Variable];
			var reads = descriptor.Reads;

			for (var i = 0; i < reads.Count; i++)
			{
				if (!ReferenceEquals(reads[i], usage)) continue;
				reads.RemoveAt(i);
				break;
			}
		}

		/// NOTE: We do not need to add the new usages of the assigned variable into the dependency lists of those writes that affect them, because those writes have been processed already
		if (to == null) return;

		// Add all the variable usages from the node tree 'to' into the descriptors
		var assignment_usages = GetAllVariableUsages(to);

		foreach (var usage in assignment_usages)
		{
			descriptors[usage.To<VariableNode>().Variable].Reads.Add(usage);
		}
	}

	/// <summary>
	/// Adds all the variable usages from the specified node tree 'from' into the specified descriptors
	/// </summary>
	private static void AddVariableUsagesFrom(Dictionary<Variable, VariableDescriptor> descriptors, Node from)
	{
		var usages = GetAllVariableUsages(from);

		foreach (var usage in usages)
		{
			descriptors[usage.To<VariableNode>().Variable].Reads.Add(usage);
		}
	}

	/// <summary>
	/// Assigns the value of the specified write to the specified reads
	/// </summary>
	private static bool Assign(Variable variable, VariableWrite write, bool recursive, Dictionary<Variable, VariableDescriptor> descriptors, VariableDescriptor descriptor, StatementFlow flow)
	{
		var assigned = false;

		foreach (var read in write.Assignable)
		{
			// Find the root of the expression which contains the root and approximate the cost of the expression
			var root = Common.GetExpressionRoot(read);
			var before = Analysis.GetCost(root);

			// Clone the assignment value and find all the variable references
			var value = write.Value.Clone();
			read.Replace(value);

			// Find the root of the expression which contains the root and approximate the cost of the expression
			root = Common.GetExpressionRoot(value);

			// Clone the root so that it can be modified
			var optimized = root.Clone();

			// Optimize the root where the value was assigned
			Analysis.OptimizeAllExpressions(optimized);

			// Approximate the new cost of the root
			var after = Analysis.GetCost(optimized);

			// 1. If the assignment is recursive, all the assignments must be done
			// 2. If the cost has decreased, the assignment should be done in most cases
			if (!recursive && after > before)
			{
				// Revert back the changes since the cost has risen
				value.Replace(read);
			}
			else
			{
				// Remove the read from the write dependencies
				for (var i = 0; i < write.Dependencies.Count; i++)
				{
					if (!ReferenceEquals(write.Dependencies[i], read)) continue;
					write.Dependencies.RemoveAt(i);
					break;
				}

				// Remove the read from the reads
				for (var i = 0; i < descriptor.Reads.Count; i++)
				{
					if (!ReferenceEquals(descriptor.Reads[i], read)) continue;
					descriptor.Reads.RemoveAt(i);
					break;
				}

				// Update the local variable usages
				AddVariableUsagesFrom(descriptors, root);
				assigned = true;
			}
		}

		if (write.Dependencies.Count == 0)
		{
			// If the write declares the variable and the variable is still used, this write needs to be preserved in simpler form, so that the variable is declared
			if (write.IsDeclaration && descriptor.Writes.Count > 1)
			{
				var replacement = new OperatorNode(Operators.ASSIGN).SetOperands(
					new VariableNode(variable),
					new UndefinedNode(variable.Type!, variable.GetRegisterFormat())
				);

				write.Node.Replace(replacement);
				flow.Replace(write.Node, replacement);
			}
			else
			{
				// Remove the write from the flow
				write.Node.Remove();
				flow.Remove(write.Node);
			}

			// Remove all the variable usages from the write
			UpdateVariableUsages(descriptors, write.Node, null);

			// Finally, remove the write from the current descriptor
			for (var i = 0; i < descriptor.Writes.Count; i++)
			{
				if (!ReferenceEquals(descriptor.Writes[i], write)) continue;
				descriptor.Writes.RemoveAt(i);
				break;
			}
		}

		return assigned;
	}

	/// <summary>
	/// Returns whether the assignment is assignable
	/// </summary>
	private static bool IsAssignable(Node assignment)
	{
		// 1. True if the node is simple and does not represent assignment
		// 2. True if the node represent a free cast
		return assignment.Find(i => {
			if (!(i.Is(NodeType.VARIABLE, NodeType.NUMBER, NodeType.DATA_POINTER, NodeType.TYPE, NodeType.OPERATOR, NodeType.STACK_ADDRESS) && !i.Is(OperatorType.ACTION)) &&
				 !(i.Is(NodeType.CAST) && i.To<CastNode>().IsFree()))
			{
				return true;
			}

			// If the node is a variable, it can not represent a static variable
			return i.Is(NodeType.VARIABLE) && i.To<VariableNode>().Variable.IsStatic;
		}) == null;
	}

	/// <summary>
	/// Safety check, which looks for assignments inside assignments. Such assignments have a high chance of causing trouble.
	/// </summary>
	private static void CaptureNestedAssignments(Node root)
	{
		var assignments = root.FindAll(i => i.Is(Operators.ASSIGN));

		foreach (var assignment in assignments)
		{
			for (var iterator = assignment.Parent; iterator != null; iterator = iterator.Parent)
			{
				if (iterator.Is(NodeType.OPERATOR) && !iterator.Is(OperatorType.LOGIC)) throw new ApplicationException("Found a nested assignment while optimizing");
			}
		}
	}

	/// <summary>
	/// Remove the described variable, if it represents an object and is only written into.
	/// Returns whether the described variable was removed.
	/// </summary>
	private static bool RemoveUnreadVariables(Variable variable, VariableDescriptor descriptor)
	{
		// If something is written into a parameter object, it can not be determined whether the written value is used elsewhere
		if (variable.IsParameter) return false;

		// The writes must use the registered allocation function or stack allocation
		foreach (var write in descriptor.Writes)
		{
			var value = Analyzer.GetSource(write.Value);

			if (value.Instance == NodeType.STACK_ADDRESS) continue;
			if (value.Instance == NodeType.FUNCTION && value.To<FunctionNode>().Function == Parser.AllocationFunction) continue;

			return false;
		}

		// If any of the reads of the variable is not used to write into the object, just abort
		foreach (var read in descriptor.Reads)
		{
			if (read.Parent!.Instance == NodeType.SCOPE) continue;

			var iterator = read.Parent;
			if (iterator!.Instance == NodeType.CAST) { iterator = iterator.Parent; }
			if (iterator!.Instance != NodeType.LINK || !Analyzer.IsEdited(iterator)) return false; 
		}

		// Collect all the editors to remove
		var editors = new List<VariableWrite>(descriptor.Writes);

		foreach (var read in descriptor.Reads)
		{
			if (read.Parent!.Instance == NodeType.SCOPE)
			{
				read.Remove();
				continue;
			}

			var iterator = read.Parent;
			if (iterator!.Instance == NodeType.CAST) { iterator = iterator.Parent; }

			var editor = Analyzer.GetEditor(iterator!);
			editors.Add(new VariableWrite(editor));
		}

		// Now, replace the editors with their assigned values
		foreach (var editor in editors)
		{
			editor.Node.Replace(Analyzer.GetSource(editor.Value));
		}

		return true;
	}

	/// <summary>
	/// Looks for assignments of the specified variable which can be inlined
	/// </summary>
	private static void AssignFunctionVariables(FunctionImplementation implementation, Node root)
	{
		var variables = implementation.Locals.Concat(implementation.Variables.Values).Distinct().ToArray();
		
		// Create assignments, which initialize the parameters
		/// NOTE: Fixes the situation, where the code contains a single conditional assignment to the parameter and one read.
		/// Without the initialization, the value of the single assignment would be inlined.
		var initializations = new List<Node>();

		foreach (var parameter in implementation.Variables.Values.Where(i => i.IsParameter))
		{
			var initialization = new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(parameter),
				new UndefinedNode(parameter.Type!, parameter.Type!.GetRegisterFormat())
			);

			initializations.Add(initialization);
			root.Insert(root.First, initialization);
		}

		var descriptors = GetVariableDescriptors(implementation, root);
		var flow = new StatementFlow(root);

		CaptureNestedAssignments(root);

		foreach (var variable in variables)
		{
			var descriptor = descriptors[variable];

			if (RemoveUnreadVariables(variable, descriptor))
			{
				descriptors.Remove(variable);
				continue;
			}

			RegisterWriteDependencies(descriptor, flow);

			var assignable = new List<Node>();

			foreach (var write in new List<VariableWrite>(descriptor.Writes))
			{
				// The initializations of parameters must be left intact
				if (variable.IsParameter && write.IsDeclaration) continue;

				// If the value of the write contains a call for example, it should not be assigned
				if (!IsAssignable(write.Node)) continue;

				// Collect all local variables that affect the value of the write. If any of these is edited, it means the value of the write changes
				var dependencies = GetWriteDependencies(write.Node);
				var obstacles = dependencies.SelectMany(i => descriptors[i].Writes).Select(i => i.Node).ToArray();

				// Collect all reads from the dependencies, where the value of the current write can be assigned
				var recursive = write.Value.Find(i => i.Is(variable)) != null;

				assignable.Clear();

				foreach (var read in write.Dependencies)
				{
					// If the read is dependent on any of the other writes, the value of the current write can not be assigned
					if (descriptor.Writes.Except(write).SelectMany(i => i.Dependencies).Any(i => ReferenceEquals(i, read))) continue;

					var assign = true;
					var from = flow.IndexOf(write.Node) + 1;
					var to = flow.IndexOf(read);

					// If the read happens before the edit, which is possible in loops for example, it is not reliable to get all the nodes between the edit and the read
					if (to < from) continue;

					// Find all assignments between the write and the read
					for (var i = from; i < to; i++)
					{
						var node = flow.Nodes[i];
						if (node == null || !node.Is(Operators.ASSIGN)) continue;

						var edited = Analyzer.GetEdited(node);
						if (!edited.Is(NodeType.VARIABLE)) continue;

						// If one of the dependencies is edited between the write and the read, the value of the write can not be assigned
						if (dependencies.Contains(edited.To<VariableNode>().Variable))
						{
							assign = false;
							break;
						}
					}

					if (assign) assignable.Add(read);
				}

				// 1. Recursive writes must be assigned to all their reads
				// 2. It is assumed that recursive code can not be assigned inside a loop, if the edited variable is created externally
				if (recursive && (assignable.Count != write.Dependencies.Count || write.Node.FindParent(NodeType.LOOP) != null)) continue;

				write.Assignable.Clear();
				write.Assignable.AddRange(assignable);

				Assign(variable, write, recursive, descriptors, descriptor, flow);
			}
		}

		// Remove the parameter initializations, because the should not be executed
		foreach (var initialization in initializations) { initialization.Remove(); }

		CaptureNestedAssignments(root);
	}

	/// <summary>
	/// Returns whether the specified node trees are equal
	/// </summary>
	private static bool IsTreeEqual(Node a, Node b)
	{
		if (!Equals(a, b)) return false;

		var x = a.First;
		var y = b.First;

		while (true)
		{
			if (x == null || y == null) return x == null && y == null;
			if (!IsTreeEqual(x, y)) return false;

			x = x.Next;
			y = y.Next;
		}
	}

	/// <summary>
	/// Looks for assignments which can be inlined
	/// </summary>
	private static Node Start(FunctionImplementation implementation, Node root)
	{
		var minimum_cost_snapshot = root;
		var minimum_cost = Analysis.GetCost(root);

		var result = (Node?)null;

		while (result == null || !IsTreeEqual(result, minimum_cost_snapshot))
		{
			result = minimum_cost_snapshot;

			var snapshot = minimum_cost_snapshot.Clone();

			if (Analysis.IsRepetitionAnalysisEnabled) 
			{
				snapshot = MemoryAccessAnalysis.Unrepeat(snapshot);
			}

			if (Analysis.IsMathematicalAnalysisEnabled) AssignFunctionVariables(implementation, snapshot);

			// Try to optimize all comparisons found in the current snapshot
			if (Analysis.IsMathematicalAnalysisEnabled) Analysis.OptimizeComparisons(snapshot);

			// Try to unwrap conditional statements whose outcome have been resolved
			if (Analysis.IsUnwrapAnalysisEnabled) UnwrapmentAnalysis.Start(implementation, snapshot);

			// Removes all statements which are not reachable
			ReconstructionAnalysis.RemoveUnreachableStatements(snapshot);

			// Finally, try to simplify all expressions
			if (Analysis.IsMathematicalAnalysisEnabled) Analysis.OptimizeAllExpressions(snapshot);

			ReconstructionAnalysis.RemoveRedundantInlineNodes(snapshot);

			// Calculate the complexity of the current snapshot
			var cost = Analysis.GetCost(snapshot);

			if (cost < minimum_cost)
			{
				// Since the current snapshot is less complex it should be used
				minimum_cost_snapshot = snapshot;
				minimum_cost = cost;
			}

			// NOTE: This is a repetition, but it is needed since some functions do not have variables
			// Finally, try to simplify all expressions
			if (Analysis.IsMathematicalAnalysisEnabled)
			{
				Analysis.OptimizeAllExpressions(snapshot);
			}

			// Calculate the complexity of the current snapshot
			cost = Analysis.GetCost(snapshot);

			if (cost < minimum_cost)
			{
				// Since the current snapshot is less complex it should be used
				minimum_cost_snapshot = snapshot;
				minimum_cost = cost;
			}
		}

		return result;
	}

	/// <summary>
	/// Removes variables from the specified function implementation if they are not referenced in any way
	/// </summary>
	private static void RemoveUnusedVariables(FunctionImplementation implementation, Node root)
	{
		var descriptors = GetVariableDescriptors(implementation, root);

		foreach (var iterator in descriptors)
		{
			var descriptor = iterator.Value;
			var variable = iterator.Key;

			// If the variable is used, skip it
			if (variable.IsParameter || descriptor.Reads.Any() || descriptor.Writes.Any()) continue;

			// If the variable is a pack, do not remove it, because we might have to use it later for debugging information for instance
			if (variable.Type!.IsPack) continue;

			var context = variable.Context;

			if (context.Variables.ContainsKey(variable.Name))
			{
				context.Variables.Remove(variable.Name);
				root.FindAll(NodeType.DECLARE).Cast<DeclareNode>().Where(i => i.Variable == variable).ForEach(i => i.Remove());
			}
		}
	}

	/// <summary>
	/// Optimizes the specified function using several methods such as variable assignment and simplifying values
	/// </summary>
	public static Node Optimize(FunctionImplementation implementation, Node root)
	{
		RemoveUnusedVariables(implementation, root);
		root = Start(implementation, root);
		RemoveUnusedVariables(implementation, root);
		return root;
	}
}