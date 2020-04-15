using System.Collections.Generic;
using System.Linq;

public class VariableUsageInfo
{
    public Variable Variable;
    public Result? Reference;
    public int Usages;

    public VariableUsageInfo(Variable variable, int usages)
    {
        Variable = variable;
        Usages = usages;
    }
}

public static class Loops
{
    private static Dictionary<Variable, int> GetVariableUsageCount(Node parent)
    {
        var variables = new Dictionary<Variable, int>();
        var iterator = parent.First;

        while (iterator != null)
        {
            if (iterator is VariableNode node && node.Variable.IsPredictable)
            {
                variables[node.Variable] = variables.GetValueOrDefault(node.Variable, 0) + 1;
            }
            else
            {
                foreach (var usage in GetVariableUsageCount(iterator))
                {
                    variables[usage.Key] = variables.GetValueOrDefault(usage.Key, 0) + usage.Value;
                }
            }

            iterator = iterator.Next;
        }

        return variables;
    }

    private static List<VariableUsageInfo> GetAllVariableUsages(LoopNode node)
    {
        // Get all variables in the loop and their number of usages
        var variables = GetVariableUsageCount(node).Select(i => new VariableUsageInfo(i.Key, i.Value)).ToList();

        // Sort the variables based on their number of usages (most used variables first)
        variables.Sort((a, b) => -a.Usages.CompareTo(b.Usages));

        return variables;
    }

    private static Result BuildForeverLoop(Unit unit, LoopNode node)
    {
        var start = unit.GetNextLabel();

        // Initialize the loop
        PrepareRelevantVariables(unit, node);

        // Build the loop body
        var result = BuildLoopBody(unit, node, new LabelInstruction(unit, start));

        // Jump to the start of the loop
        unit.Append(new JumpInstruction(unit, start));

        return result;
    }

    private static void PrepareRelevantVariables(Unit unit, LoopNode node)
    {
        var variables = GetAllVariableUsages(node);
        unit.Append(new CacheVariablesInstruction(unit, variables));
    }

    private static IEnumerable<Variable> GetAllNonLocalVariables(Context local_context, Node body)
    {
        return body.FindAll(n => n.Is(NodeType.VARIABLE_NODE))
                    .Select(n => ((VariableNode)n).Variable)
                    .Where(v => v.IsPredictable && v.Context != local_context)
                    .Distinct();
    }

    private static Result BuildLoopBody(Unit unit, LoopNode loop, LabelInstruction start)
    {
        var non_local_variables = GetAllNonLocalVariables(loop.Context, loop);

        var state = unit.GetState(unit.Position);
        var result = (Result?)null;

        using (var scope = new Scope(unit))
        {
            // Append the label where the loop will start
            unit.Append(start);

            var merge = new MergeScopeInstruction(unit, non_local_variables);

            // Build the loop body
            result = Builders.Build(unit, loop.Body);

            if (!loop.IsForeverLoop)
            {
                // Build the loop action
                Builders.Build(unit, loop.Action);
            }

            // Restore the state after the body
            merge.Append();

            // Restore the state after the body
            unit.Append(merge);
        }

        unit.Set(state);

        return result;
    }

    public static Result Build(Unit unit, LoopNode node)
    {
        if (node.IsForeverLoop)
        {
            return BuildForeverLoop(unit, node);
        }

        // Create the start and end label of the loop
        var start = unit.GetNextLabel();
        var end = unit.GetNextLabel();

        // Initialize the loop
        PrepareRelevantVariables(unit, node);
    
        if (node.Condition is OperatorNode start_condition)
        {
            var left = References.Get(unit, start_condition.Left);
            var right = References.Get(unit, start_condition.Right);

            // Compare the two operands
            var comparison = new CompareInstruction(unit, left, right).Execute();

            // Jump to the end based on the comparison
            unit.Append(new JumpInstruction(unit, comparison, (ComparisonOperator)start_condition.Operator, true, end));
        }

        // Build the loop body
        var result = BuildLoopBody(unit, node, new LabelInstruction(unit, start));

        if (node.Condition is OperatorNode end_condition)
        {
            var left = References.Get(unit, end_condition.Left);
            var right = References.Get(unit, end_condition.Right);

            // Compare the two operands
            var comparison = new CompareInstruction(unit, left, right).Execute();

            // Jump to the start based on the comparison
            unit.Append(new JumpInstruction(unit, comparison, (ComparisonOperator)end_condition.Operator, false, start));
        }

        // Append the label where the loop ends
        unit.Append(new LabelInstruction(unit, end));

        return result;
    }
}