public class Construction
{
	public static Instructions Build(Unit unit, ConstructionNode node)
	{
		var instructions = new Instructions();

		var type = node.Type;
		var allocation = Call.Build(unit, null, "function_allocate", Size.DWORD, new NumberReference(type.ContentSize, Size.DWORD));

		instructions.Append(allocation);

		var implementation = node.GetConstructor();
		var constructor = implementation.Metadata as Constructor;

		if (!constructor.IsDefault)
		{
			var call = Call.Build(unit, allocation.Reference, implementation, node.Parameters);

			instructions.Append(call);
			instructions.SetReference(call.Reference);
		}
		else
		{
			instructions.SetReference(allocation.Reference);
		}

		return instructions;
	}
}