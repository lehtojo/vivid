using System.Linq;
using System;

public static class Analyzer
{
	public static bool IsEdited(Node reference)
	{
		var parent = reference.FindParent(i => !i.Is(NodeType.CAST)) ?? throw new ApplicationException("Reference did not have a valid parent");

		if (parent is OperatorNode operation && operation.Operator.Type == OperatorType.ACTION)
		{
			return operation.Left == reference || reference.IsUnder(operation.Left);
		}

		return parent.Is(NodeType.INCREMENT, NodeType.DECREMENT);
	}

	public static Node GetEdited(Node edit)
	{
		return edit.GetLeftWhile(i => i.Is(NodeType.CAST)) ?? throw new ApplicationException("Edit did not contain destination");
	}

	public static Node GetEditor(Node reference)
	{
		var editor = reference.FindParent(i => !i.Is(NodeType.CAST)) ?? throw new ApplicationException("Could not find the editor node");
		return (editor.Is(OperatorType.ACTION) || editor.Is(NodeType.INCREMENT, NodeType.DECREMENT)) ? editor : throw new ApplicationException("Could not find the editor node");
	}

	public static Node? TryGetEditor(Node reference)
	{
		var editor = reference.FindParent(i => !i.Is(NodeType.CAST));

		if (editor == null)
		{
			return null;
		}

		return (editor.Is(OperatorType.ACTION) || editor.Is(NodeType.INCREMENT, NodeType.DECREMENT)) ? editor : null;
	}

	private static void ResetVariableUsages(Node root)
	{
		root.FindAll(n => n.Is(NodeType.VARIABLE))
			.Select(n => n.To<VariableNode>().Variable)
			.Distinct()
			.ForEach(v => { v.References.Clear(); v.Edits.Clear(); v.Reads.Clear(); });
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
			variable.Edits.Clear();
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

	private static void AnalyzeVariableUsages(Node root)
	{
		foreach (var iterator in root.FindAll(n => n.Is(NodeType.VARIABLE)).Select(n => n.To<VariableNode>()))
		{
			iterator.Variable.References.Add(iterator);

			if (IsEdited(iterator))
			{
				iterator.Variable.Edits.Add(iterator);
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
			if (variable.IsConstant && variable.References.Any() && !variable.Edits.Any() && !variable.Reads.Any())
			{
				foreach (var reference in variable.References)
				{
					if (IsEdited(reference.To<VariableNode>()))
					{
						variable.Edits.Add(reference);
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
			foreach (var variable in type.Variables.Values
				.Where(variable => Flag.Has(variable.Modifiers, AccessModifier.STATIC)))
			{
				// Static variables should be treated like global variables
				variable.Category = VariableCategory.GLOBAL;
			}

			ConfigureStaticVariables(type);
		}
	}

	private static void ApplyConstants(Context context)
	{
		var constants = context.Variables.Values.Where(v => v.IsConstant);

		foreach (var constant in constants)
		{
			if (constant.Edits.Count == 0)
			{
				throw new Exception($"Value for the constant '{constant.Name}' is never assigned");
			}
			else if (constant.Edits.Count > 1)
			{
				throw new Exception($"Value for the constant '{constant.Name}' is assigned twice or more");
			}

			var edit = constant.Edits.First().Parent;

			if (edit is OperatorNode assignment)
			{
				if (assignment.Right.Is(NodeType.NUMBER) || assignment.Right.Is(NodeType.STRING))
				{
					var value = (assignment.Right as ICloneable) ?? throw new NotImplementedException("Constant value didn't support cloning");

					foreach (var reference in constant.Reads)
					{
						reference.Replace((Node)value.Clone());
					}
				}
				else
				{
					throw new Exception($"Value assigned to constant '{constant.Name}' is not a constant");
				}
			}
			else
			{
				throw new Exception($"Invalid value assignment for constant '{constant.Name}'");
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