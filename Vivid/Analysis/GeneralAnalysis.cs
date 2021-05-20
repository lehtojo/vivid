using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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

[SuppressMessage("Microsoft.Maintainability", "CA1815", Justification = "This functionality is not needed")]
public struct RedundantAssignment
{
	public Node Node { get; set; }
	public Variable? Declaration { get; set; }
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
	private static VariableDescriptor GetVariableDescriptor(Variable variable, List<VariableNode> nodes)
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
	public static Dictionary<Variable, VariableDescriptor> GetVariableDescriptors(FunctionImplementation implementation, Node root)
	{
		var nodes = FindAll(root, i => i.Is(NodeType.VARIABLE)).Cast<VariableNode>().ToList();
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
	/// Removes all the assignments which do not have any effect on the execution of the specified function
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

		foreach (var iterator in redundants)
		{
			// Find all function calls inside the redundant write and replace the write with them
			var blocks = iterator.Node.FindTop(i => i.Is(NodeType.FUNCTION, NodeType.CALL, NodeType.INLINE));

			// If any of the calls in under a link node, select those link nodes
			blocks = blocks.Select(i => i.Parent != null && i.Parent.Is(NodeType.LINK) ? i.Parent : i).ToList();

			var inline = new InlineNode(iterator.Node.Position);

			// Declare the variable of the assignment if needed
			if (iterator.Declaration != null)
			{
				inline.Add(new DeclareNode(iterator.Declaration));
			}

			// Add all the blocks to the inline node
			blocks.ForEach(i => inline.Add(i));

			if (!inline.IsEmpty)
			{
				iterator.Node.Replace(inline);
			}
			else
			{
				iterator.Node.Remove();
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
	private static void Assign(VariableWrite write, bool recursive)
	{
		foreach (var read in write.Assignable)
		{
			// Find the root of the expression which contains the root and approximate the cost of the expression
			var root = Common.GetExpressionRoot(read);
			var before = Analysis.GetCost(root);

			// Clone the assignment value and find all the variable references
			var value = write.Value.Clone();
			read.Replace(value);

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
				// Because the assignment will not be reverted back, remove the read from the dependencies
				write.Dependencies.Remove(read);
			}
		}
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
	/// Looks for assignments of the specified variable which can be inlined
	/// </summary>
	private static void Assign(FunctionImplementation implementation, Node root)
	{
		var variables = implementation.Locals.Concat(implementation.Variables.Values).Distinct().ToArray();

		foreach (var variable in variables)
		{
			var descriptors = GetVariableDescriptors(implementation, root);
			var descriptor = descriptors[variable];

			var flow = new Flow(root);

			RegisterWriteDependencies(descriptors, flow);

			foreach (var write in descriptor.Writes)
			{
				// Collect all local variables that affect the value of the write. If any of these is edited, it means the value of the write changes
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
					if (descriptor.Writes.Except(write).SelectMany(i => i.Dependencies).Any(i => ReferenceEquals(i, read)))
					{
						continue;
					}

					var assign = true;

					var from = flow.Indices[write.Node];
					var to = flow.Indices[read];

					// If the read happens before the edit, which is possible in loops for example, it is not reliable to get all the nodes between the edit and the read
					if (to < from)
					{
						continue;
					}

					foreach (var node in flow.Nodes.GetRange(from, to - from).Where(i => i.Is(NodeType.VARIABLE)).Cast<VariableNode>())
					{
						if (dependencies.Contains(node.Variable) && Analyzer.IsEdited(node))
						{
							var editor = Analyzer.GetEditor(node);
							var position = flow.Indices[editor];

							if (position <= from || position >= to)
							{
								continue;
							}

							assign = false;
							break;
						}
					}

					if (!assign)
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

				write.Assignable.AddRange(assignable);
			}

			for (var i = 0; i < descriptor.Writes.Count; i++)
			{
				var write = descriptor.Writes[i];
				var recursive = write.Value.Find(i => i.Is(variable)) != null;

				Assign(write, recursive);
			}

			// Lastly, remove all the assignments which have no dependencies left
			for (var i = descriptor.Writes.Count - 1; i >= 0; i--)
			{
				var write = descriptor.Writes[i];

				if (write.Dependencies.Count != 0)
				{
					continue;
				}

				// If this statement is a declaration and the next write is not in the same scope, replace this statement with a declaration node
				// 1. Only the first write can be a declaration
				// 2. There must be at least two writes
				if (i == 0 && descriptor.Writes.Count > 1)
				{
					var a = write.Node.GetParentContext();
					var b = descriptor.Writes[1].Node.GetParentContext();

					if (a == b)
					{
						write.Node.Remove();
						continue;
					}

					write.Node.Replace(new DeclareNode(variable));
				}
				else
				{
					write.Node.Remove();
				}
			}

			Analysis.OptimizeAllExpressions(root);
		}
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
	private static Node AssignVariables(FunctionImplementation implementation, Node root)
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

			if (Analysis.IsMathematicalAnalysisEnabled) Assign(implementation, snapshot);

			// Try to optimize all comparisons found in the current snapshot
			if (Analysis.IsMathematicalAnalysisEnabled) Analysis.OptimizeComparisons(snapshot);
			
			// Try to unwrap conditional statements whose outcome have been resolved
			if (Analysis.IsUnwrapAnalysisEnabled) UnwrapmentAnalysis.UnwrapStatements(snapshot);

			// Removes all statements which are not reachable
			ReconstructionAnalysis.RemoveUnreachableStatements(snapshot);

			// Finally, try to simplify all expressions
			if (Analysis.IsMathematicalAnalysisEnabled) Analysis.OptimizeAllExpressions(snapshot);

			ReconstructionAnalysis.SubstituteInlineNodes(snapshot);

			// Calculate the complexity of the current snapshot
			var cost = Analysis.GetCost(snapshot);

			if (cost <= minimum_cost)
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
			if (variable.IsParameter || descriptor.Reads.Any() || descriptor.Writes.Any())
			{
				continue;
			}

			var context = variable.Context;

			if (context.Variables.ContainsKey(variable.Name))
			{
				context.Variables.Remove(variable.Name);
				root.FindAll(i => i.Is(NodeType.DECLARE)).Cast<DeclareNode>().Where(i => i.Variable == variable).ForEach(i => i.Remove());
				continue;
			}

			/// NOTE: If this happens, it should not break anything
			Console.WriteLine("Warning: Could not remove unused variable");
		}
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

		RemoveUnusedVariables(implementation, root);

		root = AssignVariables(implementation, root);

		if (!Assembler.IsDebuggingEnabled)
		{
			RemoveRedundantAssignments(implementation, root);
		}

		RemoveUnusedVariables(implementation, root);

		return root;
	}
}