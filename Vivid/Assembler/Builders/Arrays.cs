public static class Arrays
{
	public static Result BuildOffset(Unit unit, OffsetNode node, AccessMode mode)
	{
		var start = References.Get(unit, node.Start);
		var offset = References.Get(unit, node.Offset.First!);

		if (node.GetType().IsPack)
		{
			return new GetPackMemoryAddressInstruction(unit, node.GetType(), mode, start, offset, node.GetStride()).Execute();
		}

		return new GetMemoryAddressInstruction(unit, mode, node.GetFormat(), start, offset, node.GetStride()).Execute();
	}
}