using System;

public static class References
{
    public static Handle CreateConstantNumber(Unit unit, object value)
    {
        return new ConstantHandle(value);
    }

    public static Handle CreateVariableHandle(Unit unit, Result? self, Variable variable)
    {
        var handle = (Handle?)null;

        switch (variable.Category)
        {
            case VariableCategory.PARAMETER:
            {
                handle = new VariableMemoryHandle(unit, variable);
                break;
            }

            case VariableCategory.LOCAL:
            {
                handle = new VariableMemoryHandle(unit, variable);
                break;
            }

            case VariableCategory.MEMBER:
            {
                handle = new MemoryHandle(unit, self ?? throw new ArgumentException("Member variable didn't have its base pointer"), variable.Alignment);
                handle.Size = variable.Type!.GetSize();
                break;
            }

            case VariableCategory.GLOBAL:
            {
                handle = new DataSectionHandle(variable.GetStaticName());
                break;
            }

            default:
            {
                throw new NotImplementedException("Variable category not implemented");
            }
        }

        if (variable.Type is Number number && !number.IsUnsigned)
        {
            handle.IsUnsigned = false;
        }

        return handle;
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
            self = new GetVariableInstruction(unit, null, unit.Self ?? throw new ApplicationException("Encountered member variable in non-member function"), AccessMode.READ).Execute();
        }

        var handle = new GetVariableInstruction(unit, self, variable, mode).Execute();
        handle.Metadata.Attach(new VariableAttribute(variable));

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
        return new Result(new ConstantHandle(node.GetIdentifier(unit)));
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

            case NodeType.CAST_NODE:
            {
                var cast = (CastNode)node;
                return References.Get(unit, cast.First!, mode);
            }

            case NodeType.OPERATOR_NODE:
            {
                var operation = (OperatorNode)node;

                if (operation.Operator == Operators.EXTENDER)
                {
                    return Arrays.BuildOffset(unit, operation, mode);
                }

                return Builders.Build(unit, node);
            }

            default:
            {
                return Builders.Build(unit, node);
            }
        }
    }
}