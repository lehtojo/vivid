using System;

public static class ArithmeticOperators
{
    public static Result Build(Unit unit, OperatorNode node)
    {
        var operation = node.Operator;

        if (operation == Operators.ADD)
        {
            return BuildAdditionOperator(unit, node);
        }
        else if (operation == Operators.SUBTRACT)
        {
            return BuildSubtractionOperator(unit, node);
        }
        else if (operation == Operators.MULTIPLY)
        {
            return BuildMultiplicationOperator(unit, node);
        }
        else if (operation == Operators.ASSIGN)
        {
            return BuildAssignOperator(unit, node);
        }
        else if (operation == Operators.EXTENDER)
        {
            return Arrays.Build(unit, node);
        }

        throw new ArgumentException("Node not implemented yet");
    } 

    public static Result BuildAdditionOperator(Unit unit, OperatorNode node)
    {
        var left = References.Get(unit, node.Left);
        var right = References.Get(unit, node.Right);

        return new AdditionInstruction(unit, left, right).Execute();
    }

    public static Result BuildSubtractionOperator(Unit unit, OperatorNode node)
    {
        var left = References.Get(unit, node.Left);
        var right = References.Get(unit, node.Right);

        return new SubtractionInstruction(unit, left, right).Execute();
    }

    public static Result BuildMultiplicationOperator(Unit unit, OperatorNode node)
    {
        var left = References.Get(unit, node.Left);
        var right = References.Get(unit, node.Right);

        return new MultiplicationInstruction(unit, left, right).Execute();
    }

    public static Result BuildAssignOperator(Unit unit, OperatorNode node) 
    {
        var left = References.Get(unit, node.Left);
        var right = References.Get(unit, node.Right);

        return new AssignInstruction(unit, left, right).Execute();
    }

    public static void GetDivisionConstants(int divider, int bits)
    {
        
    }
}