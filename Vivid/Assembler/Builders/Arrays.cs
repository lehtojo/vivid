public static class Arrays
{
	public static Result BuildOffset(Unit unit, OffsetNode node, AccessMode mode)
	{
		var start = References.Get(unit, node.Start, mode);
		var offset = References.Get(unit, node.Offset.First!);
		var stride = node.GetStride();

		// The memory address of the accessor must be created in multiple steps, if the stride is too large and it can not be combined with the offset
		if (stride > Instructions.X64.EVALUATE_MAX_MULTIPLIER)
		{
			// Pattern:
			// index = offset * stride
			// => [start + index]
			var index = new MultiplicationInstruction(unit, offset, new Result(new ConstantHandle((long)stride), Assembler.Format), Assembler.Format, false).Execute();

			return new GetMemoryAddressInstruction(unit, mode, node.GetType(), node.GetFormat(), start, index, 1).Execute();
		}

		// Pattern: [start + offset * stride]
		return new GetMemoryAddressInstruction(unit, mode, node.GetType(), node.GetFormat(), start, offset, stride).Execute();
	}
}