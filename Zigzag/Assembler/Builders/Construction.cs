public class Construction
{
	public static Instructions Build(Unit unit, ConstructionNode node)
	{
		Instructions instructions = new Instructions();

		Type type = node.Type;
		Instructions allocation = Call.Build(unit, null, "function_allocate", Size.DWORD, new NumberReference(type.ContentSize, Size.DWORD));

		instructions.Append(allocation);
		instructions.SetReference(allocation.Reference);

		Constructor constructor = node.GetConstructor();

		if (!constructor.IsDefault)
		{
			Instructions call = Call.Build(unit, allocation.Reference, constructor, node.Parameters);
			instructions.Append(call);
		}

		return instructions;
	}
}