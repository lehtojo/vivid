using System;

public static class ArithmeticOperators
{
    public static Result Build(Unit unit, IncrementNode node)
    {
        return BuildAdditionOperator(unit, node.Object, new NumberNode(NumberType.INT32, 1), true);
    }

    public static Result Build(Unit unit, OperatorNode node)
    {
        /// TODO: Create a register preference system dependent on the situation
        var operation = node.Operator;
        
        if (operation == Operators.ADD)
        {
            return BuildAdditionOperator(unit, node.Left, node.Right);
        }
        else if (operation == Operators.SUBTRACT)
        {
            return BuildSubtractionOperator(unit, node.Left, node.Right);
        }
        else if (operation == Operators.MULTIPLY)
        {
            return BuildMultiplicationOperator(unit, node.Left, node.Right);
        }
        else if (operation == Operators.DIVIDE)
        {
            return BuildDivisionOperator(unit, false, node.Left, node.Right);
        }
        else if (operation == Operators.MODULUS)
        {
            return BuildDivisionOperator(unit, true, node.Left, node.Right);
        }
        else if (operation == Operators.ASSIGN_ADD)
        {
            return BuildAdditionOperator(unit, node.Left, node.Right, true);
        }
        else if (operation == Operators.ASSIGN_SUBTRACT)
        {
            return BuildSubtractionOperator(unit, node.Left, node.Right, true);
        }
        else if (operation == Operators.ASSIGN_MULTIPLY)
        {
            return BuildMultiplicationOperator(unit, node.Left, node.Right, true);
        }
        else if (operation == Operators.ASSIGN_DIVIDE)
        {
            return BuildDivisionOperator(unit, false, node.Left, node.Right, true);
        }
        else if (operation == Operators.ASSIGN_MODULUS)
        {
            return BuildDivisionOperator(unit, true, node.Left, node.Right, true);
        }
        else if (operation == Operators.ASSIGN)
        {
            return BuildAssignOperator(unit, node);
        }
        else if (operation == Operators.EXTENDER)
        {
            return Arrays.Build(unit, node, AccessMode.READ);
        }

        throw new ArgumentException("Node not implemented yet");
    }

    public static Result BuildNegate(Unit unit, NegateNode node)
    {
        return new NegateInstruction(unit, References.Get(unit, node.Target)).Execute();
    }

    public static Result BuildAdditionOperator(Unit unit, Node first, Node second, bool assigns = false)
    {
        var left = References.Get(unit, first, assigns ? AccessMode.WRITE : AccessMode.READ);
        var right = References.Get(unit, second);

        return new AdditionInstruction(unit, left, right, assigns).Execute();
    }

    public static Result BuildSubtractionOperator(Unit unit, Node first, Node second, bool assigns = false)
    {
        var left = References.Get(unit, first, assigns ? AccessMode.WRITE : AccessMode.READ);
        var right = References.Get(unit, second);

        return new SubtractionInstruction(unit, left, right, assigns).Execute();
    }

    public static Result BuildMultiplicationOperator(Unit unit, Node first, Node second, bool assigns = false)
    {
        var left = References.Get(unit, first, assigns ? AccessMode.WRITE : AccessMode.READ);
        var right = References.Get(unit, second);

        return new MultiplicationInstruction(unit, left, right, assigns).Execute();
    }

    public static Result BuildDivisionOperator(Unit unit, bool modulus, Node first, Node second, bool assigns = false)
    {
        var left = References.Get(unit, first, assigns ? AccessMode.WRITE : AccessMode.READ);
        var right = References.Get(unit, second);

        return new DivisionInstruction(unit, modulus, left, right, assigns).Execute();
    }

    public static Result BuildAssignOperator(Unit unit, OperatorNode node) 
    {
        var left = References.Get(unit, node.Left, AccessMode.WRITE);
        var right = References.Get(unit, node.Right);

        return new AssignInstruction(unit, left, right).Execute();
    }

    public static void GetDivisionConstants(int divider, int bits)
    {
        
    }
}