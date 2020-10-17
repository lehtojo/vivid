using System.Linq;
using System;

public static class Analyzer
{
	public static bool IsEdited(VariableNode reference)
	{
		return reference.Parent is OperatorNode operation && operation.Operator.Type == OperatorType.ACTION && operation.Left == reference ||
			(reference.Parent?.Is(NodeType.INCREMENT, NodeType.DECREMENT) ?? false);
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

		foreach (var subcontext in context.Subcontexts)
		{
			ResetVariableUsages(subcontext);
		}

		foreach (var type in context.Types.Values)
		{
			ResetVariableUsages(type);
		}
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

	public static void Analyze(Context context, Node root)
	{
		ResetVariableUsages(root);
		ResetVariableUsages(context);
		AnalyzeVariableUsages(root);
		AnalyzeVariableUsages(context);
		ConfigureStaticVariables(context);
		ApplyConstants(context);
	}
}