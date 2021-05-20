using System.Collections.Generic;
using System.Linq;
using System;

public static class MemoryAccessAnalysis
{
	/// <summary>
	/// Replaces the specified repetition with the specified variable.
	/// Optionally loads the value of the repetition to the specified variable.
	/// </summary>
	private static Node ReplaceRepetition(Node repetition, Variable variable, bool store = false)
	{
		if (Analyzer.IsEdited(repetition))
		{
			var edit = Analyzer.GetEditor(repetition);

			if (edit.Is(Operators.ASSIGN))
			{
				// Store the value of the assignment to the specified variable
				var initialization = new OperatorNode(Operators.ASSIGN).SetOperands(
					new VariableNode(variable),
					edit.Right
				);

				var inline = new InlineNode(edit.Position) { initialization };

				edit.Replace(inline);

				// Store the value into the repetition
				inline.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
					repetition.Clone(),
					new VariableNode(variable)
				));

				// Add a result to the inline node if the return value of the edit is used
				if (ReconstructionAnalysis.IsValueUsed(edit))
				{
					inline.Add(new VariableNode(variable));
				}

				return inline;
			}

			// Increments, decrements and special assignment operators should be unwrapped before unrepetition
			throw new ApplicationException("Repetition was edited by increment, decrement or special assignment operator which should no happen");
		}

		if (store)
		{
			// Store the value of the repetition to the specified variable
			var initialization = new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(variable),
				repetition.Clone()
			);

			// Replace the repetition with the initialization
			var inline = new InlineNode(repetition.Position) { initialization, new VariableNode(variable) };
			repetition.Replace(inline);

			return inline;
		}

		var result = new VariableNode(variable);
		repetition.Replace(result);

		return result;
	}

	/// <summary>
	/// Returns the first elements which satisfy the specified condition, while preferring the right side of assignment operators
	/// </summary>
	private static List<Node> FindTop(Node root, Predicate<Node> filter)
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
			else
			{
				nodes.AddRange(FindTop(i, filter));
			}
		}

		return nodes;
	}

	/// <summary>
	/// Finds parts of the specified expression which can be edited.
	/// If one of the returned nodes is edited, the value of the expression might change.
	/// </summary>
	private static List<Node> GetEditables(Node expression)
	{
		var result = new List<Node>();

		foreach (var iterator in expression)
		{
			if (iterator.Is(NodeType.LINK))
			{
				result.Add(iterator);
				result.AddRange(GetEditables(iterator));
				continue;
			}
			
			var editables = GetEditables(iterator);

			if (editables.Any())
			{
				result.AddRange(editables);
				result.AddRange(editables.SelectMany(i => GetEditables(i)));
			}
			else
			{
				result.Add(expression.GetBottomLeft()!);
			}
		}

		return result;
	}

	/// <summary>
	/// Returns all member variables which are accessed in the specified expression
	/// </summary>
	private static Variable[] GetEditableMembers(Node expression)
	{
		return expression.FindAll(i => i.Is(NodeType.VARIABLE)).Cast<VariableNode>().Where(i => i.Variable.IsMember).Select(i => i.Variable).ToArray();
	}

	/// <summary>
	/// Tries to find identical memory reads and localizes them, therefore it optimizes memory access
	/// </summary>
	public static Node Unrepeat(Node node)
	{
		var before = Analysis.GetCost(node);
		var root = node.Clone();

		var links = FindTop(root, i => i.Is(NodeType.LINK, NodeType.OFFSET));

		while (true)
		{
			var flow = new Flow(root);

			if (!links.Any())
			{
				// Since there are no links left, it is time to choose whether to use the old node tree version or the new one
				return Analysis.GetCost(root) < before ? root : node;
			}

			var repetitions = new List<Node>();
			var start = links.First();

			// Collect all parts of the start node which can be edited
			var dependencies = GetEditables(start);
			var members = GetEditableMembers(start);

			// Find all the other usages of the link 'start'
			for (var i = links.Count - 1; i >= 1; i--)
			{
				var other = links[i];

				// If the current link contains nodes which should not be moved, skip it
				if (other.Find(i => !i.Is(NodeType.VARIABLE, NodeType.TYPE, NodeType.LINK, NodeType.OFFSET, NodeType.CONTENT, NodeType.NUMBER)) != null)
				{
					links.RemoveAt(i);
					continue;
				}

				// Add the link 'other' if it completely matches the link 'start'
				if (start.Equals(other))
				{
					repetitions.Insert(0, other);
					continue;
				}
				
				// Analyze the inner links, some of those can match the currently inspected link 'start'
				var sublinks = other.FindAll(i => i.Is(NodeType.LINK)).Cast<LinkNode>();

				foreach (var sublink in sublinks)
				{
					if (!sublink.Equals(start)) continue;
					repetitions.Insert(0, sublink);
				}
			}

			links.RemoveAt(0); // Remove the first link 'start', since it has been processed

			if (!repetitions.Any())
			{
				// Find inner links inside the current one and process them now
				var inner = FindTop(start, i => i.Is(NodeType.LINK, NodeType.OFFSET));
				links.InsertRange(0, inner);
				continue;
			}

			// Remove all the repetitions from the link list since they are about to be modified
			repetitions.ForEach(i => links.Remove(i));

			/// NOTE: If a scope node does not have a parent, it must be the root scope
			var context = start.FindParent(i => i.Is(NodeType.SCOPE) && i.Parent == null)!.To<ScopeNode>().Context;
			var variable = context.DeclareHidden(start.GetType());

			// Initialize the variable
			var scope = ReconstructionAnalysis.GetSharedScope(repetitions.Concat(new[] { start }).ToArray());
			if (scope == null) throw new ApplicationException("Links did not have a shared scope");

			// Since the repetitions are ordered find the insert position using the first repetition and the shared scope
			ReconstructionAnalysis.GetInsertPosition(start, scope).Insert(new DeclareNode(variable));

			ReplaceRepetition(start, variable, true);

			foreach (var repetition in repetitions)
			{
				var store = false;

				// Find all edits between the start and the repetition
				var edits = flow.FindBetween(start, repetition, i => i.Is(OperatorType.ACTION) || i.Is(NodeType.INCREMENT, NodeType.DECREMENT));

				// If any of the edits contain a destination which matches any of the dependencies, a store is required
				foreach (var edit in edits)
				{
					var edited = Analyzer.GetEdited(edit);

					if (edited == start) continue;
					
					// If the dependencies does not contain the edited node, loading might not be necessary
					if (!dependencies.Contains(edited))
					{
						// If the current edit writes to any of the critical member variables, it may not be safe to use repetition without loading
						var is_member_variable_edited = edited.Is(NodeType.LINK) && edited.Right.Is(NodeType.VARIABLE);
						
						if (!is_member_variable_edited || !members.Contains(edited.Right.To<VariableNode>().Variable))
						{
							// None of the edits must access raw memory, since they can edit the repetitions
							if (!edited.Is(NodeType.OFFSET) && edited.Find(i => i.Is(NodeType.OFFSET)) == null) continue;
						}
					}

					start = repetition;
					store = true;
					break;
				}

				// 1. If there are function calls between the start and the repetition, the function calls could edit the repetition, so a store is required
				// 2. If the start is not always executed before the repetition, a store is needed
				if (flow.Between(start, repetition, i => i.Is(NodeType.FUNCTION, NodeType.CALL)))
				{
					start = repetition;
					store = true;
				}
				else if (!flow.IsExecutedBefore(start, repetition))
				{
					start = repetition;
					store = true;
				}

				ReplaceRepetition(repetition, variable, store);
			}
		}
	}
}