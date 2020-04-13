using System;

public static class References
{
    public static Handle CreateConstantNumber(Unit unit, object value)
    {
        return new ConstantHandle(value);
    }

    public static Handle CreateVariableHandle(Unit unit, Result? self, Variable variable)
    {
        switch (variable.Category)
        {
            case VariableCategory.PARAMETER:
            {
                return MemoryHandle.FromStack(unit, variable.Alignment);
            }

            case VariableCategory.LOCAL:
            {
                return MemoryHandle.FromStack(unit, -variable.Alignment - variable.Type!.Size);
            }

            case VariableCategory.MEMBER:
            {
                return new MemoryHandle(self ?? throw new ArgumentException("Member variable didn't have its base pointer"), variable.Alignment);
            }

            default:
            {
                throw new NotImplementedException("Variable category not implemented");
            }
        }
    }

    public static Result GetVariable(Unit unit, VariableNode node, AccessMode mode)
    {
        return GetVariable(unit, node.Variable, mode);
    }

    public static Result GetVariable(Unit unit, Variable variable, AccessMode mode)
    {
        Result? self = null;

        if (variable.Category == VariableCategory.MEMBER)
        {
            self = new GetSelfPointerInstruction(unit).Execute();
        }

        var handle = new GetVariableInstruction(unit, self, variable, mode).Execute();
        var version = unit.GetCurrentVariableVersion(variable);
        
        handle.Metadata.Attach(new VariableAttribute(variable, version));

        return handle;
    }

    public static Result GetConstant(Unit unit, NumberNode node)
    {
        var handle = new GetConstantInstruction(unit, node.Value).Execute();
        handle.Metadata.Attach(new ConstantAttribute(node.Value));

        return handle;
    }

    public static Result GetString(Unit unit, StringNode node)
    {
        return new Result(new DataSectionHandle(node.GetIdentifier(unit)));
    }

    public static Result Get(Unit unit, Node node, AccessMode mode = AccessMode.READ)
    {
        switch (node.GetNodeType())
        {
            case NodeType.VARIABLE_NODE:
            {
                return GetVariable(unit, (VariableNode)node, mode);
            }

            case NodeType.NUMBER_NODE:
            {
                return GetConstant(unit, (NumberNode)node);
            }

            case NodeType.STRING_NODE:
            {
                return GetString(unit, (StringNode)node);
            }

            default:
            {
                return Builders.Build(unit, node);
            }
        }
    }
}