using System;
using System.Collections.Generic;
using System.Text;

public static class Increments
{
	public static Instructions Build(Unit unit, IncrementNode node, ReferenceType type)
	{
		var instructions = new Instructions();

		if (type == ReferenceType.DEFAULT)
		{
			var source = References.Get(unit, node.Object, ReferenceType.DIRECT);
			instructions.Append(source);
			instructions.Append(Instruction.Unsafe("add", source.Reference, new NumberReference(1, Size.DWORD), Size.DWORD));

			return instructions;
		}
		else
		{
			var source = References.Get(unit, node.Object, ReferenceType.VALUE);
			instructions.Append(source);
			instructions.Append(Instruction.Unsafe("add", source.Reference, new NumberReference(1, Size.DWORD), Size.DWORD));

			var destination = References.Get(unit, node.Object, ReferenceType.DIRECT).Reference;
			Memory.Move(unit, instructions, source.Reference, destination);

			return instructions.SetReference(Value.GetOperation(source.Reference.GetRegister(), Size.DWORD));
		}
	}
}
