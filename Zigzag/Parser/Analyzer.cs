public static class Analyzer
{
    public static bool IsEdited(VariableNode reference)
    {
        return reference.Parent is OperatorNode operation && operation.Operator.Type == OperatorType.ACTION;
    }

    public static void Analyze(Context context)
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
    }
}