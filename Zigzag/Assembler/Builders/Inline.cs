using System;
using System.Collections.Generic;
using System.Text;

public static class Inline
{
	public static Instructions Build(Unit unit, InlineNode node)
	{
		var instructions = new Instructions();

		var implementation = node.Implementation;
		var iterator = node.Parameters.First;

		instructions.Comment($"Inlined global function '{implementation.Metadata.Name}'");

		for (var i = 0; i < node.Implementation.Parameters.Count; i++)
		{
			var parameter = node.Implementation.Parameters[i];

			var destination = References.GetVariableReference(unit, parameter, ReferenceType.DIRECT);
			instructions.Append(destination);

			var source = References.Get(unit, iterator, ReferenceType.VALUE);
			instructions.Append(source);

			Memory.Move(unit, instructions, source.Reference, destination.Reference);
			
			if (source.Reference.IsRegister())
			{
				var register = source.Reference.GetRegister();
				register.Attach(Value.GetVariable(source.Reference, parameter));
			}

			iterator = iterator.Next;
		}

		var body = unit.Assemble(node.Body);
		instructions.Append(body);

		var result = implementation.ReturnType;

		if (result != Types.UNKNOWN)
		{
			var size = Size.Get(implementation.ReturnType.Size);
			instructions.SetReference(Value.GetOperation(unit.EAX, size));
		}

		if (node.End)
		{
			instructions.Label(node.GetEndLabel());
		}

		instructions.Comment($"End of inlined global function '{implementation.Metadata.Name}'");

		return instructions;
	}
}