using System.Collections.Generic;

public static class Map
{
	public static Dictionary<K, V> Of<K, V>(K k1, V v1)
	{
		return new Dictionary<K, V>(new KeyValuePair<K, V>[]
		{
			new KeyValuePair<K, V>(k1, v1)
		});
	}
	public static Dictionary<K, V> Of<K, V>(K k1, V v1, K k2, V v2)
	{
		return new Dictionary<K, V>(new KeyValuePair<K, V>[]
		{
			new KeyValuePair<K, V>(k1, v1),
			new KeyValuePair<K, V>(k2, v2)
		});
	}

	public static Dictionary<K, V> Of<K, V>(K k1, V v1, K k2, V v2, K k3, V v3)
	{
		return new Dictionary<K, V>(new KeyValuePair<K, V>[]
		{
			new KeyValuePair<K, V>(k1, v1),
			new KeyValuePair<K, V>(k2, v2),
			new KeyValuePair<K, V>(k3, v3),
		});
	}
}

public class Unit
{
	public Register EAX { get; private set; }
	public Register EBX { get; private set; }
	public Register ECX	{ get; private set; }
	public Register EDX	{ get; private set; }
	public Register ESI	{ get; private set; }
	public Register EDI	{ get; private set; }
	public Register EBP	{ get; private set; }
	public Register ESP	{ get; private set; }

	public List<Register> Registers { get; private set; } = new List<Register>();

	public string Prefix { get; private set; }
	public string NextLabel => $"{Prefix}_L{Counter++}";

	private int Counter = 1;

	public Unit(string prefix)
	{
		this.Prefix = prefix;

		EAX = new Register(Map.Of(Size.DWORD, "eax", Size.WORD, "ax", Size.BYTE, "al"));
		EBX = new Register(Map.Of(Size.DWORD, "ebx", Size.WORD, "bx", Size.BYTE, "bl"));
		ECX = new Register(Map.Of(Size.DWORD, "ecx", Size.WORD, "cx", Size.BYTE, "cl"));
		EDX = new Register(Map.Of(Size.DWORD, "edx", Size.WORD, "dx", Size.BYTE, "dl"));
		ESI = new Register(Map.Of(Size.DWORD, "esi"));
		EDI = new Register(Map.Of(Size.DWORD, "edi"));
		EBP = new Register(Map.Of(Size.DWORD, "ebp"));
		ESP = new Register(Map.Of(Size.DWORD, "esp"));

		Registers.AddRange(new Register[] { EAX, EBX, ECX, EDX, ESI, EDI });
	}

	/**
     * Clones an unit
     * @param unit Unit to clone
     */
	private Unit(Unit unit)
	{
		Prefix = unit.Prefix;
		Counter = unit.Counter;
		EAX = unit.EAX.Clone();
		EBX = unit.EBX.Clone();
		ECX = unit.ECX.Clone();
		EDX = unit.EDX.Clone();
		ESI = unit.ESI.Clone();
		EDI = unit.EDI.Clone();
		EBP = unit.EBP.Clone();
		ESP = unit.ESP.Clone();

		Registers.AddRange(new Register[] { EAX, EBX, ECX, EDX, ESI, EDI });
	}

	/**
     * @return True if any register doesn't hold a value, otherwise false
     */
	public bool IsAnyRegisterAvailable => Registers.Exists(r => r.IsAvailable);
	public bool IsAnyRegisterUncritical => Registers.Exists(r => !r.IsCritical);

	public Register GetNextRegister()
	{
		Register register = Registers.Find(r => r.IsAvailable);

		if (register != null)
		{
			return register;
		}

		register = Registers.Find(r => !r.IsCritical);

		if (register != null)
		{
			return register;
		}

		return Registers[0];
	}

	public bool IsObjectPointerLoaded(Register register)
	{
		return register.IsReserved && register.Value.Type == ValueType.OBJECT_POINTER;
	}

	public Register GetObjectPointer()
	{
		foreach (Register register in Registers)
		{
			if (register.IsReserved && register.Value.Type == ValueType.OBJECT_POINTER)
			{
				return register;
			}
		}

		return null;
	}

	public void Reset(Variable variable)
	{
		foreach (Register register in Registers)
		{
			if (register.Contains(variable))
			{
				register.Reset();
				break;
			}
		}
	}

	public void Reset()
	{
		foreach (Register register in Registers)
		{
			register.Reset();
		}
	}

	/**
     * Returns a possible cache reference to variable
     * @param variable Variable to look for
     * @return Success: Cache reference to the variable, Failure: null
     */
	public Reference IsCached(Variable variable)
	{
		foreach (Register register in Registers)
		{
			if (register.Contains(variable))
			{
				return register.Value;
			}
		}

		return null;
	}

	/**
     * Returns a possible register cache to variable
     * @param variable Variable to look for
     * @return Success: Register which contains the variable, Failure: null
     */
	public Register IsRegisterCached(Variable variable)
	{
		foreach (Register register in Registers)
		{
			if (register.Contains(variable))
			{
				return register;
			}
		}

		return null;
	}

	/**
     * Turns node tree structure into assembly
     * @param node Program represented in node tree form
     * @return Assembly representation of the node tree
     */
	public Instructions Assemble(Node node)
	{
		switch (node.GetNodeType())
		{

			case NodeType.OPERATOR_NODE:
			{
				OperatorNode @operator = (OperatorNode)node;

				switch (@operator.Operator.Type)
				{

					case OperatorType.CLASSIC:
					return Classic.Build(this, (OperatorNode)node);

					case OperatorType.ACTION:
					return Assign.build(this, (OperatorNode)node);

					case OperatorType.INDEPENDENT:
					return Links.Build(this, (LinkNode)node, ReferenceType.READ);

					default:
					return null;
				}
			}

			case NodeType.FUNCTION_NODE:
			{
				return Call.Build(this, (FunctionNode)node);
			}

			case NodeType.CONSTRUCTION_NODE:
			{
				return Construction.Build(this, (ConstructionNode)node);
			}

			case NodeType.IF_NODE:
			{
				return Conditionals.start(this, (IfNode)node);
			}

			case NodeType.LOOP_NODE:
			{
				return Loop.Build(this, (LoopNode)node);
			}

			case NodeType.RETURN_NODE:
			{
				return Return.Build(this, (ReturnNode)node);
			}

			case NodeType.JUMP_NODE:
			{
				return Labels.Build(this, (JumpNode)node);
			}

			case NodeType.LABEL_NODE:
			{
				return Labels.Build(this, (LabelNode)node);
			}

			case NodeType.LINK_NODE:
			{
				return Links.Build(this, (LinkNode)node, ReferenceType.READ);
			}

			default:
			{
				Instructions bundle = new Instructions();
				Node iterator = node.First;

				while (iterator != null)
				{
					Instructions instructions = Assemble(iterator);

					if (instructions != null)
					{
						bundle.Append(instructions);
					}

					Step();

					iterator = iterator.Next;
				}

				return bundle;
			}
		}
	}

	public void Step()
	{
		foreach (Register register in Registers)
		{
			if (register.IsReserved)
			{
				Value value = register.Value;
				value.IsCritical = false;
			}
		}
	}
	
	public Unit Clone()
	{
		return new Unit(this);
	}
}