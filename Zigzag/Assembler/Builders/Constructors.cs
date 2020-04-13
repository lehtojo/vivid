using System;

public static class Constructors
{
    public static void CreateHeader(Unit unit, Type type)
    {
        if (type.ContentSize == 0)
        {
            throw new NotImplementedException("No implementation for empty objects found");
        }

        unit.Self = Calls.Build(unit, Memory.FUNCTION_ALLOCATE, new NumberNode(NumberType.INT32, type.ContentSize));
    }
}