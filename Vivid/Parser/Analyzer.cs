using System;
using System.Linq;

public static class Analyzer
{
	/// <summary>
	/// Returns whether the specified is edited
	/// </summary>
	public static bool IsEdited(Node node)
	{
		var parent = node.FindParent(i => !i.Is(NodeType.CAST)) ?? throw new ApplicationException("Reference did not have a valid parent");

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
		return editor.GetLeftWhile(i => i.Is(NodeType.CAST)) ?? throw new ApplicationException("Edit did not contain destination");
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
		root.FindAll(n => n.Is(NodeType.VARIABLE))
			.Select(n => n.To<VariableNode>().Variable)
			.Distinct()
			.ForEach(v => { v.References.Clear(); v.Writes.Clear(); v.Reads.Clear(); });
	}

	private static void ResetVariableUsages(Context context)
	{
		foreach (var implementation in context.GetImplementedFunctions())
		{
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

	public static void FindUsages(Variable variable, Node root)
	{
		variable.References.Clear();
		variable.Writes.Clear();
		variable.Reads.Clear();

		foreach (var iterator in root.FindAll(n => n.Is(NodeType.VARIABLE)).Cast<VariableNode>().Where(i => i.Variable == variable))
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

	private static void AnalyzeVariableUsages(Node root)
	{
		foreach (var iterator in root.FindAll(n => n.Is(NodeType.VARIABLE)).Select(n => n.To<VariableNode>()))
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

		foreach (var implementation in context.GetImplementedFunctions())
		{
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

	public static void ApplyConstants(Context context)
	{
		var constants = context.Variables.Values.Where(i => i.IsConstant);

		foreach (var constant in constants)
		{
			if (constant.Writes.Count == 0)
			{
				throw Errors.Get(constant.Position, $"Value for the constant '{constant.Name}' is never assigned");
			}
			else if (constant.Writes.Count > 1)
			{
				throw Errors.Get(constant.Position, $"Value for the constant '{constant.Name}' is assigned twice or more");
			}

			var edit = constant.Writes.First().Parent;

			if (edit is OperatorNode assignment)
			{
				if (assignment.Right.Is(NodeType.NUMBER) || assignment.Right.Is(NodeType.STRING))
				{
					var value = (assignment.Right as ICloneable) ?? throw new NotImplementedException("Constant value did not support cloning");

					foreach (var reference in constant.Reads)
					{
						reference.Replace((Node)value.Clone());
					}
				}
				else
				{
					throw Errors.Get(constant.Position, $"Value assigned to constant '{constant.Name}' is not a constant");
				}
			}
			else
			{
				throw Errors.Get(constant.Position, $"Invalid value assignment for constant '{constant.Name}'");
			}
		}

		foreach (var subcontext in context.Subcontexts)
		{
			ApplyConstants(subcontext);
		}

		foreach (var type in context.Types.Values)
		{
			ApplyConstants(type);
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