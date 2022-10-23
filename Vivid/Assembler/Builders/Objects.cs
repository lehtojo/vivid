public static class Objects
{
	public static Result Build(Unit unit, ObjectLinkNode node)
	{
		var value = References.Get(unit, node.Value, AccessMode.READ);
		var offset = new Result(new ConstantHandle(8L), Settings.Signed);
		var address = new GetMemoryAddressInstruction(unit, AccessMode.WRITE, node.Value.GetType(), Settings.Signed, value, offset, 1).Add();
		var increment = new Result(new ConstantHandle(1L), Settings.Signed);

		return new AdditionInstruction(unit, address, increment, Settings.Signed, true).Add();
	}

	public static Result Build(Unit unit, ObjectUnlinkNode node)
	{
		var value = References.Get(unit, node.Value, AccessMode.READ);
		var offset = new Result(new ConstantHandle(8L), Settings.Signed);
		var address = new GetMemoryAddressInstruction(unit, AccessMode.WRITE, node.Value.GetType(), Settings.Signed, value, offset, 1).Add();
		var increment = new Result(new ConstantHandle(-1L), Settings.Signed);

		return new AtomicExchangeAdditionInstruction(unit, address, increment, Settings.Signed).Add();
	}
}