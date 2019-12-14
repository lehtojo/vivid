public class Return
{
	public static Instructions Build(Unit unit, ReturnNode node)
	{
		var instructions = new Instructions();

		var @object = References.Read(unit, node.First);
		instructions.Append(@object);

		var reference = @object.Reference;

		if (reference.GetRegister() != unit.EAX)
		{
			Memory.Move(unit, instructions, reference, new RegisterReference(unit.EAX));
		}

		var inline = node.FindParent(n => n.GetNodeType() == NodeType.INLINE_NODE) as InlineNode;

		if (inline == null)
		{
			instructions.Append(Functions.FOOTER);
		}
		else
		{
			instructions.Append($"jmp {inline.GetEndLabel()}");
		}

		return instructions.SetReference(Value.GetOperation(unit.EAX, reference.GetSize()));
	}
}