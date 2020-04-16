using System;

public static class Arrays
{
    public static Result Build(Unit unit, OperatorNode node, AccessMode mode)
    {
        var @base = References.Get(unit, node.Left);
        var offset = References.Get(unit, node.Right.First!);

        var type = node.GetType() ?? throw new ApplicationException("Couldn't get the memory stride type");

        return new GetMemoryAddressInstruction(unit, mode, @base, offset, type.ReferenceSize).Execute();
    }
}