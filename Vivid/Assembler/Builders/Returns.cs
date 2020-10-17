public static class Returns
{
	public static Result Build(Unit unit, ReturnNode node)
	{
		return new ReturnInstruction(unit, node.Value != null ? References.Get(unit, node.Value) : null, unit.Function.ReturnType).Execute();
	}
}