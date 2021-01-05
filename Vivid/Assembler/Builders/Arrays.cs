public static class Arrays
{
	public static Result BuildOffset(Unit unit, OffsetNode node, AccessMode mode)
	{
		var start = References.Get(unit, node.Start);
		var offset = References.Get(unit, node.Offset.First!);

		return new GetMemoryAddressInstruction(unit, mode, node.GetFormat(), start, offset, node.GetStride()).Execute();
	}
}