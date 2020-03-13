using System;

public static class References
{
    public static Handle CreateConstantNumber(Unit unit, object value)
    {
        var handle = new ConstantHandle(value);
        unit.AddHandle(handle);

        return handle;
    }

    public static Handle CreateVariableHandle(Unit unit, Variable variable)
    {
        switch (variable.Category)
        {
            case VariableCategory.PARAMETER:
            {
                var handle = new StackMemoryHandle(variable.Alignment);
                unit.AddHandle(handle);

                return handle;
            }

            case VariableCategory.LOCAL:
            {
                var handle = new StackMemoryHandle(-variable.Alignment);
                unit.AddHandle(handle);

                return handle;
            }

            default:
            {
                throw new NotImplementedException("Variable category not implemented");
            }
        }
    }

    public static Quantum<Handle> GetVariable(Unit unit, VariableNode node)
    {
        var handle = new GetVariableInstruction(node.Variable).Execute(unit);
        handle.Value.Metadata = node.Variable;

        return handle;
    }

    public static Quantum<Handle> GetConstant(Unit unit, NumberNode node)
    {
        var handle = new GetConstantInstruction(node.Value).Execute(unit);
        handle.Value.Metadata = node.Value;

        return handle;
    }

    public static Quantum<Handle> Get(Unit unit, Node node)
    {
        switch (node.GetNodeType())
        {
            case NodeType.VARIABLE_NODE:
            {
                return GetVariable(unit, (VariableNode)node);
            }

            case NodeType.NUMBER_NODE:
            {
                return GetConstant(unit, (NumberNode)node);
            }

            default:
            {
                return Builders.Build(unit, node);
            }
        }
    }
}