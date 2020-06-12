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

		var allocation = Calls.Build(unit, Assembler.AllocationFunction!, CallingConvention.X64, Types.LINK, new NumberNode(Assembler.Format, type.ContentSize));
		allocation.Metadata.Attach(new VariableAttribute(unit.Self));

		//unit.Cache(unit.Self, allocation, true);
		unit.Append(new SetVariableInstruction(unit, unit.Self, allocation));
	}
}