using System;

public static class References
{
	public static Handle CreateConstantNumber(object value)
	{
		return new ConstantHandle(value);
	}

	public static Handle CreateVariableHandle(Unit unit, Result? self, Variable variable)
	{
      Handle? handle;

      switch (variable.Category)
		{
			case VariableCategory.PARAMETER:
			{
				handle = new StackVariableHandle(unit, variable);
				break;
			}

			case VariableCategory.LOCAL:
			{
				handle = new StackVariableHandle(unit, variable);
				break;
			}

			case VariableCategory.MEMBER:
			{
				var parent = (Type)variable.Context;

				handle = new MemoryHandle
				(
					unit, 
					self ?? throw new ArgumentException("Member variable didn't have its base pointer"), 
					variable.GetAlignment(parent) ?? throw new ApplicationException("Member variable wasn't aligned")
				);
				
				break;
			}

			case VariableCategory.GLOBAL:
			{
				handle = new DataSectionHandle(variable.GetStaticName());
				break;
			}

			default: throw new NotImplementedException("Unrecognized variable category");
		}

		return handle;
	}

	public static Result GetVariable(Unit unit, VariableNode node, AccessMode mode)
	{
		return GetVariable(unit, node.Variable, mode);
	}

	public static Result GetVariable(Unit unit, Variable variable, AccessMode mode)
	{
		Result? self = null;

		if (variable.Category == VariableCategory.MEMBER)
		{
			self = new GetVariableInstruction(unit, null, unit.Self ?? throw new ApplicationException("Encountered member variable in non-member function"), AccessMode.READ).Execute();
		}

		var handle = new GetVariableInstruction(unit, self, variable, mode).Execute();
		handle.Metadata.Attach(new VariableAttribute(variable));

		return handle;
	}

	public static Result GetConstant(Unit unit, NumberNode node)
	{
		var handle = new GetConstantInstruction(unit, node.Value, node.Type).Execute();
		handle.Metadata.Attach(new ConstantAttribute(node.Value));

		return handle;
	}

	public static Result GetString(Unit unit, StringNode node)
	{
		return new Result(new DataSectionHandle(node.GetIdentifier(unit), true), Assembler.Size.ToFormat());
	}

	public static Result Get(Unit unit, Node node, AccessMode mode = AccessMode.READ)
	{
		switch (node.GetNodeType())
		{
			case NodeType.VARIABLE_NODE:
			{
				return GetVariable(unit, (VariableNode)node, mode);
			}

			case NodeType.NUMBER_NODE:
			{
				return GetConstant(unit, (NumberNode)node);
			}

			case NodeType.STRING_NODE:
			{
				return GetString(unit, (StringNode)node);
			}

			case NodeType.CAST_NODE:
			{
				return Casts.Build(unit, (CastNode)node);
			}

			case NodeType.OPERATOR_NODE:
			{
				return Builders.Build(unit, (OperatorNode)node);
			}

			case NodeType.OFFSET_NODE:
			{
				return Arrays.BuildOffset(unit, (OffsetNode)node, mode);
			}

			default:
			{
				return Builders.Build(unit, node);
			}
		}
	}
}