using System.Collections.Generic;
using System.Linq;
using System;

public static class MemoryAccessAnalysis
{
	/// <summary>
	/// Replaces the specified repetition with the specified variable.
	/// Optionally loads the value of the repetition to the specified variable.
	/// </summary>
	private static void ReplaceRepetition(Node repetition, Variable variable, bool access = false)
	{
		if (Analyzer.IsEdited(repetition))
		{
			var editor = Analyzer.GetEditor(repetition);

			if (!editor.Is(Operators.ASSIGN)) throw new ApplicationException("Encountered a complex editor node");

			// Store the value of the assignment to the specified variable
			editor.Insert(new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(variable),
				editor.Right
			));

			// Store the value into the repetition
			editor.Replace(new OperatorNode(Operators.ASSIGN).SetOperands(
				repetition.Clone(),
				new VariableNode(variable)
			));

			return;
		}

		if (access)
		{
			var position = ReconstructionAnalysis.GetExpressionExtractPosition(repetition);

			// Store the value of the repetition to the specified variable
			position.Insert(new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(variable),
				repetition.Clone()
			));
		}

		repetition.Replace(new VariableNode(variable));
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
		return expression.FindAll(NodeType.VARIABLE).Cast<VariableNode>().Where(i => i.Variable.IsMember).Select(i => i.Variable).ToArray();
	}

	/// <summary>
	/// Tries to find identical memory reads and localizes them, therefore it optimizes memory access
	/// </summary>
	public static Node Unrepeat(Node node)
	{
		var before = Analysis.GetCost(node);
		var root = node.Clone();

		var links = root.FindTop(i => i.Is(NodeType.LINK, NodeType.OFFSET));

		while (true)
		{
			var flow = new Flow(root);

			if (!links.Any())
			{
				// Since there are no links left, it is time to choose whether to use the old node tree version or the new one
				return Analysis.GetCost(root) < before ? root : node;
			}

			var repetitions = new List<Node>();
			var first = links.First();

			// Collect all parts of the start node which can be edited
			var dependencies = GetEditables(first);
			var members = GetEditableMembers(first);

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
				if (first.Equals(other))
				{
					repetitions.Insert(0, other);
					continue;
				}
				
				// Analyze the inner links, some of those can match the currently inspected link 'start'
				var sublinks = other.FindAll(NodeType.LINK).Cast<LinkNode>();

				foreach (var sublink in sublinks)
				{
					if (!sublink.Equals(first)) continue;
					repetitions.Insert(0, sublink);
				}
			}

			links.RemoveAt(0); // Remove the first link 'start', since it has been processed

			if (!repetitions.Any())
			{
				// Find inner links inside the current one and process them now
				var inner = first.FindTop(i => i.Is(NodeType.LINK, NodeType.OFFSET));
				links.InsertRange(0, inner);
				continue;
			}

			repetitions.Insert(0, first);

			var accesses = new bool[repetitions.Count];
			accesses[0] = true;

			var start = first;

			// Skip the first repetition
			for (var i = 1; i < repetitions.Count; i++)
			{
				// Load the current repetition
				var repetition = repetitions[i];

				// All writes are memory accesses
				if (Analyzer.IsEdited(repetition))
				{
					accesses[i] = true;
				}

				if (accesses[i]) continue;

				// Find all edits between the start and the repetition
				var edits = flow.FindBetween(start, repetition, i => i.Is(OperatorType.ACTION) || i.Is(NodeType.INCREMENT, NodeType.DECREMENT));
				
				// If any of the edits contain a destination which matches any of the dependencies, a store is required
				foreach (var edit in edits)
				{
					var edited = Analyzer.GetEdited(edit);

					// If the edited is one of the repetitions, no need to worry about that
					if (repetitions.Any(i => ReferenceEquals(i, edited))) continue;
					
					// If the dependencies does not contain the edited node, loading might not be necessary
					if (!dependencies.Contains(edited))
					{
						// If the current edit writes to any of the critical member variables, it may not be safe to use repetition without loading
						var is_member_variable_edited = edited.Is(NodeType.LINK) && edited.Right.Is(NodeType.VARIABLE);
						
						if (!is_member_variable_edited || !members.Contains(edited.Right.To<VariableNode>().Variable))
						{
							// None of the edits must access raw memory, since they can edit the repetitions
							if (!edited.Is(NodeType.OFFSET) && edited.Find(NodeType.OFFSET) == null) continue;
						}
					}

					start = repetition;
					accesses[i] = true;
					break;
				}

				if (accesses[i]) continue;

				// If there are function calls between the start and the repetition, the function calls could edit the repetition, so a store is required
				if (flow.Between(start, repetition, i => i.Is(NodeType.FUNCTION, NodeType.CALL)))
				{
					start = repetition;
					accesses[i] = true;
					continue;
				}

				// Access the memory if the current repetition can be reached without executing any of the previous loads
				var loads = new List<Node>();

				for (var j = 0; j < i; j++)
				{
					// Skip the repetition if it does not access the memory
					if (!accesses[j]) continue;
					
					var load = repetitions[j];

					if (Analyzer.IsEdited(load))
					{
						// Use the editor instead of the edited
						load = Analyzer.GetEditor(load);
					}

					loads.Add(load);
				}

				var obstacles = loads.Select(i => flow.Indices[i]).ToArray();
				var positions = new List<int> { flow.Indices[repetition] };

				var result = flow.GetExecutablePositions(0, obstacles, positions, new SortedSet<int>());

				if (result == null || result.Any())
				{
					start = repetition;
					accesses[i] = true;
				}
			}

			start = first;

			// If every repetition needs to access the memory, then these repetitions can not be optimized
			if (!accesses.Contains(false))
			{
				// Find inner links inside the current one and process them now
				var inner = first.FindTop(i => i.Is(NodeType.LINK, NodeType.OFFSET));
				links.InsertRange(0, inner);
				continue;
			}

			/// NOTE: If a scope node does not have a parent, it must be the root scope
			var context = first.FindParent(i => i.Is(NodeType.SCOPE) && i.Parent == null)!.To<ScopeNode>().Context;
			var variable = context.DeclareHidden(first.GetType());

			// Initialize the variable
			var scope = ReconstructionAnalysis.GetSharedScope(repetitions.Concat(new[] { first }).ToArray());
			if (scope == null) throw new ApplicationException("Links did not have a shared scope");

			// Since the repetitions are ordered find the insert position using the first repetition and the shared scope
			ReconstructionAnalysis.GetInsertPosition(first, scope).Insert(new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(variable),
				new UndefinedNode(variable.Type!, variable.GetRegisterFormat())
			));

			// Remove all the repetitions from the link list since they are about to be modified
			repetitions.ForEach(i => links.Remove(i));

			for (var i = 0; i < accesses.Length; i++)
			{
				var repetition = repetitions[i];
				var access = accesses[i];

				ReplaceRepetition(repetition, variable, access);

				if (!access) continue;

				start = repetition;
			}
		}
	}
}