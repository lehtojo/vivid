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

		switch (variable.Category)
		{
			case VariableCategory.PARAMETER:
			{
				return new StackVariableHandle(unit, variable);
			}

			case VariableCategory.LOCAL:
			{
				return variable.IsInlined()
					? new StackAllocationHandle(unit, variable.Type!.AllocationSize, variable.Context.Identity + '.' + variable.Name)
					: new StackVariableHandle(unit, variable);
			}

			case VariableCategory.MEMBER:
			{
				if (variable.Type!.IsPack)
				{
					throw new NotSupportedException("Accessing pack members is not supported here");
				}

				return new MemoryHandle
				(
					unit,
					self ?? throw new ArgumentException("Member variable did not have its self pointer"),
					variable.GetAlignment(self_type!) ?? throw new ApplicationException("Member variable was not aligned")
				);
			}

			case VariableCategory.GLOBAL:
			{
				if (variable.Type!.IsPack)
				{
					throw new NotSupportedException("Accessing pack members is not supported here");
				}

				var handle = new DataSectionHandle(variable.GetStaticName());

				if (Assembler.UseIndirectAccessTables)
				{
					handle.Modifier = DataSectionModifier.GLOBAL_OFFSET_TABLE;
				}

				return handle;
			}

			default: throw new NotImplementedException("Unrecognized variable category");
		}
	}

	public static Result GetVariable(Unit unit, VariableNode node, AccessMode mode)
	{
		return GetVariable(unit, node.Variable, mode);
	}

	public static Result GetVariableDebug(Unit unit, Variable variable, AccessMode mode)
	{
		if (variable.Type!.IsPack) return unit.GetVariableValue(variable);

		return new GetVariableInstruction(unit, variable, mode).Add();
	}

	public static Result GetVariable(Unit unit, Variable variable, AccessMode mode)
	{
		if (Assembler.IsDebuggingEnabled) return GetVariableDebug(unit, variable, mode);

		if (variable.IsStatic || variable.IsInlined())
		{
			return new GetVariableInstruction(unit, variable, mode).Add();
		}

		return unit.GetVariableValue(variable);
	}

	public static Result GetConstant(Unit unit, NumberNode node)
	{
		return new GetConstantInstruction(unit, node.Value, node.Type.IsUnsigned(), node.Type.IsDecimal()).Add();
	}

	public static Result GetString(Unit unit, StringNode node)
	{
		var handle = new DataSectionHandle(node.GetIdentifier(unit), true);

		if (Assembler.UseIndirectAccessTables)
		{
			handle.Modifier = DataSectionModifier.GLOBAL_OFFSET_TABLE;
		}

		return new Result(handle, Assembler.Format);
	}

	public static Result GetDataPointer(DataPointer node)
	{
		if (node.Data is FunctionImplementation implementation)
		{
			var handle = new DataSectionHandle(implementation.GetFullname(), node.Offset, true);
			
			if (Assembler.UseIndirectAccessTables)
			{
				handle.Modifier = DataSectionModifier.GLOBAL_OFFSET_TABLE;
			}

			return new Result(handle, Assembler.Format);
		}

		if (node.Data is Table table)
		{
			var handle = new DataSectionHandle(table.Name, node.Offset, true);

			if (Assembler.UseIndirectAccessTables)
			{
				handle.Modifier = DataSectionModifier.GLOBAL_OFFSET_TABLE;
			}

			return new Result(handle, Assembler.Format);
		}

		throw new ApplicationException("Could not build data pointer");
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

			case NodeType.ACCESSOR:
			{
				return Accessors.Build(unit, (AccessorNode)node, mode);
			}

			case NodeType.LINK:
			{
				return Links.Build(unit, (LinkNode)node, mode);
			}

			case NodeType.PARENTHESIS:
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
				return new AllocateStackInstruction(unit, node.To<StackAddressNode>()).Add();
			}

			default:
			{
				return Builders.Build(unit, node);
			}
		}
	}
}