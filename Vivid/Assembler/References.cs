using System;

public enum AccessMode
{
	WRITE,
	READ
}

public static class References
{
	public static Handle CreateConstantNumber(object value, Format format)
	{
		return new ConstantHandle(value, format);
	}

	public static Handle CreateVariableHandle(Unit unit, Variable variable)
	{
		return CreateVariableHandle(unit, null, null, variable);
	}

	public static Handle CreateVariableHandle(Unit unit, Result? self, Type? self_type, Variable variable)
	{
		// The self pointer must always come with its type
		if (self != null && self_type == null)
		{
			throw new InvalidOperationException("Tried to create variable handle which used a self pointer without its type");
		}

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
				handle = new MemoryHandle
				(
					unit,
					self ?? throw new ArgumentException("Member variable did not have its base pointer"),
					variable.GetAlignment(self_type!) ?? throw new ApplicationException("Member variable was not aligned")
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
		Type? self_type = null;

		if (variable.Category == VariableCategory.MEMBER)
		{
			self = new GetVariableInstruction(unit, unit.Self ?? throw new ApplicationException("Encountered member variable in non-member function"), AccessMode.READ).Execute();
			self_type = unit.Self.Type!;
		}

		return new GetVariableInstruction(unit, self, self_type, variable, mode).Execute();
	}

	public static Result GetConstant(Unit unit, NumberNode node)
	{
		return new GetConstantInstruction(unit, node.Value, node.Type.IsDecimal()).Execute();
	}

	public static Result GetString(Unit unit, StringNode node)
	{
		return new Result(new DataSectionHandle(node.GetIdentifier(unit), true), Assembler.Format);
	}

	public static Result GetDataPointer(DataPointer node)
	{
		return node.Data switch
		{
			FunctionImplementation implementation => new Result(new DataSectionHandle(implementation.GetFullname(), node.Offset, true), Assembler.Format),

			Table table => new Result(new DataSectionHandle(table.Name, node.Offset, true), Assembler.Format),

			_ => throw new ApplicationException("Could not build data pointer")
		};
	}

	public static Result Get(Unit unit, Node node, AccessMode mode = AccessMode.READ)
	{
		switch (node.Instance)
		{
			case NodeType.DATA_POINTER:
			{
				return GetDataPointer((DataPointer)node);
			}

			case NodeType.VARIABLE:
			{
				return GetVariable(unit, (VariableNode)node, mode);
			}

			case NodeType.NUMBER:
			{
				return GetConstant(unit, (NumberNode)node);
			}

			case NodeType.STRING:
			{
				return GetString(unit, (StringNode)node);
			}

			case NodeType.CAST:
			{
				return Casts.Build(unit, (CastNode)node, mode);
			}

			case NodeType.OPERATOR:
			{
				return Builders.Build(unit, (OperatorNode)node);
			}

			case NodeType.OFFSET:
			{
				return Arrays.BuildOffset(unit, (OffsetNode)node, mode);
			}

			case NodeType.LINK:
			{
				return Links.Build(unit, (LinkNode)node, mode);
			}

			case NodeType.CONTENT:
			{
				Result? result = null;

				foreach (var iterator in node)
				{
					result = Get(unit, iterator, mode);
				}

				return result ?? throw new ApplicationException("Found empty parenthesis and its value was required");
			}

			case NodeType.STACK_ADDRESS:
			{
				return new AllocateStackInstruction(unit, node.To<StackAddressNode>()).Execute();
			}

			default:
			{
				return Builders.Build(unit, node);
			}
		}
	}
}