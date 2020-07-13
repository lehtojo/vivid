public static class Returns
{
	public static Result Build(Unit unit, ReturnNode node)
	{
		return new ReturnInstruction(unit, References.Get(unit, node.Value), unit.Function.ReturnType).Execute();
	}
}