using System;

public static class Construction
{
    public static Result Build(Unit unit, ConstructionNode node)
    {
        var metadata = (Constructor?)node.GetConstructor()?.Metadata ?? throw new ApplicationException("Constructor didn't hold any metadata");
        
        if (metadata.IsDefault)
        {
            if (node.Type.ContentSize == 0)
            {
                throw new NotImplementedException("No implementation for empty objects found");
            }

            return Calls.Build(unit, Memory.FUNCTION_ALLOCATE, new NumberNode(NumberType.INT32, node.Type.ContentSize));
        }
        else
        {
            return Calls.Build(unit, node.Parameters, node.GetConstructor()!);
        }
    }
}