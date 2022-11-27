using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

public enum AccessType
{
	WRITE,
	READ,
	UNKNOWN
}

public static class Analyzer
{
	/// <summary>
	/// Returns whether the specified is edited
	/// </summary>
	public static bool IsEdited(Node node)
	{
		var parent = node.FindParent(i => !i.Is(NodeType.CAST)) ?? throw new ApplicationException("Node did not have a valid parent");

		if (parent.Is(OperatorType.ASSIGNMENT))
		{
			return parent.Left == node || node.IsUnder(parent.Left);
		}

		return parent.Is(NodeType.INCREMENT, NodeType.DECREMENT);
	}

	/// <summary>
	/// Try to determine the type of access related to the specified node
	/// </summary>
	public static AccessType TryGetAccessType(Node node)
	{
		var parent = node.FindParent(i => !i.Is(NodeType.CAST));
		if (parent == null) return AccessType.UNKNOWN;

		if (parent.Is(OperatorType.ASSIGNMENT))
		{
			if (parent.Left == node || node.IsUnder(parent.Left)) return AccessType.WRITE;
			return AccessType.READ;
		}

		if (parent.Is(NodeType.INCREMENT, NodeType.DECREMENT)) return AccessType.WRITE;
		return AccessType.READ;
	}

	/// <summary>
	/// Returns the node which is the destination of the specified edit
	/// </summary>
	public static Node GetEdited(Node editor)
	{
		return editor.GetLeftWhile(i => i.Is(NodeType.CAST)) ?? throw new ApplicationException("Editor did not have a destination");
	}

	/// <summary>
	/// Returns the node which edits the specified node
	/// </summary>
	public static Node GetEditor(Node edited)
	{
		var editor = edited.FindParent(i => !i.Is(NodeType.CAST)) ?? throw new ApplicationException("Could not find the editor node");
		return (editor.Is(OperatorType.ASSIGNMENT) || editor.Is(NodeType.INCREMENT, NodeType.DECREMENT)) ? editor : throw new ApplicationException("Could not find the editor node");
	}

	/// <summary>
	/// Tries to return the node which edits the specified node.
	/// Returns null if the specified node is not edited.
	/// </summary>
	public static Node? TryGetEditor(Node node)
	{
		var editor = node.FindParent(i => !i.Is(NodeType.CAST));

		if (editor == null)
		{
			return null;
		}

		return (editor.Is(OperatorType.ASSIGNMENT) || editor.Is(NodeType.INCREMENT, NodeType.DECREMENT)) ? editor : null;
	}

	/// <summary>
	/// Tries to returns the source value which is assigned without any casting or filtering.
	/// Returns null if the specified editor is not an assignment operator.
	/// </summary>
	public static Node GetSource(Node node)
	{
		while (true)
		{
			// Do not return the cast node since it does not represent the source value
			if (node.Is(NodeType.CAST))
			{
				node = node.To<CastNode>().Object;
				continue;
			}

			// Do not return the following nodes since they do not represent the source value
			if (node.Is(NodeType.PARENTHESIS, NodeType.INLINE))
			{
				node = node.Right;
				continue;
			}

			break;
		}

		return node;
	}

	private static void ResetVariableUsages(Node root)
	{
		root.FindAll(NodeType.VARIABLE)
			.Select(i => i.To<VariableNode>().Variable)
			.Distinct()
			.ForEach(i => { i.Usages.Clear(); i.Writes.Clear(); i.Reads.Clear(); });
	}

	private static void ResetVariableUsages(Context context)
	{
		foreach (var implementation in Common.GetAllFunctionImplementations(context))
		{
			if (implementation.Node == null) continue;
			ResetVariableUsages(implementation.Node!);
		}

		foreach (var variable in context.Variables.Values)
		{
			variable.Usages.Clear();
			variable.Writes.Clear();
			variable.Reads.Clear();
		}

		foreach (var subcontext in context.Subcontexts)
		{
			ResetVariableUsages(subcontext);
		}

		foreach (var type in context.Types.Values)
		{
			ResetVariableUsages(type);
		}
	}

	public static void ResetVariableUsages(Node root, Context context)
	{
		ResetVariableUsages(root);
		ResetVariableUsages(context);
	}

	/// <summary>
	/// Finds all the usages of the specified variable in the specified node tree and in the specified context
	/// </summary>
	public static Task[] FindAllUsagesAsync(Variable variable, Node root, Context context)
	{
		FindUsages(variable, root);
		return Common.GetAllFunctionImplementations(context).Select(i => Task.Run(() => FindUsages(variable, i.Node!, false))).ToArray();
	}

