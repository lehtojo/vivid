using System;

public static class Construction
{
	public static Result Build(Unit unit, ConstructionNode node)
	{
		var metadata = (Constructor?)node.GetConstructor()?.Metadata ?? throw new ApplicationException("Constructor didn't hold any metadata");

		if (metadata.IsEmpty)
		{
			if (node.Type.ContentSize == 0)
			{
				throw new NotImplementedException("No implementation for empty objects found");
			}

			return Calls.Build(unit, Assembler.AllocationFunction!, CallingConvention.X64, Types.LINK, new NumberNode(Assembler.Format, (long)node.Type.ContentSize));
		}
		else
		{
			return Calls.Build(unit, node.Parameters, node.GetConstructor()!);
		}
	}
}