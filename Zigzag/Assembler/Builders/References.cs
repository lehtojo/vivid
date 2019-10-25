using System;

public enum ReferenceType
{
	DIRECT,
	READ,
	VALUE,
	REGISTER
}

public class References
{
	public static MemoryReference GetObjectPointer(Unit unit)
	{
		return MemoryReference.Parameter(unit, 0, 4);
	}

	private static Instructions GetFunctionReference(Unit unit, FunctionNode node, ReferenceType type)
	{
		if (type == ReferenceType.DIRECT)
		{
			Console.Error.WriteLine("ERROR: Writable function return values aren't supported");
		}

		return Call.Build(unit, node);
	}

	private static Instructions GetLinkReference(Unit unit, LinkNode node, ReferenceType type)
	{
		return Links.Build(unit, node, type);
	}

	/**
     * Returns a reference to a variable
     * @param node Variable as node
     * @param type Type of reference to get
     * @return Reference to a variable
     */
	private static Instructions GetVariableReference(Unit unit, VariableNode node, ReferenceType type)
	{
		Variable variable = node.Variable;

		if (type == ReferenceType.DIRECT)
		{
			unit.Reset(variable);
		}
		else
		{
			Reference cache = unit.IsCached(variable);

			if (cache != null)
			{
				// Variable may be cached in stack
				if (!cache.IsRegister())
				{
					Instructions move = Memory.ToRegister(unit, cache);
					return move.SetReference(global::Value.GetVariable(move.Reference, variable));
				}

				return Instructions.reference(cache);
			}
		}

		Instructions instructions = new Instructions();
		Reference reference;

		switch (variable.Category)
		{
			case VariableCategory.GLOBAL:
			{
				reference = ManualReference.Global(variable.Name, Size.Get(variable.Type.Size));
				break;
			}

			case VariableCategory.LOCAL:
			{
				reference = MemoryReference.Local(unit, variable.Alignment, variable.Type.Size);
				break;
			}

			case VariableCategory.PARAMETER:
			{
				reference = MemoryReference.Parameter(unit, variable.Alignment, variable.Type.Size);
				break;
			}

			case VariableCategory.MEMBER:
			{
				Register register = unit.GetObjectPointer();

				if (register == null)
				{
					Instructions move = Memory.GetObjectPointer(unit, type);
					instructions.Append(move);

					register = move.Reference.GetRegister();
				}

				reference = MemoryReference.Member(register, variable.Alignment, variable.Type.Size);
				break;
			}

			default:
			{
				return null;
			}
		}

		if ((type == ReferenceType.VALUE || type == ReferenceType.REGISTER) && reference.IsComplex())
		{
			Instructions move = Memory.ToRegister(unit, reference);
			instructions.Append(move);

			return instructions.SetReference(global::Value.GetVariable(move.Reference, variable));
		}

		return instructions.SetReference(reference);
	}

	private static Instructions GetNumberReference(Unit unit, NumberNode node, ReferenceType type)
	{
		Size size = Size.Get(node.Type.Size);

		switch (type)
		{
			case ReferenceType.DIRECT: return Instructions.reference(new AddressReference(node.Value));

			case ReferenceType.VALUE:
			case ReferenceType.READ: return Instructions.reference(new NumberReference(node.Value, size));

			case ReferenceType.REGISTER:
			{
				Instructions instructions = Memory.ToRegister(unit, new NumberReference(node.Value, size));
				instructions.SetReference(global::Value.GetNumber(instructions.Reference.GetRegister(), size));
				return instructions;
			}

		}

		return null;
	}

