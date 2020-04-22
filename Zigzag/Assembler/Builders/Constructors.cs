using System;

public static class Constructors
{
    public static void CreateHeader(Unit unit, Type type)
    {
        if (type.ContentSize == 0)
        {
            throw new NotImplementedException("No implementation for empty objects found");
        }

        if (unit.Self == null)
        {
            throw new ApplicationException("Couldn't create constructor header since this pointer was missing");
        }

        var allocation = Calls.Build(unit, Assembler.AllocationFunction!, new NumberNode(NumberType.INT32, type.ContentSize));
        allocation.Metadata.Attach(new VariableAttribute(unit.Self, -1));

        unit.Cache(unit.Self, allocation, true);
    }
}