public class Links
{
	public static Instructions Build(Unit unit, LinkNode node, ReferenceType type)
	{
		var instructions = new Instructions();

		if (node.Right.GetNodeType() == NodeType.FUNCTION_NODE)
		{
			var left = References.Read(unit, node.Left);
			instructions.Append(left);

			var call = Call.Build(unit, left.Reference, (FunctionNode)node.Right);
			instructions.Append(call).SetReference(call.Reference);
		}
		else if (node.Right.GetNodeType() == NodeType.VARIABLE_NODE)
		{
			var variable = ((VariableNode)node.Right).Variable;

			if (type != ReferenceType.DIRECT)
			{
				var register = unit.IsRegisterCached(variable);

				if (register != null)
				{
					return instructions.SetReference(register.Value);
				}
			}

			var left = References.Register(unit, node.Left);
			instructions.Append(left);

			var reference = new MemoryReference(left.Reference.GetRegister(), variable.Alignment, variable.Type.Size);

			if (type == ReferenceType.VALUE || type == ReferenceType.REGISTER)
			{
				var move = Memory.ToRegister(unit, reference);
				instructions.Append(move);

				var size = Size.Get(variable.Type.Size);

				return instructions.SetReference(Value.GetOperation(move.Reference.GetRegister(), size));
			}
			else
			{
				return instructions.SetReference(reference);
			}
		}

		return instructions;
	}
}