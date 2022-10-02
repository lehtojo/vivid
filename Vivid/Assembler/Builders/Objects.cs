public static class Objects
{
	public static Result Build(Unit unit, ObjectLinkNode node)
	{
		var value = References.Get(unit, node.Value, AccessMode.READ);
		var offset = new Result(new ConstantHandle(8L), Assembler.Signed);
		var address = new GetMemoryAddressInstruction(unit, AccessMode.WRITE, node.Value.GetType(), Assembler.Signed, value, offset, 1).Add();
		var increment = new Result(new ConstantHandle(1L), Assembler.Signed);

		return new AdditionInstruction(unit, address, increment, Assembler.Signed, true).Add();
	}

	public static Result Build(Unit unit, ObjectUnlinkNode node)
	{
		var value = References.Get(unit, node.Value, AccessMode.READ);
		var offset = new Result(new ConstantHandle(8L), Assembler.Signed);
		var address = new GetMemoryAddressInstruction(unit, AccessMode.WRITE, node.Value.GetType(), Assembler.Signed, value, offset, 1).Add();
		var increment = new Result(new ConstantHandle(-1L), Assembler.Signed);

		return new AtomicExchangeAdditionInstruction(unit, address, increment, Assembler.Signed).Add();
	}
}