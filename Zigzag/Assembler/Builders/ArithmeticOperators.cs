using System;

public static class ArithmeticOperators
{
    public static Quantum<Handle> Build(Unit unit, OperatorNode node)
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

        throw new ArgumentException("Node not implemented yet");
    } 

    public static Quantum<Handle> BuildAdditionOperator(Unit unit, OperatorNode node)
    {
        var left = References.Get(unit, node.Left);
        var right = References.Get(unit, node.Right);

        return new AdditionInstruction(left, right).Execute(unit);
    }

    public static Quantum<Handle> BuildSubtractionOperator(Unit unit, OperatorNode node)
    {
        var left = References.Get(unit, node.Left);
        var right = References.Get(unit, node.Right);

        return new SubtractionInstruction(left, right).Execute(unit);
    }

    public static Quantum<Handle> BuildMultiplicationOperator(Unit unit, OperatorNode node)
    {
        var left = References.Get(unit, node.Left);
        var right = References.Get(unit, node.Right);

        return new MultiplicationInstruction(left, right).Execute(unit);
    }

    public static Quantum<Handle> BuildAssignOperator(Unit unit, OperatorNode node) 
    {
        var left = References.Get(unit, node.Left);
        var right = References.Get(unit, node.Right);

        return new AssignInstruction(left, right).Execute(unit);
    }

    public static void GetDivisionConstants(int divider, int bits)
    {
        
    }
}