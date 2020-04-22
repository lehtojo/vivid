using System;

public static class Arrays
{
    public static Result Build(Unit unit, OperatorNode node, AccessMode mode)
    {
        var start = References.Get(unit, node.Left);
        var offset = References.Get(unit, node.Right.First!);

        var type = node.GetType() ?? throw new ApplicationException("Couldn't get the memory stride type");
        var stride = type == Types.LINK ? 1 : type.ReferenceSize;

        return new GetMemoryAddressInstruction(unit, mode, start, offset, stride).Execute();
    }
}