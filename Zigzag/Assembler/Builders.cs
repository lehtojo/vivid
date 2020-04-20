using System;

public static class Builders
{
    public static Result Build(Unit unit, Node node)
    {
        switch (node.GetNodeType())
        {
            case NodeType.FUNCTION_NODE:
            {
                return Calls.Build(unit, (FunctionNode)node);
            }

            case NodeType.INCREMENT_NODE:
            {
                return ArithmeticOperators.Build(unit, (IncrementNode)node);
            }

            case NodeType.OPERATOR_NODE:
            {
                return ArithmeticOperators.Build(unit, (OperatorNode)node);
            }

            case NodeType.LINK_NODE:
            {
                return Links.Build(unit, (LinkNode)node);
            }

            case NodeType.CONSTRUCTION_NODE:
            {
                return Construction.Build(unit, (ConstructionNode)node);
            }

            case NodeType.IF_NODE:
            {
                return Conditionals.Start(unit, (IfNode)node);
            }

            case NodeType.LOOP_NODE:
            {
                return Loops.Build(unit, (LoopNode)node);
            }

            case NodeType.RETURN_NODE:
            {
                return Returns.Build(unit, (ReturnNode)node);
            }

            case NodeType.CAST_NODE:
            {
                return Builders.Build(unit, node.First!);
            }

            case NodeType.NEGATE_NODE:
            {
                return ArithmeticOperators.BuildNegate(unit, (NegateNode)node);
            }

            default:
            {
                var iterator = node.First;

                Result? reference = null;

                while (iterator != null)
                {
                    reference = Builders.Build(unit, iterator);
                    iterator = iterator.Next;
                }

                return reference ?? new Result();
            }
        }
    }
}