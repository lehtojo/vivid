using System;
using System.Linq;
using System.Threading.Tasks;

public static class Analyzer
{
	/// <summary>
	/// Returns whether the specified is edited
	/// </summary>
	public static bool IsEdited(Node node)
	{
		var parent = node.FindParent(i => !i.Is(NodeType.CAST)) ?? throw new ApplicationException("Node did not have a valid parent");

		if (parent.Is(OperatorType.ACTION))
		{
			return parent.Left == node || node.IsUnder(parent.Left);
		}

		return parent.Is(NodeType.INCREMENT, NodeType.DECREMENT);
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
		return (editor.Is(OperatorType.ACTION) || editor.Is(NodeType.INCREMENT, NodeType.DECREMENT)) ? editor : throw new ApplicationException("Could not find the editor node");
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

		return (editor.Is(OperatorType.ACTION) || editor.Is(NodeType.INCREMENT, NodeType.DECREMENT)) ? editor : null;
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
			if (node.Is(NodeType.CONTENT, NodeType.INLINE))
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
			.ForEach(i => { i.References.Clear(); i.Writes.Clear(); i.Reads.Clear(); });
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
			variable.References.Clear();
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
	/// Finds all the usages of the specified variable in the specified node tree
	/// </summary>
	public static void FindUsages(Variable variable, Node root, bool clear = true)
	{
		if (clear)
		{
			variable.References.Clear();
			variable.Writes.Clear();
			variable.Reads.Clear();
		}

		foreach (var iterator in root.FindAll(NodeType.VARIABLE).Cast<VariableNode>().Where(i => i.Variable == variable))
		{
			variable.References.Add(iterator);

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
	/// Iterates through the usages of the specified variable and adds them to 'write' and 'read' lists accordingly
	/// </summary>
	private static void CategorizeUsages(Variable variable)
	{
		variable.Writes.Clear();
		variable.Reads.Clear();

		foreach (var usage in variable.References)
		{
			if (IsEdited(usage))
			{
				variable.Writes.Add(usage);
			}
			else
			{
				variable.Reads.Add(usage);
			}
		}
	}

	private static void AnalyzeVariableUsages(Node root)
	{
		foreach (var iterator in root.FindAll(NodeType.VARIABLE).Cast<VariableNode>())
		{
			iterator.Variable.References.Add(iterator);

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
			if (variable.IsConstant && variable.References.Any() && !variable.Writes.Any() && !variable.Reads.Any())
			{
				foreach (var reference in variable.References)
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
		var constants = context.Variables.Values.Where(i => i.IsConstant);

		foreach (var constant in constants)
		{
			if (constant.Writes.Count == 0) throw Errors.Get(constant.Position, $"Value for the constant '{constant.Name}' is never assigned");
			if (constant.Writes.Count > 1) throw Errors.Get(constant.Position, $"Value for the constant '{constant.Name}' is assigned more than once");

			var write = constant.Writes.First().Parent;

			if (write == null || !write.Is(Operators.ASSIGN)) throw Errors.Get(constant.Position, $"Invalid assignment for constant '{constant.Name}'");
			
			var value = Analyzer.GetSource(write.Right);
			if (!value.Is(NodeType.NUMBER) && !value.Is(NodeType.STRING)) throw Errors.Get(constant.Position, $"Value assigned to constant '{constant.Name}' is not a constant");

			foreach (var usage in constant.Reads)
			{
				var destination = usage;

				// If the parent of the constant is a link node, it needs to be replaced with the value of the constant
				// Example:
				// namespace A { C = 0 }
				// print(A.C) => print(0)
				if (usage.Parent != null && usage.Parent.Is(NodeType.LINK))
				{
					destination = usage.Parent;
				}

				destination.Replace(write.Right.Clone());
			}
		}

		foreach (var subcontext in context.Subcontexts) ApplyConstants(subcontext);
		foreach (var type in context.Types.Values) ApplyConstants(type);
	}

	/// <summary>
	/// Finds all the constant usages in the specified node tree and inserts the values of the constants into their usages
	/// </summary>
	public static void ApplyConstants(Node root)
	{
		var constants = root.FindAll(NodeType.VARIABLE).Cast<VariableNode>().Where(i => i.Variable.IsConstant).ToArray();

		foreach (var usage in constants)
		{
			var constant = usage.Variable;

			CategorizeUsages(constant);

			if (constant.Writes.Count == 0) throw Errors.Get(constant.Position, $"Value for the constant '{constant.Name}' is never assigned");
			if (constant.Writes.Count > 1) throw Errors.Get(constant.Position, $"Value for the constant '{constant.Name}' is assigned more than once");

			var write = constant.Writes.First().Parent;

			if (write == null || !write.Is(Operators.ASSIGN)) throw Errors.Get(constant.Position, $"Invalid assignment for constant '{constant.Name}'");
			
			var value = Analyzer.GetSource(write.Right);
			if (!value.Is(NodeType.NUMBER) && !value.Is(NodeType.STRING)) throw Errors.Get(constant.Position, $"Value assigned to constant '{constant.Name}' is not a constant");
			
			var destination = (Node)usage;

			// If the parent of the constant is a link node, it needs to be replaced with the value of the constant
			// Example:
			// namespace A { C = 0 }
			// print(A.C) => print(0)
			if (usage.Parent != null && usage.Parent.Is(NodeType.LINK))
			{
				destination = usage.Parent;
			}

			destination.Replace(write.Right.Clone());
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