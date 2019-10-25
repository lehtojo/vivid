public class Links
{
	public static Instructions Build(Unit unit, LinkNode node, ReferenceType type)
	{
		Instructions instructions = new Instructions();

		if (node.Right.GetNodeType() == NodeType.FUNCTION_NODE)
		{
			Instructions left = References.Read(unit, node.Left);
			instructions.Append(left);

			Instructions call = Call.Build(unit, left.Reference, (FunctionNode)node.Right);
			instructions.Append(call).SetReference(call.Reference);
		}
		else if (node.Right.GetNodeType() == NodeType.VARIABLE_NODE)
		{
			Variable variable = ((VariableNode)node.Right).Variable;

			if (type != ReferenceType.DIRECT)
			{
				Register register = unit.IsRegisterCached(variable);

				if (register != null)
				{
					return instructions.SetReference(register.Value);
				}
			}

			Instructions left = References.Register(unit, node.Left);
			instructions.Append(left);

			Reference reference = new MemoryReference(left.Reference.GetRegister(), variable.Alignment, variable.Type.Size);

			if (type == ReferenceType.VALUE || type == ReferenceType.REGISTER)
			{
				Instructions move = Memory.ToRegister(unit, reference);
				instructions.Append(move);

				Size size = Size.Get(variable.Type.Size);

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