using System;

public static class Arrays
{
	public static Result BuildOffset(Unit unit, OffsetNode node, AccessMode mode)
	{
		var start = References.Get(unit, node.Start);
		var offset = References.Get(unit, node.Offset.First!);

		var type = node.GetType() ?? throw new ApplicationException("Could not get the memory stride type");
		var stride = Equals(type, Types.LINK) ? 1 : type.ReferenceSize;
		var format = Equals(type, Types.LINK) ? Format.UINT8 : type.Format;

		return new GetMemoryAddressInstruction(unit, mode, format, start, offset, stride).Execute();
	}
}