	/// <summary>
	/// Loads all variable usages from the specified function
	/// </summary>
	public static void LoadVariableUsages(FunctionImplementation implementation)
	{
		// Reset all parameters and locals
		foreach (var variable in implementation.GetAllVariables())
		{
			variable.Usages.Clear();
			variable.Writes.Clear();
			variable.Reads.Clear();
		}

		var self = implementation.Self;

		if (self != null)
		{
			self.Usages.Clear();
			self.Writes.Clear();
			self.Reads.Clear();
		}

		var usages = implementation.Node!.FindAll(NodeType.VARIABLE);

		foreach (var usage in usages)
		{
			var variable = usage.To<VariableNode>().Variable;
			if (!variable.IsPredictable) continue;

			if (IsEdited(usage)) { variable.Writes.Add(usage); }
			else { variable.Reads.Add(usage); }

			variable.Usages.Add(usage);
		}
	}

	/// <summary>
	/// Finds all the usages of the specified variable in the specified node tree
	/// </summary>
	public static void FindUsages(Variable variable, Node root, bool clear = true)
	{
		if (clear)
		{
			variable.Usages.Clear();
			variable.Writes.Clear();
			variable.Reads.Clear();
		}

		foreach (var iterator in root.FindAll(NodeType.VARIABLE).Cast<VariableNode>().Where(i => i.Variable == variable))
		{
			variable.Usages.Add(iterator);

			if (IsEdited(iterator))
			{
				variable.Writes.Add(iterator);
			}
			else
			{
				variable.Reads.Add(iterator);
			}
		}
	}
	
	/// <summary>
	/// Iterates through the usages of the specified variable and adds them to 'write' and 'read' lists accordingly.
	/// Returns whether the usages were added or not. This function does not add the usages, if the access type of an usage can not be determined accurately.
	/// </summary>
	private static bool TryCategorizeUsages(Variable variable)
	{
		variable.Writes.Clear();
		variable.Reads.Clear();

		foreach (var usage in variable.Usages)
		{
			var access = TryGetAccessType(usage);

			// If the access type is unknown, accurate information about usages is not available, therefore we must abort
			if (access == AccessType.UNKNOWN)
			{
				variable.Writes.Clear();
				variable.Reads.Clear();
				return false;
			}

			if (access == AccessType.WRITE)
			{
				variable.Writes.Add(usage);
			}
			else
			{
				variable.Reads.Add(usage);
			}
		}

		return true;
	}

	private static void AnalyzeVariableUsages(Node root)
	{
		foreach (var iterator in root.FindAll(NodeType.VARIABLE).Cast<VariableNode>())
		{
			iterator.Variable.Usages.Add(iterator);

			if (IsEdited(iterator))
			{
				iterator.Variable.Writes.Add(iterator);
			}
			else
			{
				iterator.Variable.Reads.Add(iterator);
			}
		}
	}

	private static void AnalyzeVariableUsages(Context context)
	{
		// Update all constants in the current context
		foreach (var variable in context.Variables.Values)
		{
			if (variable.IsConstant && variable.Usages.Any() && !variable.Writes.Any() && !variable.Reads.Any())
			{
				foreach (var reference in variable.Usages)
				{
					if (IsEdited(reference.To<VariableNode>()))
					{
						variable.Writes.Add(reference);
					}
					else
					{
						variable.Reads.Add(reference);
					}
				}
			}
		}

		foreach (var implementation in Common.GetAllFunctionImplementations(context))
		{
			if (implementation.Node == null) continue;
			AnalyzeVariableUsages(implementation.Node!);
		}

		foreach (var subcontext in context.Subcontexts)
		{
			AnalyzeVariableUsages(subcontext);
		}

		foreach (var type in context.Types.Values)
		{
			AnalyzeVariableUsages(type);
		}
	}

	public static void AnalyzeVariableUsages(Node root, Context context)
	{
		AnalyzeVariableUsages(root);
		AnalyzeVariableUsages(context);
	}

	private static void ConfigureStaticVariables(Context context)
	{
		foreach (var type in context.Types.Values)
		{
			foreach (var variable in type.Variables.Values.Where(i => i.IsStatic))
			{
				// Static variables should be treated like global variables
				variable.Category = VariableCategory.GLOBAL;
			}

			ConfigureStaticVariables(type);
		}
	}