	private static Instructions GetStringReference(Unit unit, StringNode node, ReferenceType type)
	{
		switch (type)
		{

			case ReferenceType.DIRECT:
			{
				return Instructions.reference(ManualReference.String(node.Identifier, true));
			}

			case ReferenceType.READ:
			{
				return Instructions.reference(ManualReference.String(node.Identifier, false));
			}

			case ReferenceType.VALUE:
			case ReferenceType.REGISTER:
			{
				Reference reference = ManualReference.String(node.Identifier, false);
				Instructions instructions = Memory.ToRegister(unit, reference);

				return instructions.SetReference(global::Value.GetString(instructions.Reference.GetRegister()));
			}

			default: return null;
		}
	}

	public static Instructions Get(Unit unit, Node node, ReferenceType type)
	{
		switch (node.GetNodeType())
		{
			case NodeType.FUNCTION_NODE:
			{
				return GetFunctionReference(unit, (FunctionNode)node, type);
			}

			case NodeType.LINK_NODE:
			{
				return GetLinkReference(unit, (LinkNode)node, type);
			}

			case NodeType.VARIABLE_NODE:
			{
				return GetVariableReference(unit, (VariableNode)node, type);
			}

			case NodeType.NUMBER_NODE:
			{
				return GetNumberReference(unit, (NumberNode)node, type);
			}

			case NodeType.CONTENT_NODE:
			{
				return References.Get(unit, node.First, type);
			}

			case NodeType.STRING_NODE:
			{
				return GetStringReference(unit, (StringNode)node, type);
			}

			case NodeType.CAST_NODE:
			{
				return References.Get(unit, node.First, type);
			}

			case NodeType.OPERATOR_NODE:
			{
				OperatorNode @operator = (OperatorNode)node;

				if (@operator.Operator == Operators.EXTENDER)
				{
					return Arrays.Build(unit, @operator, type);
				}

				if (type == ReferenceType.DIRECT)
				{
					Console.WriteLine("Warning: Complex writable reference requested");
				}

				return unit.Assemble(node);
			}

			default:
			{
				if (type == ReferenceType.DIRECT)
				{
					Console.WriteLine("Warning: Complex writable reference requested");
				}

				return unit.Assemble(node);
			}
		}
	}

	public static Instructions Direct(Unit unit, Node node)
	{
		return References.Get(unit, node, ReferenceType.DIRECT);
	}

	public static Instructions Read(Unit unit, Node node)
	{
		return References.Get(unit, node, ReferenceType.READ);
	}

	public static Instructions Value(Unit unit, Node node)
	{
		return References.Get(unit, node, ReferenceType.VALUE);
	}

	public static Instructions Register(Unit unit, Node node)
	{
		return References.Get(unit, node, ReferenceType.REGISTER);
	}

	/**
     * Returns whether node is primitive (Requires only building references)
     */
	private static bool IsPrimitive(Node node)
	{
		return node.GetNodeType() == NodeType.VARIABLE_NODE || node.GetNodeType() == NodeType.STRING_NODE || node.GetNodeType() == NodeType.NUMBER_NODE;
	}

	/**
     * Returns references to both given nodes
     * @param program Program to append the instructions for referencing the nodes
     * @param a Node a
     * @param b Node b
     * @param at Node a's reference type
     * @param bt Node a'b reference type
     * @return References to both of the nodes
     */
	public static Reference[] Get(Unit unit, Instructions program, Node a, Node b, ReferenceType at, ReferenceType bt)
	{
		Reference[] references = new Reference[2];

		if (!IsPrimitive(a))
		{
			Instructions instructions = References.Get(unit, a, at);
			references[0] = instructions.Reference;

			program.Append(instructions);
		}

		if (!IsPrimitive(b))
		{
			Instructions instructions = References.Get(unit, b, bt);
			references[1] = instructions.Reference;

			program.Append(instructions);
		}

		if (references[0] == null)
		{
			Instructions instructions = References.Get(unit, a, at);
			references[0] = instructions.Reference;

			program.Append(instructions);
		}

		if (references[1] == null)
		{
			Instructions instructions = References.Get(unit, b, bt);
			references[1] = instructions.Reference;

			program.Append(instructions);
		}

		return references;
	}
}