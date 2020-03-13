using System;

public static class Builders
{
    public static Quantum<Handle> Build(Unit unit, Node node)
    {
        switch (node.GetNodeType())
        {
            case NodeType.FUNCTION_NODE:
            {
                return Calls.Build(unit, (FunctionNode)node);
            }

            case NodeType.OPERATOR_NODE:
            {
                return ArithmeticOperators.Build(unit, (OperatorNode)node);
            }

            case NodeType.RETURN_NODE:
            {
                return Returns.Build(unit, (ReturnNode)node);
            }

            default:
            {
                var iterator = node.First;

                Quantum<Handle>? reference = null;

                while (iterator != null)
                {
                    reference = Builders.Build(unit, iterator);
                    iterator = iterator.Next;
                }

                return reference ?? throw new ArgumentException("Node isn't supported");
            }
        }
    }
}