	/// <summary>
	/// Inserts the values of the constants in the specified into their usages
	/// </summary>
	public static void ApplyConstants(Context context)
	{
		foreach (var iterator in context.Variables)
		{
			var variable = iterator.Value;
			if (!variable.IsConstant) continue;

			// Try to categorize the usages of the constant
			// If no accurate information is available, the value of the constant can not be inlined
			if (!TryCategorizeUsages(variable)) continue;

			if (variable.Writes.Count == 0) throw Errors.Get(variable.Position, $"Value for the constant '{variable.Name}' is never assigned");
			if (variable.Writes.Count > 1) throw Errors.Get(variable.Position, $"Value for the constant '{variable.Name}' is assigned more than once");

			var value = EvaluateConstant(variable);
			if (value == null) throw Errors.Get(variable.Position, $"Could not evaluate a constant value for '{variable.Name}'");

			foreach (var usage in variable.Reads)
			{
				// If the parent of the constant is a link node, it needs to be replaced with the value of the constant
				var destination = usage;
				if (usage.Previous != null && usage.Parent != null && usage.Parent.Instance == NodeType.LINK) { destination = usage.Parent; }

				destination.Replace(value.Clone());
			}
		}

		foreach (var subcontext in context.Subcontexts) ApplyConstants(subcontext);
		foreach (var type in context.Types.Values) ApplyConstants(type);
	}

	/// <summary>
	/// Evaluates the value of the specified constant and returns it. If evaluation fails, none is returned.
	/// </summary>
	public static Node? EvaluateConstant(Variable variable, HashSet<Variable> trace)
	{
		// Ensure we do not enter into an infinite evaluation cycle
		if (trace.Contains(variable)) return null;
		trace.Add(variable);

		// Verify there is exactly one definition for the specified constant
		TryCategorizeUsages(variable);

		var writes = variable.Writes;
		if (writes.Count != 1) return null;

		var write = variable.Writes.First().Parent;
		if (write == null || !write.Is(Operators.ASSIGN)) return null;

		// Extract the definition for the constant
		var value = Analyzer.GetSource(write.Last!);

		// If the current value is a constant, we can just stop
		if (value.Is(NodeType.NUMBER, NodeType.STRING)) return write.Last!;

		// Find other constant from the extracted definition
		var dependencies = value.FindAll(NodeType.VARIABLE).Where(i => i.To<VariableNode>().Variable.IsConstant).ToList();

		if (value.Instance == NodeType.VARIABLE && value.To<VariableNode>().Variable.IsConstant)
		{
			dependencies = new List<Node> { value };
		}

		var evaluation = (Node?)null;

		// Evaluate the dependencies
		foreach (var dependency in dependencies)
		{
			// If the evaluation of the dependency fails, the whole evaluation fails as well
			evaluation = EvaluateConstant(dependency.To<VariableNode>().Variable, trace);
			if (evaluation == null) return null;

			// If the parent of the dependency is a link node, it needs to be replaced with the value of the dependency
			var destination = dependency;
			if (dependency.Previous != null && dependency.Parent != null && dependency.Parent.Instance == NodeType.LINK) { destination = dependency.Parent; }

			// Replace the dependency with its value
			destination.Replace(evaluation);
		}

		// Update the value, because it might have been replaced
		value = Analyzer.GetSource(write.Last!);

		// Since all of the dependencies were evaluated successfully, we can try evaluating the value of the specified constant
		evaluation = Analysis.GetSimplifiedValue(value);
		if (!evaluation.Is(NodeType.NUMBER, NodeType.STRING)) return null;

		value.Replace(evaluation);
		return write.Last!;
	}

	/// <summary>
	/// Evaluates the value of the specified constant and returns it. If evaluation fails, none is returned.
	/// </summary>
	public static Node? EvaluateConstant(Variable variable)
	{
		return EvaluateConstant(variable, new HashSet<Variable>());
	}

	/// <summary>
	/// Finds all the constant usages in the specified node tree and inserts the values of the constants into their usages
	/// </summary>
	public static void ApplyConstantsInto(Node root)
	{
		var usages = root.FindAll(NodeType.VARIABLE).Where(i => i.To<VariableNode>().Variable.IsConstant);

		foreach (var usage in usages)
		{
			var value = EvaluateConstant(usage.To<VariableNode>().Variable);
			if (value == null) continue;


			// If the parent of the constant is a link node, it needs to be replaced with the value of the constant
			var destination = usage;
			if (usage.Previous != null && usage.Parent != null && usage.Parent.Instance == NodeType.LINK) { destination = usage.Parent; }

			destination.Replace(value.Clone());
		}
	}

	public static void Analyze(Node root, Context context)
	{
		ResetVariableUsages(root, context);
		AnalyzeVariableUsages(root, context);
		ConfigureStaticVariables(context);
		ApplyConstants(context);
	}
}