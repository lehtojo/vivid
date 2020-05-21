using System;

public static class Arrays
{
	public static Result BuildOffset(Unit unit, OperatorNode node, AccessMode mode)
	{
		var start = References.Get(unit, node.Left);
		var offset = References.Get(unit, node.Right.First!);

		var type = node.GetType() ?? throw new ApplicationException("Couldn't get the memory stride type");
		var stride = type == Types.LINK ? 1 : type.ReferenceSize;
		var format = type == Types.LINK ? Format.UINT8 : type.Format;

		return new GetMemoryAddressInstruction(unit, mode, format, start, offset, stride).Execute();
	}

	public static Result BuildAllocation(Unit unit, ArrayAllocationNode array)
	{
		return Calls.Build(unit, Assembler.AllocationFunction!, CallingConvention.X64, array.Length);
	}
}