public static class Analyzer
{
    public static bool IsEdited(VariableNode reference)
    {
        return reference.Parent is OperatorNode operation && operation.Operator.Type == OperatorType.ACTION || (reference.Parent?.Is(NodeType.INCREMENT_NODE) ?? false);
    }

    public static void AnalyzeVariableUsages(Context context)
    {
        foreach (var variable in context.Variables.Values)
        {
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

        foreach (var type in context.Types.Values)
        {
            AnalyzeVariableUsages(type);
        }

        foreach (var function in context.GetImplementedFunctions())
        {
            AnalyzeVariableUsages(function);
        }
    }

    public static void ConfigureStaticVariables(Context context)
    {
        foreach (var type in context.Types.Values)
        {
            foreach (var variable in type.Variables.Values)
            {
                if (Flag.Has(variable.Modifiers, AccessModifier.STATIC))
                {   
                    // Static variables should be treated like global variables
                    variable.Category = VariableCategory.GLOBAL;
                }
            }
            
            ConfigureStaticVariables(type);
        }
    }

    public static void Analyze(Context context)
    {
        AnalyzeVariableUsages(context);
        ConfigureStaticVariables(context);
    }
}