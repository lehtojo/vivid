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
    private static Dictionary<Variable, int> GetNonLocalVariableUsageCount(Node parent, params Context[] local_contexts)
    {
        var variables = new Dictionary<Variable, int>();
        var iterator = parent.First;

        while (iterator != null)
        {
            /// TODO: Detect this pointer need
            if (iterator is VariableNode node && node.Variable.IsPredictable && !local_contexts.Any(c => node.Variable.Context.IsInside(c)))
            {
                variables[node.Variable] = variables.GetValueOrDefault(node.Variable, 0) + 1;
            }
            else
            {
                foreach (var usage in GetNonLocalVariableUsageCount(iterator))
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
        // Get all non-local variables in the loop and their number of usages
        var variables = GetNonLocalVariableUsageCount(node, node.StepsContext, node.BodyContext)
                            .Select(i => new VariableUsageInfo(i.Key, i.Value)).ToList();

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

    private static bool ContainsFunction(LoopNode node)
    {
        return node.Find(n => n.Is(NodeType.FUNCTION_NODE)) != null;
    }

    private static void PrepareRelevantVariables(Unit unit, LoopNode node)
    {
        var variables = GetAllVariableUsages(node);
        var non_volatile_mode = ContainsFunction(node);

        unit.Append(new CacheVariablesInstruction(unit, node, variables, non_volatile_mode));
    }

    private static IEnumerable<Variable> GetAllNonLocalVariables(Context local_context, Node body)
    {
        return body.FindAll(n => n.Is(NodeType.VARIABLE_NODE))
                    .Select(n => ((VariableNode)n).Variable)
                    .Where(v => v.IsPredictable && !v.Context.IsInside(local_context))
                    .Distinct();
    }

    private static Result BuildLoopBody(Unit unit, LoopNode loop, LabelInstruction start)
    {
        var non_local_variables = GetAllNonLocalVariables(loop.BodyContext, loop);

        var state = unit.GetState(unit.Position);
        var result = (Result?)null;

        using (var scope = new Scope(unit, non_local_variables))
        {
            // Append the label where the loop will start
            unit.Append(start);

            var symmetry_start = new SymmetryStartInstruction(unit, non_local_variables);
            unit.Append(symmetry_start);

            // Build the loop body
            result = Builders.Build(unit, loop.Body);

            if (!loop.IsForeverLoop)
            {
                // Build the loop action
                Builders.Build(unit, loop.Action);
            }

            var symmetry_end = new SymmetryEndInstruction(unit, symmetry_start);

            // Restore the state after the body
            symmetry_end.Append();

            // Restore the state after the body
            unit.Append(symmetry_end);
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