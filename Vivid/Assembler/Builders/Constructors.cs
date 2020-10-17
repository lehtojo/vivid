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
			throw new ApplicationException("Could not create constructor header since this pointer was missing");
		}

		var allocation = Calls.Build(unit, Assembler.AllocationFunction!, CallingConvention.X64, Types.LINK, new NumberNode(Assembler.Format, (long)type.ContentSize));
		allocation.Metadata.Attach(new VariableAttribute(unit.Self));

		unit.Append(new SetVariableInstruction(unit, unit.Self, allocation));

		if (type.Initialization != null)
		{
			Builders.Build(unit, type.Initialization);
		}
	}

	public static void CreateFooter(Unit unit, Type type)
	{
		var self = References.GetVariable(unit, unit.Self!);
		unit.Append(new ReturnInstruction(unit, self, type));
	}
}