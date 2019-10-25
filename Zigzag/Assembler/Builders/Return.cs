public class Return
{
	public static Instructions Build(Unit unit, ReturnNode node)
	{
		Instructions instructions = new Instructions();

		Instructions @object = References.Read(unit, node.First);
		instructions.Append(@object);

		Reference reference = @object.Reference;

		if (reference.GetRegister() != unit.EAX)
		{
			instructions.Append(Memory.Move(unit, reference, new RegisterReference(unit.EAX)));
		}

		instructions.Append(Functions.FOOTER);

		return instructions.SetReference(Value.GetOperation(unit.EAX, reference.GetSize()));
	}
}