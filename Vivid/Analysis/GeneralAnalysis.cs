using System.Collections.Generic;
using System.Linq;
using System;

public class VariableWrite
{
	public Node Node { get; set; }
	public List<Node> Dependencies { get; } = new List<Node>();
	public List<Node> Assignable { get; } = new List<Node>();
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

	public VariableDescriptor(List<Node> reads, List<Node> writes)
	{
		Writes = writes.Select(i => new VariableWrite(i)).ToList();
		Reads = reads;
	}
}

public struct RedundantAssignment
{
	public Node Node;
	public Variable? Declaration;
}

public static class GeneralAnalysis
{
	/// <summary>
	/// Finds all nodes which pass the specified filter, favoring right side of assignment operations first
	/// </summary>
	private static List<Node> FindAll(Node root, Predicate<Node> filter)
	{
		var nodes = new List<Node>();
		var iterator = (IEnumerable<Node>)root;

		if (root.Is(OperatorType.ACTION))
		{
			iterator = iterator.Reverse();
		}

		foreach (var i in iterator)
		{
			if (filter(i))
			{
				nodes.Add(i);
			}

			nodes.AddRange(FindAll(i, filter));
		}

		return nodes;
	}

	/// <summary>
	/// Returns all the nodes which are edited from the specified set of nodes
	/// </summary>
	private static List<Node> GetWrites(IEnumerable<Node> references)
	{
		return references.Where(v => Analyzer.IsEdited(v.To<VariableNode>())).ToList();
	}

	/// <summary>
	/// Produces a descriptor for the specified variable from the specified set of variable nodes
	/// </summary>
	private static VariableDescriptor GetVariableDescriptor(Variable variable, IEnumerable<VariableNode> nodes)
	{
		var reads = nodes.Where(i => i.Is(variable)).Cast<Node>().ToList();
		var writes = GetWrites(reads);

		for (var i = 0; i < writes.Count; i++)
		{
			for (var j = 0; j < reads.Count; j++)
			{
				if (reads[j] == writes[i])
				{
					reads.RemoveAt(j);
					break;
				}
			}
		}

		return new VariableDescriptor(reads, writes.Select(i => Analyzer.GetEditor(i)).ToList());
	}

	/// <summary>
	/// Produces descriptors for all the variables defined in the specified function implementation
	/// </summary>
	private static Dictionary<Variable, VariableDescriptor> GetVariableDescriptors(FunctionImplementation implementation, Node root)
	{
		var nodes = FindAll(root, i => i.Is(NodeType.VARIABLE)).Cast<VariableNode>();
		var variables = implementation.Locals.Concat(implementation.Variables.Values).Distinct();

		return new Dictionary<Variable, VariableDescriptor>
		(
			variables.Select(i => new KeyValuePair<Variable, VariableDescriptor>(i, GetVariableDescriptor(i, nodes)))
		);
	}

	/// <summary>
	/// Registers all dependencies for the specified variable writes
	/// </summary>
	private static void RegisterWriteDependencies(Dictionary<Variable, VariableDescriptor> descriptors, Flow flow)
	{
		foreach (var descriptor in descriptors)
		{
			descriptor.Value.Writes.ForEach(i => i.Dependencies.Clear());

			var obstacles = descriptor.Value.Writes.Select(i => flow.Indices[i.Node]).ToArray();
			var nodes = new Dictionary<int, Node>(descriptor.Value.Reads.Select(i => new KeyValuePair<int, Node>(flow.Indices[i], i)));
			var positions = nodes.Keys.ToList();

			for (var i = 0; i < descriptor.Value.Writes.Count; i++)
			{
				var start = obstacles[i];
				var executable = flow.GetExecutablePositions(start, obstacles, new List<int>(positions), new SortedSet<int>());
				
				if (executable.Any())
				{
					descriptor.Value.Writes[i].Dependencies.AddRange(executable.Select(i => nodes[i]));
				}
			}
		}
	}

	/// <summary>
	/// Removes the statement while taking care of the calls which might be inside it
	/// </summary>
	private static void RemoveStatement(Node statement)
	{
		// Find all function calls inside the redundant write and replace the write with them
		var calls = statement.FindAll(i => i.Is(NodeType.FUNCTION, NodeType.CALL));

		if (calls.Any())
		{
			// Add all the calls under an inline node
			var inline = new InlineNode(statement.Position);
			calls.ForEach(inline.Add);

			statement.Replace(inline);
		}
		else
		{
			statement.Remove();
		}
	}
	
