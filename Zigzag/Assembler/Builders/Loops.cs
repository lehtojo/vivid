
public static class Loops
{
    private static Result BuildForeverLoop(Unit unit, LoopNode node)
    {
        var start = unit.GetNextLabel();

        // Append the label where the loop will start
        unit.Append(new LabelInstruction(unit, start));

        // Build the loop body
        var result = Builders.Build(unit, node.Body);

        // Jump to the start of the loop
        unit.Append(new JumpInstruction(unit, start));

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
    
        if (node.Condition is OperatorNode start_condition)
        {
            var left = References.Get(unit, start_condition.Left);
            var right = References.Get(unit, start_condition.Right);

            // Compare the two operands
            var comparison = new CompareInstruction(unit, left, right).Execute();

            // Jump to the next label based on the comparison
            unit.Append(new JumpInstruction(unit, comparison, (ComparisonOperator)start_condition.Operator, true, end));
        }

        // Append the label where the loop body will start
        unit.Append(new LabelInstruction(unit, start));

        // Build the loop body
        var result = Builders.Build(unit, node.Body);

        if (node.Condition is OperatorNode end_condition)
        {
            var left = References.Get(unit, end_condition.Left);
            var right = References.Get(unit, end_condition.Right);

            // Compare the two operands
            var comparison = new CompareInstruction(unit, left, right).Execute();

            // Jump to the next label based on the comparison
            unit.Append(new JumpInstruction(unit, comparison, (ComparisonOperator)end_condition.Operator, true, start));
        }

        // Append the label where the loop ends
        unit.Append(new LabelInstruction(unit, end));

        return result;
    }
}