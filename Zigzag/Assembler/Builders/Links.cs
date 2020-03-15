using System;

public static class Links
{
    public static Result Build(Unit unit, LinkNode node)
    {
        var @base = References.Get(unit, node.Object);

        if (node.Member is VariableNode member)
        {
            return new GetObjectPointerInstruction(unit, member.Variable, @base, member.Variable.Alignment).Execute();
        }
        else if (node.Member is FunctionNode function)
        {
            return Calls.Build(unit, @base, function);
        }
        else
        {
            throw new NotImplementedException("Unsupported member node");
        }
    }
}