	/// <summary>
	/// Removes all the assigments which do not have any effect on the execution of the specified function
	/// </summary>
	private static void RemoveRedundantAssignments(FunctionImplementation implementation, Node root)
	{
		var descriptors = GetVariableDescriptors(implementation, root);
		var flow = new Flow(root);
		var redundants = new List<RedundantAssignment>();

		// Remove assignments such as: x = x
		foreach (var iterator in descriptors)
		{
			for (var i = iterator.Value.Writes.Count - 1; i >= 0; i--)
			{
				var write = iterator.Value.Writes[i];

				if (!write.Node.Is(Operators.ASSIGN) || !write.Value.Is(iterator.Key))
				{
					continue;
				}

				iterator.Value.Writes.RemoveAt(i);
				write.Node.Remove();
			}
		}

		foreach (var iterator in descriptors)
		{
			var descriptor = iterator.Value;
			var variable = iterator.Key;

			foreach (var write in descriptor.Writes)
			{
				var obstacles = descriptor.Writes.Where(i => i != write).Select(i => i.Node).ToArray();
				var required = false;

				foreach (var read in descriptor.Reads)
				{
					if (flow.IsReachableWithoutExecuting(read, write.Node, obstacles))
					{
						required = true;
						break;
					}
				}

				if (!required)
				{
					// Check whether this write is a declaration
					var is_declaration = descriptor.Writes.First() == write && variable.IsLocal;

					redundants.Add(new RedundantAssignment() 
					{ 
						Node = write.Node,
						Declaration = is_declaration ? variable : null 
					});
				}
			}
		}

		foreach (var redundant in redundants)
		{
			if (redundant.Declaration != null)
			{
				var declaration = new DeclareNode(redundant.Declaration);

				// Find all function calls inside the redundant write and replace the write with them
				var blocks = redundant.Node.FindTop(i => i.Is(NodeType.FUNCTION, NodeType.CALL, NodeType.INLINE));

				// If any of the calls in under a link node, select those link nodes
				blocks = blocks.Select(i => i.Parent!.Is(NodeType.LINK) ? i.Parent! : i).ToList();

				if (blocks.Any())
				{
					// Add all the calls under an inline node
					var inline = new InlineNode(redundant.Node.Position) { declaration };
					blocks.ForEach(inline.Add);

					redundant.Node.Replace(inline);
				}
				else
				{
					redundant.Node.Replace(declaration);
				}
			}
			else
			{
				RemoveStatement(redundant.Node);
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

			if (variable.IsPredictable && !variable.IsConstant)
			{
				return new[] { variable };
			}
		}

		return value.FindAll(i =>
		{
			if (!i.Is(NodeType.VARIABLE))
			{
				return false;
			}

			var variable = i.To<VariableNode>().Variable;
			return variable.IsPredictable && !variable.IsConstant && !variable.IsSelfPointer;
			
		}).Select(i => i.To<VariableNode>().Variable).ToArray();
	}

	/// <summary>
	/// Assigns the value of the specified write to the specified reads
	/// </summary>
	private static void Assign(VariableWrite write, IEnumerable<Node> reads)
	{
		var value = write.Value;

		foreach (var read in reads)
		{
			read.Replace(value.Clone());
		}
	}

	/// <summary>
	/// Returns whether the assignment is assignable
	/// </summary>
	private static bool IsAssignable(Node assignment)
	{
		return assignment.Find(i => !i.Is(NodeType.VARIABLE, NodeType.NUMBER, NodeType.OPERATOR, NodeType.DATA_POINTER, NodeType.TYPE) && !(i.Is(NodeType.CAST) && i.To<CastNode>().IsFree())) == null; 
	}

	/// <summary>
	/// Looks for assignments of the specified variable which can be inlined
	/// </summary>
	private static void Assign(FunctionImplementation implementation, Node root, Variable variable)
	{
		var descriptors = GetVariableDescriptors(implementation, root);

		if (!descriptors.TryGetValue(variable, out VariableDescriptor? descriptor))
		{
			return;
		}

		var flow = new Flow(root);

		RegisterWriteDependencies(descriptors, flow);

		foreach (var write in descriptor.Writes)
		{
			// Collect all local variabes that affect the value of the write. If any of these is edited, it means the value of the write changes
			var dependencies = GetWriteDependencies(write.Node);
			var obstacles = dependencies.SelectMany(i => descriptors[i].Writes).Select(i => i.Node).ToArray();

			var assignable = new List<Node>();
			var recursive = write.Value.Find(i => i.Is(variable)) != null;

			foreach (var read in write.Dependencies)
			{
				// If the value of the write contains calls, it should not be assigned
				if (!IsAssignable(write.Node))
				{
					continue;
				}

				// If the read is dependent on any of the other writes, the value of the current write can not be assigned
				if (descriptor.Writes.Where(i => !ReferenceEquals(i, write)).Any(i => i.Dependencies.Any(i => ReferenceEquals(i, read))))
				{
					continue;
				}

				// If any of the obstacles is between the write and the read, the value of the write can not be assigned
				if (obstacles.Any(i => flow.IsBetween(i, write.Node, read)))
				{
					continue;
				}

				assignable.Add(read);
			}

			// If the value contains the destination variable and it can not be assigned to all its usages or it is repeated, it should not be assigned
			if (recursive && (assignable.Count != write.Dependencies.Count || flow.IsRepeated(write.Node)))
			{
				continue;
			}

			if (!assignable.Any())
			{
				continue;
			}

			write.Assignable.AddRange(assignable);

			Assign(write, assignable);

			// Remove all the assigned locations from the descriptor
			assignable.ForEach(i => descriptor.Reads.Remove(i));
		}

		// Lastly, remove all the assignments which have no dependencies left
		for (var i = descriptor.Writes.Count - 1; i >= 0; i--)
		{
			var write = descriptor.Writes[i];

			// If there are no dependencies left for the current write, it can be removed
			if (write.Assignable.Count == write.Dependencies.Count)
			{
				RemoveStatement(write.Node);

				descriptor.Writes.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// Looks for assignments which can be inlined
	/// </summary>
	private static Node AssignVariables(FunctionImplementation implementation, Node root)
	{
		var minimum_cost_snapshot = root;
		var minimum_cost = Analysis.GetCost(root);
		var cost = 0L;
		
		var result = (Node?)null;

		while (result == null || !result.Equals(minimum_cost_snapshot))
		{
			result = minimum_cost_snapshot;

			var snapshot = minimum_cost_snapshot.Clone();

			if (Analysis.IsRepetitionAnalysisEnabled)
			{
				Analysis.Unrepeat(snapshot);
			}

			Start:

			var variables = implementation.Locals.Concat(implementation.Parameters);

			foreach (var variable in variables)
			{
				if (Analysis.IsMathematicalAnalysisEnabled)
				{
					Assign(implementation, snapshot, variable);
				}

				// Try to optimize all comparisons found in the current snapshot
				if (Analysis.IsMathematicalAnalysisEnabled && Analysis.OptimizeComparisons(snapshot))
				{
					goto Start;
				}

				// Try to unwrap conditional statements whose outcome have been resolved
				if (Analysis.IsUnwrapAnalysisEnabled && Analysis.UnwrapStatements(snapshot))
				{
					goto Start;
				}

				// Removes all statements which are not reachable
				if (Analysis.RemoveUnreachableStatements(snapshot))
				{
					goto Start;
				}

				// Finally, try to simplify all expressions
				if (Analysis.IsMathematicalAnalysisEnabled)
				{
					Analysis.OptimizeAllExpressions(snapshot);
				}

				ReconstructionAnalysis.SubstituteInlineNodes(snapshot);

				// Calculate the complexity of the current snapshot
				cost = Analysis.GetCost(snapshot);

				if (cost < minimum_cost)
				{
					// Since the current snapshot is less complex it should be used
					minimum_cost_snapshot = snapshot;
					minimum_cost = cost;
				}
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
	/// Optimizes the specified function using several methods such as variable assignment and simplifying values
	/// </summary>
	public static Node Optimize(FunctionImplementation implementation, Node root)
	{
		if (!Assembler.IsDebuggingEnabled)
		{
			RemoveRedundantAssignments(implementation, root);
		}
		
		root = AssignVariables(implementation, root);

		if (!Assembler.IsDebuggingEnabled)
		{
			RemoveRedundantAssignments(implementation, root);
		}

		return root;
	}
}