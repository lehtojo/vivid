using System;
using System.Linq;

public enum ReferenceType
{
	DIRECT,
	READ,
	VALUE,
	REGISTER,
	DEFAULT
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

	private static Instructions GetVariableReference(Unit unit, VariableNode node, ReferenceType type)
	{
		return GetVariableReference(unit, node.Variable, type);
	}

	public static Instructions GetVariableReference(Unit unit, Variable variable, ReferenceType type)
	{
		if (type == ReferenceType.DIRECT)
		{
			unit.Reset(variable);
		}
		else
		{
			var cache = unit.IsCached(variable);

			if (cache != null)
			{
				// Variable may be cached in stack
				if (!cache.IsRegister())
				{
					var move = Memory.ToRegister(unit, cache);
					return move.SetReference(global::Value.GetVariable(move.Reference, variable));
				}

				return Instructions.GetReference(cache);
			}
		}

		var instructions = new Instructions();

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
				var register = unit.GetObjectPointer();

				if (register == null)
				{
					var move = Memory.GetObjectPointer(unit, type);
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

		reference.Metadata = variable;

		if ((type == ReferenceType.VALUE || type == ReferenceType.REGISTER) && reference.IsComplex())
		{
			var move = Memory.ToRegister(unit, reference);
			instructions.Append(move);

			return instructions.SetReference(global::Value.GetVariable(move.Reference, variable));
		}

		return instructions.SetReference(reference);
	}

	private static Instructions GetNumberReference(Unit unit, NumberNode node, ReferenceType type)
	{
		var size = Size.Get(node.Type.Size);

		switch (type)
		{
			case ReferenceType.DIRECT: return Instructions.GetReference(new AddressReference(node.Value));

			case ReferenceType.VALUE:
			case ReferenceType.READ: return Instructions.GetReference(new NumberReference(node.Value, size));

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
				return Instructions.GetReference(ManualReference.String(node.Identifier, true));
			}

			case ReferenceType.READ:
			{
				return Instructions.GetReference(ManualReference.String(node.Identifier, false));
			}

			case ReferenceType.VALUE:
			case ReferenceType.REGISTER:
			{
				var reference = ManualReference.String(node.Identifier, false);
				var instructions = Memory.ToRegister(unit, reference);

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
				var operation = node as OperatorNode;

				if (operation.Operator == Operators.EXTENDER)
				{
					return Arrays.Build(unit, operation, type);
				}

				if (type == ReferenceType.DIRECT)
				{
					Console.WriteLine("Warning: Complex writable reference requested");
				}

				return unit.Assemble(node, type);
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

	private const int FUNCTION_WEIGHT = 2;
	private const int DEFAULT_WEIGHT = 1;

	private static int GetWeight(Node node)
	{
		return node.Select(n =>
		{
			switch (n.GetNodeType())
			{
				case NodeType.CONSTRUCTION_NODE: return FUNCTION_WEIGHT;
				case NodeType.FUNCTION_NODE: return FUNCTION_WEIGHT;
				case NodeType.INLINE_NODE: return FUNCTION_WEIGHT;

				default: return DEFAULT_WEIGHT;
			}
		}).Sum();
	}

	/// <summary>
	/// Returns references to the nodes
	/// </summary>
	/// <param name="unit">Unit used to operate</param>
	/// <param name="program">Instructions where the operations are appended</param>
	/// <param name="n1">First node</param>
	/// <param name="n2">Second node</param>
	/// <param name="r1t">Type of reference to get for the first node</param>
	/// <param name="r2t">Type of reference to get for the second node</param>
	/// <param name="r1">Result: Reference for the first node</param>
	/// <param name="r2">Result: Reference for the second node</param>
	public static void Get(Unit unit, Instructions program, Node n1, Node n2, ReferenceType r1t, ReferenceType r2t, out Reference r1, out Reference r2)
	{
		r1 = null;
		r2 = null;

		var w1 = GetWeight(n1);
		var w2 = GetWeight(n2);

		if (w1 >= w2)
		{
			var instructions = References.Get(unit, n1, r1t);
			r1 = instructions.Reference;

			program.Append(instructions);

			instructions = References.Get(unit, n2, r2t);
			r2 = instructions.Reference;

			program.Append(instructions);
		}
		else
		{
			var instructions = References.Get(unit, n2, r2t);
			r2 = instructions.Reference;

			program.Append(instructions);

			instructions = References.Get(unit, n1, r1t);
			r1 = instructions.Reference;

			program.Append(instructions);
		}
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
		var references = new Reference[2];

		if (!IsPrimitive(a))
		{
			var instructions = References.Get(unit, a, at);
			references[0] = instructions.Reference;

			program.Append(instructions);
		}

		if (!IsPrimitive(b))
		{
			var instructions = References.Get(unit, b, bt);
			references[1] = instructions.Reference;

			program.Append(instructions);
		}

		if (references[0] == null)
		{
			var instructions = References.Get(unit, a, at);
			references[0] = instructions.Reference;

			program.Append(instructions);
		}

		if (references[1] == null)
		{
			var instructions = References.Get(unit, b, bt);
			references[1] = instructions.Reference;

			program.Append(instructions);
		}

		return references;
	}
}