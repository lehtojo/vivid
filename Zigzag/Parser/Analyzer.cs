using System.Linq;
using System;

public static class Analyzer
{
	public static bool IsEdited(VariableNode reference)
	{
		return reference.Parent is OperatorNode operation && operation.Operator.Type == OperatorType.ACTION && operation.Left == reference || 
			(reference.Parent?.Is(NodeType.INCREMENT_NODE) ?? false);
	}

	private static void AnalyzeVariableUsages(Context context)
	{
		foreach (var variable in context.Variables.Values)
		{
			variable.Edits.Clear();
			variable.Reads.Clear();

			foreach (var reference in variable.References)
			{		
				if (IsEdited((VariableNode)reference))
				{
					variable.Edits.Add(reference);
				}
				else
				{
					variable.Reads.Add(reference);
				}
			}
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
				if (assignment.Right.Is(NodeType.NUMBER_NODE) || assignment.Right.Is(NodeType.STRING_NODE))
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

			// Now all the readonly references are replaced with the constant's value
			constant.Reads.Clear();
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

	public static void Analyze(Context context)
	{
		AnalyzeVariableUsages(context);
		ConfigureStaticVariables(context);
		ApplyConstants(context);
	}
}