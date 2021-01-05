using System.Collections.Generic;
using System.Linq;
using System;

public class VariableWrite
{
	public Node Node { get; set; }
	public List<Node> Dependencies { get; } = new List<Node>();
	public Node Value => Node.Last!;

	public VariableWrite(Node node)
	{
		Node = node;
	}
}

public class VariableEqualityComparer : EqualityComparer<Variable>
{
	public override bool Equals(Variable? a, Variable? b)
	{
		return a == b;
	}

	public override int GetHashCode(Variable? a) => 0;
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

public static class GeneralAnalysis
{
	private static List<Node> FindAll(Node root, Predicate<Node> filter)
	{
		var nodes = new List<Node>();
		var iterator = (IEnumerable<Node>)root;

		if (root is OperatorNode x && x.Operator.Type == OperatorType.ACTION)
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

	private static List<Node> GetReferences(Node root, Variable variable)
	{
		return FindAll(root, n => n.Is(NodeType.VARIABLE)).Where(v => v.To<VariableNode>().Variable == variable).ToList();
	}

	private static List<Node> GetWrites(List<Node> references)
	{
		return references.Where(v => Analyzer.IsEdited(v.To<VariableNode>())).ToList();
	}

	private static VariableDescriptor GetVariableDescriptor(Node root, Variable variable)
	{
		var reads = GetReferences(root, variable);
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

	private static Dictionary<Variable, VariableDescriptor> GetVariableDescriptors(FunctionImplementation implementation, Node root)
	{
		var variables = implementation.Locals.Concat(implementation.Parameters);
		return new Dictionary<Variable, VariableDescriptor>(variables.Select(v => new KeyValuePair<Variable, VariableDescriptor>(v, GetVariableDescriptor(root, v))), new VariableEqualityComparer());
	}

	private static void RegisterWriteDependencies(Dictionary<Variable, VariableDescriptor> descriptors, Flow flow)
	{
		foreach (var descriptor in descriptors)
		{
			foreach (var write in descriptor.Value.Writes)
			{
				foreach (var read in descriptor.Value.Reads)
				{
					var obstacles = descriptor.Value.Writes.Where(i => i != write).Select(i => i.Node).ToArray();

					if (flow.IsReachableWithoutExecuting(read, write.Node, obstacles))
					{
						write.Dependencies.Add(read);
					}
				}
			}
		}
	}

	private struct RedundantAssignment
	{
		public Node Node;
		public Variable? Declaration;
	}

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

	private static void RemoveRedundantAssignments(FunctionImplementation implementation, Node root)
	{
		var descriptors = GetVariableDescriptors(implementation, root);
		var flow = new Flow(root);
		var redundants = new List<RedundantAssignment>();

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

	private static Variable[] GetWriteDependencies(Node write)
	{
		var value = write.Last!;

		return (value is VariableNode node && node.Variable.IsPredictable) ? new[] { node.Variable } : value.FindAll(i => i is VariableNode x && x.Variable.IsPredictable && !x.Variable.IsSelfPointer).Select(i => i.To<VariableNode>().Variable).ToArray();
	}

	private static Node[] GetAssignable(VariableWrite write, Node[] reads, Flow flow)
	{
		// Ensure the write is always executed before the current read
		return reads.Where(i => flow.IsAlwaysExecutedBefore(write.Node, i)).ToArray();
	}

	private static void Assign(VariableWrite write, Node[] reads)
	{
		var value = write.Node.Last!;

		foreach (var read in reads)
		{
			read.Replace(value.Clone());
				
			if (!write.Dependencies.Remove(read))
			{
				throw new ApplicationException("Variable write dependency was not registered");
			}
		}
	}

	private static void Assign(FunctionImplementation implementation, Node root, Variable variable)
	{
		Start:

		var descriptors = GetVariableDescriptors(implementation, root);
		var descriptor = descriptors[variable];

		var flow = new Flow(root);

		RegisterWriteDependencies(descriptors, flow);

		for (var i = 0; i < descriptor.Writes.Count;)
		{
			var write = descriptor.Writes[i];
			var next = i + 1 == descriptor.Writes.Count ? null : descriptor.Writes[i + 1].Node;

			// If the value of the write contains calls, it should not be assigned
			if (write.Node.Find(i => !i.Is(NodeType.VARIABLE, NodeType.NUMBER, NodeType.OPERATOR)) != null)
			{
				i++;
				continue;
			}

			// Collect all local variabes that affect the value of the write. If any of these is edited, it means the value of the write changes
			var dependencies = GetWriteDependencies(write.Node);
			var obstacles = dependencies.SelectMany(i => descriptors[i].Writes).Select(i => i.Node).ToArray();

			// Collect all reads that are between the current edit and the next edit
			var reads = descriptor.Reads.Where(read => next != null ? flow.IsBetween(read, write.Node, next) : flow.IsAfter(read, write.Node)).ToArray();

			// All of these reads must be executed before any of the dependencies are edited
			reads = reads.Where(read => !obstacles.Any(obstacle => flow.IsBetween(obstacle, write.Node, read))).ToArray();

			var assignable = GetAssignable(write, reads, flow);

			// If the value contains the destination variable and it can not be assigned to all its usages or it is repeated, it should not be assigned
			var recursive = write.Value.Find(i => i is VariableNode x && x.Variable == variable) != null;

			if (recursive && (assignable.Length != reads.Length || flow.IsRepeated(write.Node)))
			{
				i++;
				continue;
			}

			// Assign the value of the write to its usages
			Assign(write, assignable);

			// If there are no dependencies left for the current write, it can be removed
			if (!write.Dependencies.Any())
			{
				RemoveStatement(write.Node);
				goto Start;
			}

			// If the value of the current write was assigned to at least one location, start over
			if (assignable.Any())
			{
				goto Start;
			}

			i++;
		}
	}

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

			var variables = implementation.Locals.Concat(implementation.Parameters);

			Start:

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

			// NOTE: This is a repetition, but it is needed since some function don't have variables
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

	public static Node Optimize(FunctionImplementation implementation, Node root)
	{
		if (!Assembler.IsDebuggingEnabled)
		{
			RemoveRedundantAssignments(implementation, root);
		}
		
		return AssignVariables(implementation, root);
	}
}