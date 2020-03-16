using System;

public static class Arrays
{
    public static Result Build(Unit unit, OperatorNode node)
    {
        var @base = References.Get(unit, node.Left);
        var offset = References.Get(unit, node.Right.First!);

        var type = node.GetType() ?? throw new ApplicationException("Couldn't get the type for memory shifting");

        return new GetMemoryAddressInstruction(unit, @base, offset, type.Size).Execute();
    }
}