using System;

public static class Conditionals
{
    private static Result Build(Unit unit, IfNode node, OperatorNode condition, Label end)
    {
        var left = References.Get(unit, condition.Left);
        var right = References.Get(unit, condition.Right);

        // Compare the two operands
        var comparison = new CompareInstruction(unit, left, right).Execute();

        // Set the next label to be the end label if there's no successor since then there wont be any other comparisons
        var interphase = node.Successor == null ? end : unit.GetNextLabel();

        // Jump to the next label based on the comparison
        unit.Append(new JumpInstruction(unit, comparison, (ComparisonOperator)condition.Operator, true, interphase));

        // Build the body of this if statement
        var result = Builders.Build(unit, node.Body);

        // If the if-statement body is executed it must skip the potential successors
        if (node.Successor != null)
        {
            // Skip the next successor from this if-statement's body and add the interphase label
            unit.Append(new JumpInstruction(unit, end));
            unit.Append(new LabelInstruction(unit, interphase));

            // Build the successor
            return Conditionals.Build(unit, node.Successor, end);
        }

        return result;
    }

    private static Result Build(Unit unit, Node node, Label end)
    {
        if (node is IfNode conditional)
        {
            var condition = conditional.Condition;

            if (condition.Is(NodeType.OPERATOR_NODE))
            {
                var operation = (OperatorNode)condition;

                if (operation.Operator.Type == OperatorType.COMPARISON)
                {
                    return Build(unit, conditional, operation, end);
                }
            }

            throw new NotImplementedException("Complex conditional statements not implemented");
        }
        else
        {
            return Builders.Build(unit, node);
        }   
    }

    public static Result Start(Unit unit, IfNode node)
    {
        var end = unit.GetNextLabel();
        var result = Build(unit, node, end);
        unit.Append(new LabelInstruction(unit, end));

        return result;
    }
}