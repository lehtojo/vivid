using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

public enum LabelUsageType
{
	LABEL,
	CALL
}

public struct LabelUsageItem
{
	public LabelUsageType Type { get; set; }
	public int Position { get; set; }
	public Label Label { get; set; }
	public DataSectionModifier Modifier { get; set; }

	public LabelUsageItem(LabelUsageType type, int position, Label label)
	{
		Type = type;
		Position = position;
		Label = label;
		Modifier = DataSectionModifier.NONE;
	}
}

public struct LabelDescriptor
{
	public EncoderModule Module { get; set; }
	public int Position { get; set; }
	public int AbsolutePosition => Module.Start + Position;

	public LabelDescriptor(EncoderModule module, int position)
	{
		Module = module;
		Position = position;
	}
}

public struct EncoderDebugLineInformation
{
	public int Offset { get; set; }
	public int Line { get; set; }
	public int Character { get; set; }

	public EncoderDebugLineInformation(int offset, int line, int character)
	{
		Offset = offset;
		Line = line;
		Character = character;
	}
}

public enum EncoderDebugFrameInformationType
{
	START,
	SET_FRAME_OFFSET,
	ADVANCE,
	END
}

public class EncoderDebugFrameInformation
{
	public EncoderDebugFrameInformationType Type { get; }
	public int Offset { get; }

	public EncoderDebugFrameInformation(EncoderDebugFrameInformationType type, int offset)
	{
		Type = type;
		Offset = offset;
	}

	public T To<T>() where T : EncoderDebugFrameInformation
	{
		return (T)this;
	}
}

public class EncoderDebugFrameStartInformation : EncoderDebugFrameInformation
{
	public string Symbol { get; }

	public EncoderDebugFrameStartInformation(int offset, string symbol) : base(EncoderDebugFrameInformationType.START, offset)
	{
		Symbol = symbol;
	}
}

public class EncoderDebugFrameOffsetInformation : EncoderDebugFrameInformation
{
	public int FrameOffset { get; }

	public EncoderDebugFrameOffsetInformation(int offset, int frame_offset) : base(EncoderDebugFrameInformationType.SET_FRAME_OFFSET, offset)
	{
		FrameOffset = frame_offset;
	}
}

public class EncoderModule
{
	public int Index { get; set; } = 0;
	public Label? Jump { get; set; } = null;
	public bool IsConditionalJump { get; set; } = false;
	public bool IsShortJump { get; set; } = false;
	public List<Instruction> Instructions { get; } = new List<Instruction>();
	public List<LabelUsageItem> Labels { get; } = new List<LabelUsageItem>();
	public List<LabelUsageItem> Calls { get; } = new List<LabelUsageItem>();
	public List<LabelUsageItem> Items { get; } = new List<LabelUsageItem>();
	public List<BinaryRelocation> MemoryAddressRelocations { get; } = new List<BinaryRelocation>();
	public List<EncoderDebugLineInformation> DebugLineInformation { get; } = new List<EncoderDebugLineInformation>();
	public List<EncoderDebugFrameInformation> DebugFrameInformation { get; } = new List<EncoderDebugFrameInformation>();
	public byte[] Output { get; set; } = Array.Empty<byte>();
	public int Position { get; set; } = 0;
	public int Start { get; set; } = 0;

	public EncoderModule(Label jump, bool conditional)
	{
		Jump = jump;
		IsConditionalJump = conditional;
	}

	public EncoderModule() {}
}

public struct EncoderOutput
{
	public BinarySection Section { get; set; }
	public Dictionary<string, BinarySymbol> Symbols { get; }
	public List<BinaryRelocation> Relocations { get; }
	public DebugFrameEncoderModule? Frames { get; }
	public DebugLineEncoderModule? Lines { get; }

	public EncoderOutput(BinarySection section, Dictionary<string, BinarySymbol> symbols, List<BinaryRelocation> relocations, DebugFrameEncoderModule? frames, DebugLineEncoderModule? lines)
	{
		Section = section;
		Symbols = symbols;
		Relocations = relocations;
		Frames = frames;
		Lines = lines;
	}
}

public enum EncodingFilterType
{
	REGISTER,
	STANDARD_REGISTER,
	MEDIA_REGISTER,
	SPECIFIC_REGISTER,
	MEMORY_ADDRESS,
	CONSTANT,
	SPECIFIC_CONSTANT,
	SIGNLESS_CONSTANT,
	EXPRESSION,
	LABEL
}

public enum EncodingRoute
{
	RMC, // Register, Memory, Constant
	RRC, // Register, Register, Constant
	DRC, // Register, Constant => Register, Register, Constant
	RR, // Register, Register
	RC, // Register, Constant
	RM, // Register, Memory address
	MR, // Memory address, Register
	OC, // Operation code + Register, Constant
	MC, // Memory address, Constant
	R, // Register
	M, // Memory
	C, // Constant
	O, // Operation code + Register
	D, // Label offset
	L, // Label declaration
	SC, // Skip, Constant
	SO, // Skip, Operation code + Register
	NONE
}

public struct InstructionEncoding
{
	public byte Prefix { get; set; }
	public bool Is64Bit { get; set; }
	public int Operation { get; set; }
	public byte Modifier { get; set; }
	public EncodingRoute Route { get; set; }
	public EncodingFilterType FilterTypeOfFirst { get; set; }
	public short FilterOfFirst { get; set; }
	public byte InputSizeOfFirst { get; set; }
	public EncodingFilterType FilterTypeofSecond { get; set; }
	public short FilterOfSecond { get; set; }
	public byte InputSizeOfSecond { get; set; }
	public EncodingFilterType FilterTypeOfThird { get; set; }
	public short FilterOfThird { get; set; }
	public byte InputSizeOfThird { get; set; }

	public InstructionEncoding(int operation, EncodingRoute route = EncodingRoute.NONE, bool rex = false)
	{
		Prefix = 0;
		Is64Bit = rex;
		Operation = operation;
		Modifier = 0;
		Route = route;
		FilterTypeOfFirst = 0;
		FilterOfFirst = 0;
		InputSizeOfFirst = 0;
		FilterTypeofSecond = 0;
		FilterOfSecond = 0;
		InputSizeOfSecond = 0;
		FilterTypeOfThird = 0;
		FilterOfThird = 0;
		InputSizeOfThird = 0;
	}

	public InstructionEncoding(int operation, byte modifier, EncodingRoute route, bool rex, EncodingFilterType filter_type_first, short filter_first, byte input_size_first, byte prefix = 0)
	{
		Prefix = prefix;
		Is64Bit = rex;
		Operation = operation;
		Modifier = modifier;
		Route = route;
		FilterTypeOfFirst = filter_type_first;
		FilterOfFirst = filter_first;
		InputSizeOfFirst = input_size_first;
		FilterTypeofSecond = 0;
		FilterOfSecond = 0;
		InputSizeOfSecond = 0;
		FilterTypeOfThird = 0;
		FilterOfThird = 0;
		InputSizeOfThird = 0;
	}

	public InstructionEncoding(int operation, byte modifier, EncodingRoute route, bool rex,
		EncodingFilterType filter_type_first, short filter_first, byte input_size_first,
		EncodingFilterType filter_type_second, short filter_second, byte input_size_second, byte prefix = 0)
	{
		Prefix = prefix;
		Is64Bit = rex;
		Operation = operation;
		Modifier = modifier;
		Route = route;
		FilterTypeOfFirst = filter_type_first;
		FilterOfFirst = filter_first;
		InputSizeOfFirst = input_size_first;
		FilterTypeofSecond = filter_type_second;
		FilterOfSecond = filter_second;
		InputSizeOfSecond = input_size_second;
		FilterTypeOfThird = 0;
		FilterOfThird = 0;
		InputSizeOfThird = 0;
	}

	public InstructionEncoding(int operation, byte modifier, EncodingRoute route, bool rex,
		EncodingFilterType filter_type_first, short filter_first, byte input_size_first,
		EncodingFilterType filter_type_second, short filter_second, byte input_size_second,
		EncodingFilterType filter_type_third, short filter_third, byte input_size_third, byte prefix = 0)
	{
		Prefix = prefix;
		Is64Bit = rex;
		Operation = operation;
		Modifier = modifier;
		Route = route;
		FilterTypeOfFirst = filter_type_first;
		FilterOfFirst = filter_first;
		InputSizeOfFirst = input_size_first;
		FilterTypeofSecond = filter_type_second;
		FilterOfSecond = filter_second;
		InputSizeOfSecond = input_size_second;
		FilterTypeOfThird = filter_type_third;
		FilterOfThird = filter_third;
		InputSizeOfThird = input_size_third;
	}
}

public struct MemoryAddressDescriptor
{
	public Register? Start { get; set; }
	public Register? Index { get; set; }
	public int Stride { get; set; }
	public int Offset { get; set; }
	public BinaryRelocation? Relocation { get; set; }

	public MemoryAddressDescriptor(Register? start, Register? index, int stride, int offset)
	{
		Start = start;
		Index = index;
		Stride = stride;
		Offset = offset;
		Relocation = null;
	}

	public MemoryAddressDescriptor(string symbol, DataSectionModifier modifier, long offset)
	{
		Start = null;
		Index = null;
		Stride = 0;
		Offset = 0;
		Relocation = new BinaryRelocation(new BinarySymbol(symbol, 0, true), 0, (int)offset, modifier);
	}
}

public static class InstructionEncoder
{
	public const int MAX_INSTRUCTION_SIZE = 15;

	public const byte LOCK_PREFIX = 240; // 0xf0
	public const byte OPERAND_SIZE_OVERRIDE = 102; // 0x66

	public const byte REX_PREFIX = 64; // 01000000
	public const byte REX_W = 8; // 00001000
	public const byte REX_R = 4; // 00000100
	public const byte REX_X = 2; // 00000010
	public const byte REX_B = 1; // 00000001

	public const byte MEMORY_OFFSET8_MODIFIER = 64; // 01000000
	public const byte MEMORY_OFFSET32_MODIFIER = 128; // 10000000
	public const byte REGISTER_DIRECT_ADDRESSING_MODIFIER = 192; // 11000000

	public const int JUMP_OFFSET8_SIZE = 2;
	public const int CONDITIONAL_JUMP_OFFSET8_SIZE = 2;
	public const int JUMP_OFFSET32_SIZE = 5;
	public const int CONDITIONAL_JUMP_OFFSET32_SIZE = 6;
	public const int JUMP_OFFSET8_OPERATION_CODE = 0xEB;

	public const string TEMPORARY_ASSEMBLY_FILE = "temporary.asm";

	/// <summary>
	/// Returns whether the specified register needs the REX-prefix
	/// </summary>
	public static bool IsExtensionRegister(Register register)
	{
		return register.Identifier >= Instructions.X64.R8;
	}

	/// <summary>
	/// Returns whether the specified register needs the REX-prefix
	/// </summary>
	public static bool IsExtensionRegister(int identifier)
	{
		return identifier >= Instructions.X64.R8;
	}

	/// <summary>
	/// Returns whether the specified register can be overriden to represent another register using the REX-prefix
	/// </summary>
	public static bool IsOverridableRegister(int register, int size)
	{
		return size == 1 && register >= Instructions.X64.RSP && register <= Instructions.X64.RDI;
	}

	/// <summary>
	/// Returns whether the specified register can be overriden to represent another register using the REX-prefix
	/// </summary>
	public static bool IsOverridableRegister(Register register, int size)
	{
		return IsOverridableRegister(register.Identifier, size);
	}

	/// <summary>
	/// Writes the specified value to the current position and advances to the next position
	/// </summary>
	public static void Write(EncoderModule module, long value)
	{
		module.Output[module.Position++] = (byte)value;
	}

	/// <summary>
	/// Writes the specified value to the specified position
	/// </summary>
	public static void Write(EncoderModule module, int position, long value)
	{
		module.Output[position] = (byte)value;
	}

	/// <summary>
	/// Writes the specified value to the current position and advances to the next position
	/// </summary>
	public static void WriteInt16(EncoderModule module, long value)
	{
		module.Output[module.Position++] = (byte)(value & 0xFF);
		module.Output[module.Position++] = (byte)((value & 0xFF00) >> 8);
	}

	/// <summary>
	/// Writes the specified value to the current position and advances to the next position
	/// </summary>
	public static void WriteInt32(EncoderModule module, long value)
	{
		module.Output[module.Position++] = (byte)(value & 0xFF);
		module.Output[module.Position++] = (byte)((value & 0xFF00) >> 8);
		module.Output[module.Position++] = (byte)((value & 0xFF0000) >> 16);
		module.Output[module.Position++] = (byte)((value & 0xFF000000) >> 24);
	}

	/// <summary>
	/// Writes the specified value to the specified position
	/// </summary>
	public static void WriteInt32(EncoderModule module, int position, long value)
	{
		module.Output[position++] = (byte)(value & 0xFF);
		module.Output[position++] = (byte)((value & 0xFF00) >> 8);
		module.Output[position++] = (byte)((value & 0xFF0000) >> 16);
		module.Output[position++] = (byte)((value & 0xFF000000) >> 24);
	}

	/// <summary>
	/// Writes the specified value to the current position and advances to the next position
	/// </summary>
	public static void WriteInt64(EncoderModule module, long value)
	{
		module.Output[module.Position++] = (byte)(value & 0xFF);
		module.Output[module.Position++] = (byte)((value & 0xFF00) >> 8);
		module.Output[module.Position++] = (byte)((value & 0xFF0000) >> 16);
		module.Output[module.Position++] = (byte)((value & 0xFF000000) >> 24);
		module.Output[module.Position++] = (byte)((value & 0xFF00000000) >> 32);
		module.Output[module.Position++] = (byte)((value & 0xFF0000000000) >> 40);
		module.Output[module.Position++] = (byte)((value & 0xFF000000000000) >> 48);
		module.Output[module.Position++] = (byte)(((ulong)value & 0xFF00000000000000) >> 56);
	}

	/// <summary>
	/// Writes the specified operation code
	/// </summary>
	public static void WriteOperation(EncoderModule module, int operation)
	{
		var next = operation & 0xFF;
		Write(module, next);

		next = operation & 0xFF00;
		if (next == 0) return;
		Write(module, next >> 8);

		next = operation & 0xFF0000;
		if (next == 0) return;
		Write(module, next >> 16);
	}

	/// <summary>
	/// Writes a REX-prefix if it is needed depending on the specified flags
	/// </summary>
	public static void TryWriteRex(EncoderModule module, bool w, bool r, bool x, bool b, bool force)
	{
		// Write the REX-prefix only if any of the flags in enabled
		var flags = (w ? REX_W : 0) | (r ? REX_R : 0) | (x ? REX_X : 0) | (b ? REX_B : 0);
		if (flags == 0 && !force) return;

		Write(module, REX_PREFIX | flags);
	}

	/// <summary>
	/// Writes a SIB-byte, which contains scale, index and base parameters
	/// </summary>
	public static void WriteSIB(EncoderModule module, int scale, int index, int start)
	{
		Write(module, (int)Math.Log2(scale) << 6 | index << 3 | start);
	}

	/// <summary>
	/// Writes a SIB-byte, which contains scale, index and base parameters
	/// </summary>
	public static void WriteSIB(EncoderModule module, int scale, int index, Register start)
	{
		WriteSIB(module, scale, index, start.Name);
	}

	/// <summary>
	/// Writes a SIB-byte, which contains scale, index and base parameters
	/// </summary>
	public static void WriteSIB(EncoderModule module, int scale, Register index, Register start)
	{
		WriteSIB(module, scale, index.Name, start.Name);
	}

	/// <summary>
	/// Writes a register and a second register using the modrm-byte. Uses register-direct addressing mode. 
	/// </summary>
	public static void WriteRegisterAndRegister(EncoderModule module, InstructionEncoding encoding, Register first, Register second)
	{
		var force = IsOverridableRegister(first, encoding.InputSizeOfFirst) || IsOverridableRegister(second, encoding.InputSizeOfSecond);
		TryWriteRex(module, encoding.Is64Bit, IsExtensionRegister(first), false, IsExtensionRegister(second), force);
		WriteOperation(module, encoding.Operation);
		Write(module, REGISTER_DIRECT_ADDRESSING_MODIFIER | first.Name << 3 | second.Name);
	}

	/// <summary>
	/// Writes a single register using the modrm-byte. Uses register-direct addressing mode. 
	/// </summary>
	public static void WriteSingleRegister(EncoderModule module, InstructionEncoding encoding, Register first)
	{
		var force = IsOverridableRegister(first, encoding.InputSizeOfFirst);
		TryWriteRex(module, encoding.Is64Bit, false, false, IsExtensionRegister(first), force);
		WriteOperation(module, encoding.Operation);
		Write(module, REGISTER_DIRECT_ADDRESSING_MODIFIER | encoding.Modifier << 3 | first.Name);
	}

	/// <summary>
	/// Writes a register and a constant using the modrm-byte. The register is encoded into the rm-field.
	/// </summary>
	public static void WriteRegisterAndConstant(EncoderModule module, InstructionEncoding encoding, Register first, long second, int size)
	{
		var force = IsOverridableRegister(first, encoding.InputSizeOfFirst);
		TryWriteRex(module, encoding.Is64Bit, false, false, IsExtensionRegister(first), force);

		WriteOperation(module, encoding.Operation);
		Write(module, REGISTER_DIRECT_ADDRESSING_MODIFIER | encoding.Modifier << 3 | first.Name);

		if (size == 1) Write(module, second);
		else if (size == 2) WriteInt16(module, second);
		else if (size == 4) WriteInt32(module, second);
		else if (size == 8) WriteInt64(module, second);
		else { throw new ApplicationException("Invalid constant size"); }
	}

	/// <summary>
	/// Writes a constant directly
	/// </summary>
	public static void WriteRawConstant(EncoderModule module, long value, int size)
	{
		if (size == 1) Write(module, value);
		else if (size == 2) WriteInt16(module, value);
		else if (size == 4) WriteInt32(module, value);
		else if (size == 8) WriteInt64(module, value);
		else { throw new ApplicationException("Invalid constant size"); }
	}

	/// <summary>
	/// Defines the specified symbol and writes a modrm-byte which uses the symbol and the specified register 'first'
	/// </summary>
	private static void WriteRegisterAndSymbol(EncoderModule module, InstructionEncoding encoding, int first, BinaryRelocation relocation)
	{
		var force = encoding.Modifier == 0 && IsOverridableRegister(first, encoding.InputSizeOfFirst);
		TryWriteRex(module, encoding.Is64Bit, IsExtensionRegister(first), false, false, force);

		WriteOperation(module, encoding.Operation);
		Write(module, (first & 7) << 3 | Instructions.X64.RBP); // Addressing: [rip+c32]

		relocation.Offset = module.Position;
		module.MemoryAddressRelocations.Add(relocation);

		WriteInt32(module, 0); // Fill the offset with zero
	}

	/// <summary>
	/// Writes register and memort address operands
	/// </summmary>
	public static void WriteRegisterAndMemoryAddress(EncoderModule module, InstructionEncoding encoding, int first, Register start, int offset)
	{
		#warning The register might also be the second operand
		var force = encoding.Modifier == 0 && IsOverridableRegister(first, encoding.InputSizeOfFirst);
		TryWriteRex(module, encoding.Is64Bit, IsExtensionRegister(first), false, IsExtensionRegister(start), force);

		WriteOperation(module, encoding.Operation);

		// Convert [start+offset] => [start]
		/// NOTE: Do not use this conversion if the start register is either RBP (0.101) or R13 (1.101)
		if (offset == 0 && start.Name != Instructions.X64.RBP && start.Name != Instructions.X64.RSP)
		{
			Write(module, (first & 7) << 3 | start.Name);
			return;
		}

		if (offset < sbyte.MinValue || offset > sbyte.MaxValue)
		{
			Write(module, MEMORY_OFFSET32_MODIFIER | (first & 7) << 3 | start.Name);

			// If the name of the register matches the name of the stack pointer, a SIB-byte is required to express the register
			if (start.Name == Instructions.X64.RSP) WriteSIB(module, 0, Instructions.X64.RSP, Instructions.X64.RSP);

			WriteInt32(module, offset);
			return;
		}

		Write(module, MEMORY_OFFSET8_MODIFIER | (first & 7) << 3 | start.Name);

		// If the name of the register matches the name of the stack pointer, a SIB-byte is required to express the register
		if (start.Name == Instructions.X64.RSP) WriteSIB(module, 0, Instructions.X64.RSP, Instructions.X64.RSP);

		Write(module, offset);
	}

	/// <summary>
	/// Writes register and memort address operands
	/// </summmary>
	public static void WriteRegisterAndMemoryAddress(EncoderModule module, InstructionEncoding encoding, int first, Register start, Register index, int scale, int offset)
	{
		// Convert [start+index*0+offset] => [start+offset]
		if (scale == 0)
		{
			WriteRegisterAndMemoryAddress(module, encoding, first, start, offset);
			return;
		}

		var force = encoding.Modifier == 0 && IsOverridableRegister(first, encoding.InputSizeOfFirst);
		TryWriteRex(module, encoding.Is64Bit, IsExtensionRegister(first), IsExtensionRegister(index), IsExtensionRegister(start), force);

		WriteOperation(module, encoding.Operation);

		// If the start register is RBP or R13, it is a special case where the offset must be added even though it is zero
		if (offset == 0 && start.Name != Instructions.X64.RBP)
		{
			Write(module, (first & 7) << 3 | Instructions.X64.RSP);
			WriteSIB(module, scale, index, start);
			return;
		}

		if (offset < sbyte.MinValue || offset > sbyte.MaxValue)
		{
			Write(module, MEMORY_OFFSET32_MODIFIER | (first & 7) << 3 | Instructions.X64.RSP);
			WriteSIB(module, scale, index, start);
			WriteInt32(module, offset);
			return;
		}

		Write(module, MEMORY_OFFSET8_MODIFIER | (first & 7) << 3 | Instructions.X64.RSP);
		WriteSIB(module, scale, index, start);
		Write(module, offset);
	}

	/// <summary>
	/// Writes register and memort address operands
	/// </summmary>
	public static void WriteRegisterAndMemoryAddress(EncoderModule module, InstructionEncoding encoding, int first, Register index, int scale, int offset)
	{
		// Convert [index*0+offset] => [offset]
		if (scale == 0)
		{
			WriteRegisterAndMemoryAddress(module, encoding, first, offset);
			return;
		}

		// Convert [index*1+offset] => [index+offset]
		if (scale == 1)
		{
			WriteRegisterAndMemoryAddress(module, encoding, first, index, offset);
			return;
		}

		var force = encoding.Modifier == 0 && IsOverridableRegister(first, encoding.InputSizeOfFirst);
		TryWriteRex(module, encoding.Is64Bit, IsExtensionRegister(first), IsExtensionRegister(index), false, force);

		WriteOperation(module, encoding.Operation);

		Write(module, (first & 7) << 3 | Instructions.X64.RSP);
		WriteSIB(module, scale, index.Name, Instructions.X64.RBP);
		WriteInt32(module, offset);
	}

	/// <summary>
	/// Writes register and memort address operands
	/// </summmary>
	public static void WriteRegisterAndMemoryAddress(EncoderModule module, InstructionEncoding encoding, int first, int offset)
	{
		var force = encoding.Modifier == 0 && IsOverridableRegister(first, encoding.InputSizeOfFirst);
		TryWriteRex(module, encoding.Is64Bit, IsExtensionRegister(first), false, false, force);

		WriteOperation(module, encoding.Operation);
		Write(module, (first & 7) << 3 | Instructions.X64.RSP);
		WriteSIB(module, 0, Instructions.X64.RSP, Instructions.X64.RBP);
		WriteInt32(module, offset);
	}

	/// <summary>
	/// Returns a object that describes the specified memory address
	/// </summary>
	public static MemoryAddressDescriptor GetMemoryAddressDescriptor(Handle handle)
	{
		return handle.Instance switch
		{
			HandleInstanceType.MEMORY => new MemoryAddressDescriptor(handle.To<MemoryHandle>().GetStart(), null, 0, handle.To<MemoryHandle>().GetOffset()),
			HandleInstanceType.COMPLEX_MEMORY => new MemoryAddressDescriptor(handle.To<ComplexMemoryHandle>().GetStart(), handle.To<ComplexMemoryHandle>().GetIndex(), handle.To<ComplexMemoryHandle>().Stride, handle.To<ComplexMemoryHandle>().GetOffset()),
			HandleInstanceType.EXPRESSION => new MemoryAddressDescriptor(handle.To<ExpressionHandle>().GetStart(), handle.To<ExpressionHandle>().GetIndex(), handle.To<ExpressionHandle>().Multiplier, handle.To<ExpressionHandle>().GetOffset()),
			HandleInstanceType.INLINE => new MemoryAddressDescriptor(handle.To<InlineHandle>().Unit.GetStackPointer(), null, 1, handle.To<InlineHandle>().AbsoluteOffset),
			HandleInstanceType.STACK_MEMORY => new MemoryAddressDescriptor(handle.To<StackMemoryHandle>().GetStart(), null, 1, handle.To<StackMemoryHandle>().GetOffset()),
			HandleInstanceType.DATA_SECTION => new MemoryAddressDescriptor(handle.To<DataSectionHandle>().Identifier, handle.To<DataSectionHandle>().Modifier, handle.To<DataSectionHandle>().Offset),
			HandleInstanceType.CONSTANT_DATA_SECTION => new MemoryAddressDescriptor(handle.To<ConstantDataSectionHandle>().Identifier, handle.To<DataSectionHandle>().Modifier, handle.To<DataSectionHandle>().Offset),
			HandleInstanceType.STACK_VARIABLE => new MemoryAddressDescriptor(handle.To<StackVariableHandle>().GetStart(), null, 1, handle.To<StackVariableHandle>().GetOffset()),
			HandleInstanceType.TEMPORARY_MEMORY => new MemoryAddressDescriptor(handle.To<StackMemoryHandle>().GetStart(), null, 1, handle.To<StackMemoryHandle>().GetOffset()),
			_ => throw new NotSupportedException("Unsupported handle")
		};
	}

	/// <summary>
	/// Returns whether the specified handle passes the configured filter
	/// </summary>
	private static bool PassesFilter(EncodingFilterType type, short filter, Handle value)
	{
		switch (type)
		{
			case EncodingFilterType.REGISTER: return value.Instance == HandleInstanceType.REGISTER;
			case EncodingFilterType.STANDARD_REGISTER: return value.Type == HandleType.REGISTER;
			case EncodingFilterType.MEDIA_REGISTER: return value.Type == HandleType.MEDIA_REGISTER;
			case EncodingFilterType.SPECIFIC_REGISTER: {
				if (value.Instance != HandleInstanceType.REGISTER) return false;
				return filter == value.To<RegisterHandle>().Register.Identifier;
			}
			case EncodingFilterType.MEMORY_ADDRESS: return value.Type == HandleType.MEMORY;
			case EncodingFilterType.CONSTANT: return value.Type == HandleType.CONSTANT;
			case EncodingFilterType.SPECIFIC_CONSTANT: {
				if (value.Instance != HandleInstanceType.CONSTANT) return false;
				return filter == (int)(long)value.To<ConstantHandle>().Value;
			}
			case EncodingFilterType.SIGNLESS_CONSTANT: return value.Type == HandleType.CONSTANT;
			case EncodingFilterType.EXPRESSION: return value.Type == HandleType.EXPRESSION;
			case EncodingFilterType.LABEL: return value.Instance == HandleInstanceType.DATA_SECTION && value.To<DataSectionHandle>().Address;
			default: return false;
		}
	}

	/// <summary>
	/// Returns how many bits are required for encoding the specified integer
	/// </summary>
	public static int GetNumberOfBitsForEncoding(long value)
	{
		if (value == long.MinValue) return 64;

		var x = (ulong)(value >= 0 ? value : -value);

		if (x > uint.MaxValue) return 64;
		else if (x > ushort.MaxValue) return 32;
		else if (x > byte.MaxValue) return 16;
		else return 8;
	}

	/// <summary>
	/// Returns whether the specified handle passes the configured filter
	/// </summary>
	private static bool PassesSize(Handle value, EncodingFilterType filter, short size)
	{
		if (value.Instance == HandleInstanceType.CONSTANT)
		{
			if (filter == EncodingFilterType.CONSTANT) return value.To<ConstantHandle>().Bits / 8 <= size;

			// Do not care about the sign, just verify all the bits can be stored in the specified size
			return GetNumberOfBitsForEncoding((long)value.To<ConstantHandle>().Value) / 8 <= size;
		}

		return value.Size.Bytes == size;
	}

	/// <summary>
	/// Finds an instruction encoding that takes none parameters and matches the specified type
	/// </summary>
	public static InstructionEncoding FindEncoding(int type)
	{
		var encodings = Instructions.X64.ParameterlessEncodings[type];
		if (encodings.Count == 0) throw new ApplicationException("Could not find instruction encoding");

		return encodings[0];
	}

	/// <summary>
	/// Finds an instruction encoding that takes one parameter and is suitable for the specified handle
	/// </summary>
	public static InstructionEncoding FindEncoding(int type, Handle first)
	{
		var encodings = Instructions.X64.SingleParameterEncodings[type];

		foreach (var encoding in encodings)
		{
			if (!PassesSize(first, encoding.FilterTypeOfFirst, encoding.InputSizeOfFirst)) continue;
			if (PassesFilter(encoding.FilterTypeOfFirst, encoding.FilterOfFirst, first)) return encoding;
		}

		throw new ApplicationException("Could not find instruction encoding");
	}

	/// <summary>
	/// Finds an instruction encoding that takes two parameters and is suitable for the specified handles
	/// </summary>
	public static InstructionEncoding FindEncoding(int type, Handle first, Handle second)
	{
		var encodings = Instructions.X64.DualParameterEncodings[type];

		foreach (var encoding in encodings)
		{
			if (!PassesSize(first, encoding.FilterTypeOfFirst, encoding.InputSizeOfFirst) || !PassesSize(second, encoding.FilterTypeofSecond, encoding.InputSizeOfSecond)) continue;
			if (!PassesFilter(encoding.FilterTypeOfFirst, encoding.FilterOfFirst, first) || !PassesFilter(encoding.FilterTypeofSecond, encoding.FilterOfSecond, second)) continue;
			return encoding;
		}

		throw new ApplicationException("Could not find instruction encoding");
	}

	/// <summary>
	/// Finds an instruction encoding that takes three parameters and is suitable for the specified handles
	/// </summary>
	public static InstructionEncoding FindEncoding(int type, Handle first, Handle second, Handle third)
	{
		var encodings = Instructions.X64.TripleParameterEncodings[type];

		foreach (var encoding in encodings)
		{
			if (!PassesSize(first, encoding.FilterTypeOfFirst, encoding.InputSizeOfFirst) || !PassesSize(second, encoding.FilterTypeofSecond, encoding.InputSizeOfSecond) || !PassesSize(third, encoding.FilterTypeOfThird, encoding.InputSizeOfThird)) continue;
			if (!PassesFilter(encoding.FilterTypeOfFirst, encoding.FilterOfFirst, first) || !PassesFilter(encoding.FilterTypeofSecond, encoding.FilterOfSecond, second) || !PassesFilter(encoding.FilterTypeOfThird, encoding.FilterOfThird, third)) continue;
			return encoding;
		}

		throw new ApplicationException("Could not find instruction encoding");
	}

	/// <summary>
	/// Returns the unique operation index of the specified instruction.
	/// This function will be removed, because instruction will use operation indices instead of text identifiers in the future.
	/// </summary>
	public static int GetInstructionIndex(Instruction instruction, string operation)
	{
		if (instruction.Type == InstructionType.LABEL) return Instructions.X64._LABEL;

		// Parameterless instructions
		if (operation == Instructions.Shared.RETURN) return Instructions.X64._RET;
		if (operation == Instructions.X64.EXTEND_QWORD) return Instructions.X64._CQO;
		if (operation == Instructions.X64.SYSTEM_CALL) return Instructions.X64._SYSCALL;
		if (operation == "fld1") return Instructions.X64._FLD1;
		if (operation == "fyl2x") return Instructions.X64._FYL2x;
		if (operation == "f2xm1") return Instructions.X64._F2XM1;
		if (operation == "faddp") return Instructions.X64._FADDP;
		if (operation == "fcos") return Instructions.X64._FCOS;
		if (operation == "fsin") return Instructions.X64._FSIN;
		if (operation == Instructions.Shared.NOP) return Instructions.X64._NOP;

		// Single parameter instructions
		if (operation == Instructions.X64.PUSH) return Instructions.X64._PUSH;
		if (operation == Instructions.X64.POP) return Instructions.X64._POP;
		if (operation == Instructions.X64.JUMP_ABOVE) return Instructions.X64._JA;
		if (operation == Instructions.X64.JUMP_ABOVE_OR_EQUALS) return Instructions.X64._JAE;
		if (operation == Instructions.X64.JUMP_BELOW) return Instructions.X64._JB;
		if (operation == Instructions.X64.JUMP_BELOW_OR_EQUALS) return Instructions.X64._JBE;
		if (operation == Instructions.X64.JUMP_EQUALS) return Instructions.X64._JE;
		if (operation == Instructions.X64.JUMP_GREATER_THAN) return Instructions.X64._JG;
		if (operation == Instructions.X64.JUMP_GREATER_THAN_OR_EQUALS) return Instructions.X64._JGE;
		if (operation == Instructions.X64.JUMP_LESS_THAN) return Instructions.X64._JL;
		if (operation == Instructions.X64.JUMP_LESS_THAN_OR_EQUALS) return Instructions.X64._JLE;
		if (operation == Instructions.X64.JUMP) return Instructions.X64._JMP;
		if (operation == Instructions.X64.JUMP_NOT_EQUALS) return Instructions.X64._JNE;
		if (operation == Instructions.X64.JUMP_NOT_ZERO) return Instructions.X64._JNZ;
		if (operation == Instructions.X64.JUMP_ZERO) return Instructions.X64._JZ;
		if (operation == Instructions.X64.CALL) return Instructions.X64._CALL;
		if (operation == "fild") return Instructions.X64._FILD;
		if (operation == "fld") return Instructions.X64._FLD;
		if (operation == "fistp") return Instructions.X64._FISTP;
		if (operation == "fstp") return Instructions.X64._FSTP;
		if (operation == Instructions.Shared.NEGATE) return Instructions.X64._NEG;
		if (operation == Instructions.X64.NOT) return Instructions.X64._NOT;
		if (operation == Instructions.X64.CONDITIONAL_SET_ABOVE) return Instructions.X64._SETA;
		if (operation == Instructions.X64.CONDITIONAL_SET_ABOVE_OR_EQUALS) return Instructions.X64._SETAE;
		if (operation == Instructions.X64.CONDITIONAL_SET_BELOW) return Instructions.X64._SETB;
		if (operation == Instructions.X64.CONDITIONAL_SET_BELOW_OR_EQUALS) return Instructions.X64._SETBE;
		if (operation == Instructions.X64.CONDITIONAL_SET_EQUALS) return Instructions.X64._SETE;
		if (operation == Instructions.X64.CONDITIONAL_SET_GREATER_THAN) return Instructions.X64._SETG;
		if (operation == Instructions.X64.CONDITIONAL_SET_GREATER_THAN_OR_EQUALS) return Instructions.X64._SETGE;
		if (operation == Instructions.X64.CONDITIONAL_SET_LESS_THAN) return Instructions.X64._SETL;
		if (operation == Instructions.X64.CONDITIONAL_SET_LESS_THAN_OR_EQUALS) return Instructions.X64._SETLE;
		if (operation == Instructions.X64.CONDITIONAL_SET_NOT_EQUALS) return Instructions.X64._SETNE;
		if (operation == Instructions.X64.CONDITIONAL_SET_NOT_ZERO) return Instructions.X64._SETNZ;
		if (operation == Instructions.X64.CONDITIONAL_SET_ZERO) return Instructions.X64._SETZ;

		// Dual parameter instructions
		if (operation == Instructions.Shared.MOVE) return Instructions.X64._MOV;
		if (operation == Instructions.Shared.ADD) return Instructions.X64._ADD;
		if (operation == Instructions.Shared.SUBTRACT) return Instructions.X64._SUB;
		if (operation == Instructions.X64.SIGNED_MULTIPLY) return Instructions.X64._IMUL;
		if (operation == Instructions.X64.UNSIGNED_MULTIPLY) return Instructions.X64._MUL;
		if (operation == Instructions.X64.SIGNED_DIVIDE) return Instructions.X64._IDIV;
		if (operation == Instructions.X64.UNSIGNED_DIVIDE) return Instructions.X64._DIV;
		if (operation == Instructions.X64.SHIFT_LEFT) return Instructions.X64._SAL;
		if (operation == Instructions.X64.SHIFT_RIGHT) return Instructions.X64._SAR;
		if (operation == Instructions.X64.UNSIGNED_CONVERSION_MOVE) return Instructions.X64._MOVZX;
		if (operation == Instructions.X64.SIGNED_CONVERSION_MOVE) return Instructions.X64._MOVSX;
		if (operation == Instructions.X64.SIGNED_DWORD_CONVERSION_MOVE) return Instructions.X64._MOVSXD;
		if (operation == Instructions.X64.EVALUATE) return Instructions.X64._LEA;
		if (operation == Instructions.Shared.COMPARE) return Instructions.X64._CMP;
		if (operation == Instructions.X64.DOUBLE_PRECISION_ADD) return Instructions.X64._ADDSD;
		if (operation == Instructions.X64.DOUBLE_PRECISION_SUBTRACT) return Instructions.X64._SUBSD;
		if (operation == Instructions.X64.DOUBLE_PRECISION_MULTIPLY) return Instructions.X64._MULSD;
		if (operation == Instructions.X64.DOUBLE_PRECISION_DIVIDE) return Instructions.X64._DIVSD;
		if (operation == Instructions.X64.DOUBLE_PRECISION_MOVE) return Instructions.X64._MOVSD;
		if (operation == Instructions.X64.RAW_MEDIA_REGISTER_MOVE) return Instructions.X64._MOVQ;
		if (operation == Instructions.X64.CONVERT_INTEGER_TO_DOUBLE_PRECISION) return Instructions.X64._CVTSI2SD;
		if (operation == Instructions.X64.CONVERT_DOUBLE_PRECISION_TO_INTEGER) return Instructions.X64._CVTTSD2SI;
		if (operation == Instructions.Shared.AND) return Instructions.X64._AND;
		if (operation == Instructions.X64.XOR) return Instructions.X64._XOR;
		if (operation == Instructions.X64.OR) return Instructions.X64._OR;
		if (operation == Instructions.X64.DOUBLE_PRECISION_COMPARE) return Instructions.X64._COMISD;
		if (operation == Instructions.X64.TEST) return Instructions.X64._TEST;
		if (operation == Instructions.X64.UNALIGNED_XMMWORD_MOVE) return Instructions.X64._MOVUPS;
		if (operation == "sqrtsd") return Instructions.X64._SQRTSD;
		if (operation == Instructions.X64.EXCHANGE) return Instructions.X64._XCHG;
		if (operation == Instructions.X64.MEDIA_REGISTER_BITWISE_XOR) return Instructions.X64._PXOR;
		if (operation == Instructions.X64.SHIFT_RIGHT_UNSIGNED) return Instructions.X64._SHR;
		if (operation == Instructions.X64.CONDITIONAL_MOVE_ABOVE) return Instructions.X64._CMOVA;
		if (operation == Instructions.X64.CONDITIONAL_MOVE_ABOVE_OR_EQUALS) return Instructions.X64._CMOVAE;
		if (operation == Instructions.X64.CONDITIONAL_MOVE_BELOW) return Instructions.X64._CMOVB;
		if (operation == Instructions.X64.CONDITIONAL_MOVE_BELOW_OR_EQUALS) return Instructions.X64._CMOVBE;
		if (operation == Instructions.X64.CONDITIONAL_MOVE_EQUALS) return Instructions.X64._CMOVE;
		if (operation == Instructions.X64.CONDITIONAL_MOVE_GREATER_THAN) return Instructions.X64._CMOVG;
		if (operation == Instructions.X64.CONDITIONAL_MOVE_GREATER_THAN_OR_EQUALS) return Instructions.X64._CMOVGE;
		if (operation == Instructions.X64.CONDITIONAL_MOVE_LESS_THAN) return Instructions.X64._CMOVL;
		if (operation == Instructions.X64.CONDITIONAL_MOVE_LESS_THAN_OR_EQUALS) return Instructions.X64._CMOVLE;
		if (operation == Instructions.X64.CONDITIONAL_MOVE_NOT_EQUALS) return Instructions.X64._CMOVNE;
		if (operation == Instructions.X64.CONDITIONAL_MOVE_NOT_ZERO) return Instructions.X64._CMOVNZ;
		if (operation == Instructions.X64.CONDITIONAL_MOVE_ZERO) return Instructions.X64._CMOVZ;
		if (operation == Instructions.X64.DOUBLE_PRECISION_XOR) return Instructions.X64._XORPD;
		if (operation == Instructions.X64.EXCHANGE_ADD) return Instructions.X64._XADD;

		return -1;
	}

	/// <summary>
	/// Handles debug line information instructions and other similar instructions
	/// </summary>
	public static bool ProcessDebugInstructions(EncoderModule module, Instruction instruction)
	{
		if (instruction.Type == InstructionType.APPEND_POSITION)
		{
			var position = instruction.To<AppendPositionInstruction>().Position;
			module.DebugLineInformation.Add(new EncoderDebugLineInformation(module.Position, position.FriendlyLine, position.FriendlyCharacter));
			module.DebugFrameInformation.Add(new EncoderDebugFrameInformation(EncoderDebugFrameInformationType.ADVANCE, module.Position));
			return true;
		}

		if (instruction.Type == InstructionType.DEBUG_START)
		{
			var symbol = instruction.Parameters.First().Value!.To<DataSectionHandle>().Identifier;
			module.DebugFrameInformation.Add(new EncoderDebugFrameStartInformation(module.Position, symbol));
			return true;
		}

		if (instruction.Type == InstructionType.DEBUG_FRAME_OFFSET)
		{
			var offset = (long)instruction.Parameters.First().Value!.To<ConstantHandle>().Value;
			module.DebugFrameInformation.Add(new EncoderDebugFrameOffsetInformation(module.Position, (int)offset));
			return true;
		}

		if (instruction.Type == InstructionType.DEBUG_END)
		{
			module.DebugFrameInformation.Add(new EncoderDebugFrameInformation(EncoderDebugFrameInformationType.END, module.Position));
			return true;
		}

		return false;
	}

	/// <summary>
	/// Returns the primary operation of the specified instruction by discarding any instruction prefixes
	/// </summary>
	public static string GetPrimaryOperation(Instruction instruction)
	{
		var i = instruction.Operation.LastIndexOf(' ');
		return i == -1 ? instruction.Operation : instruction.Operation.Substring(i + 1);
	}

	public static bool WriteInstruction(EncoderModule module, Instruction instruction)
	{
		var parameters = instruction.Parameters.Where(i => !i.IsHidden).ToList();
		var encoding = new InstructionEncoding();

		var locked = instruction.Operation.StartsWith(Instructions.X64.LOCK_PREFIX + ' ');
		var operation = GetPrimaryOperation(instruction);
		var identifier = GetInstructionIndex(instruction, operation);

		if (identifier < 0) return ProcessDebugInstructions(module, instruction);

		// Find the correct encoding
		if (parameters.Count == 0) { encoding = FindEncoding(identifier); }
		if (parameters.Count == 1) { encoding = FindEncoding(identifier, parameters[0].Value!); }
		if (parameters.Count == 2) { encoding = FindEncoding(identifier, parameters[0].Value!, parameters[1].Value!); }
		if (parameters.Count == 3) { encoding = FindEncoding(identifier, parameters[0].Value!, parameters[1].Value!, parameters[2].Value!); }

		// Write the lock prefix if necessary
		if (locked) Write(module, LOCK_PREFIX);

		// Write the instruction prefix if needed
		if (encoding.Prefix != 0) Write(module, encoding.Prefix);

		switch (encoding.Route) {
			case EncodingRoute.RRC: {
				WriteRegisterAndRegister(module, encoding, parameters[0].Value!.To<RegisterHandle>().Register, parameters[1].Value!.To<RegisterHandle>().Register);
				WriteRawConstant(module, (long)parameters[2].Value!.To<ConstantHandle>().Value, encoding.InputSizeOfThird);
				break;
			}

			case EncodingRoute.RMC: {
				var destination = parameters[0].Value!.To<RegisterHandle>().Register;
				var descriptor = GetMemoryAddressDescriptor(parameters[1].Value!);

				if (descriptor.Relocation != null) WriteRegisterAndSymbol(module, encoding, destination.Identifier, descriptor.Relocation);
				else if (descriptor.Start != null && descriptor.Index != null) WriteRegisterAndMemoryAddress(module, encoding, destination.Identifier, descriptor.Start, descriptor.Index, descriptor.Stride, descriptor.Offset);
				else if (descriptor.Start != null && descriptor.Index == null) WriteRegisterAndMemoryAddress(module, encoding, destination.Identifier, descriptor.Start, descriptor.Offset);
				else if (descriptor.Start == null && descriptor.Index != null) WriteRegisterAndMemoryAddress(module, encoding, destination.Identifier, descriptor.Index, descriptor.Stride, descriptor.Offset);
				else WriteRegisterAndMemoryAddress(module, encoding, destination.Identifier, descriptor.Offset);

				WriteRawConstant(module, (long)parameters[2].Value!.To<ConstantHandle>().Value, encoding.InputSizeOfThird);

				// Symbol relocations are computed from the end of the instruction
				if (descriptor.Relocation != null) { descriptor.Relocation.Addend -= module.Position - descriptor.Relocation.Offset; }
				break;
			}

			case EncodingRoute.DRC: {
				WriteRegisterAndRegister(module, encoding, parameters[0].Value!.To<RegisterHandle>().Register, parameters[0].Value!.To<RegisterHandle>().Register);
				WriteRawConstant(module, (long)parameters[1].Value!.To<ConstantHandle>().Value, encoding.InputSizeOfSecond);
				break;
			}

			case EncodingRoute.RR: {
				WriteRegisterAndRegister(module, encoding, parameters[0].Value!.To<RegisterHandle>().Register, parameters[1].Value!.To<RegisterHandle>().Register);
				break;
			}

			case EncodingRoute.RC: {
				WriteRegisterAndConstant(module, encoding, parameters[0].Value!.To<RegisterHandle>().Register, (long)parameters[1].Value!.To<ConstantHandle>().Value, encoding.InputSizeOfSecond);
				break;
			}

			case EncodingRoute.RM: {
				var destination = parameters[0].Value!.To<RegisterHandle>().Register;
				var descriptor = GetMemoryAddressDescriptor(parameters[1].Value!);

				if (descriptor.Relocation != null) WriteRegisterAndSymbol(module, encoding, destination.Identifier, descriptor.Relocation);
				else if (descriptor.Start != null && descriptor.Index != null) WriteRegisterAndMemoryAddress(module, encoding, destination.Identifier, descriptor.Start, descriptor.Index, descriptor.Stride, descriptor.Offset);
				else if (descriptor.Start != null && descriptor.Index == null) WriteRegisterAndMemoryAddress(module, encoding, destination.Identifier, descriptor.Start, descriptor.Offset);
				else if (descriptor.Start == null && descriptor.Index != null) WriteRegisterAndMemoryAddress(module, encoding, destination.Identifier, descriptor.Index, descriptor.Stride, descriptor.Offset);
				else WriteRegisterAndMemoryAddress(module, encoding, destination.Identifier, descriptor.Offset);

				// Symbol relocations are computed from the end of the instruction
				if (descriptor.Relocation != null) { descriptor.Relocation.Addend -= module.Position - descriptor.Relocation.Offset; }
				break;
			}

			case EncodingRoute.MR: {
				var source = parameters[1].Value!.To<RegisterHandle>().Register;
				var descriptor = GetMemoryAddressDescriptor(parameters[0].Value!);

				if (descriptor.Relocation != null) WriteRegisterAndSymbol(module, encoding, source.Identifier, descriptor.Relocation);
				else if (descriptor.Start != null && descriptor.Index != null) WriteRegisterAndMemoryAddress(module, encoding, source.Identifier, descriptor.Start, descriptor.Index, descriptor.Stride, descriptor.Offset);
				else if (descriptor.Start != null && descriptor.Index == null) WriteRegisterAndMemoryAddress(module, encoding, source.Identifier, descriptor.Start, descriptor.Offset);
				else if (descriptor.Start == null && descriptor.Index != null) WriteRegisterAndMemoryAddress(module, encoding, source.Identifier, descriptor.Index, descriptor.Stride, descriptor.Offset);
				else WriteRegisterAndMemoryAddress(module, encoding, source.Identifier, descriptor.Offset);

				// Symbol relocations are computed from the end of the instruction
				if (descriptor.Relocation != null) { descriptor.Relocation.Addend -= module.Position - descriptor.Relocation.Offset; }
				break;
			}

			case EncodingRoute.MC: {
				var descriptor = GetMemoryAddressDescriptor(parameters[0].Value!);

				if (descriptor.Relocation != null) WriteRegisterAndSymbol(module, encoding, encoding.Modifier, descriptor.Relocation);
				else if (descriptor.Start != null && descriptor.Index != null) WriteRegisterAndMemoryAddress(module, encoding, encoding.Modifier, descriptor.Start, descriptor.Index, descriptor.Stride, descriptor.Offset);
				else if (descriptor.Start != null && descriptor.Index == null) WriteRegisterAndMemoryAddress(module, encoding, encoding.Modifier, descriptor.Start, descriptor.Offset);
				else if (descriptor.Start == null && descriptor.Index != null) WriteRegisterAndMemoryAddress(module, encoding, encoding.Modifier, descriptor.Index, descriptor.Stride, descriptor.Offset);
				else WriteRegisterAndMemoryAddress(module, encoding, encoding.Modifier, descriptor.Offset);

				WriteRawConstant(module, (long)parameters[1].Value!.To<ConstantHandle>().Value, encoding.InputSizeOfSecond);

				// Symbol relocations are computed from the end of the instruction
				if (descriptor.Relocation != null) { descriptor.Relocation.Addend -= module.Position - descriptor.Relocation.Offset; }
				break;
			}

			case EncodingRoute.OC: {
				var first = parameters[0].Value!.To<RegisterHandle>().Register;
				var force = IsOverridableRegister(first, encoding.InputSizeOfFirst);
				TryWriteRex(module, encoding.Is64Bit, false, false, IsExtensionRegister(first), force);
				WriteOperation(module, encoding.Operation + first.Name);
				WriteRawConstant(module, (long)parameters[1].Value!.To<ConstantHandle>().Value, encoding.InputSizeOfFirst);
				break;
			}

			case EncodingRoute.R: {
				WriteSingleRegister(module, encoding, parameters[0].Value!.To<RegisterHandle>().Register);
				break;
			}

			case EncodingRoute.M: {
				var destination = parameters[0].Value!;
				var descriptor = GetMemoryAddressDescriptor(destination);

				if (descriptor.Relocation != null) WriteRegisterAndSymbol(module, encoding, encoding.Modifier, descriptor.Relocation);
				else if (descriptor.Start != null && descriptor.Index != null) WriteRegisterAndMemoryAddress(module, encoding, encoding.Modifier, descriptor.Start, descriptor.Index, descriptor.Stride, descriptor.Offset);
				else if (descriptor.Start != null && descriptor.Index == null) WriteRegisterAndMemoryAddress(module, encoding, encoding.Modifier, descriptor.Start, descriptor.Offset);
				else if (descriptor.Start == null && descriptor.Index != null) WriteRegisterAndMemoryAddress(module, encoding, encoding.Modifier, descriptor.Index, descriptor.Stride, descriptor.Offset);
				else WriteRegisterAndMemoryAddress(module, encoding, encoding.Modifier, descriptor.Offset);

				// Symbol relocations are computed from the end of the instruction
				if (descriptor.Relocation != null) { descriptor.Relocation.Addend -= module.Position - descriptor.Relocation.Offset; }
				break;
			}

			case EncodingRoute.SC: {
				TryWriteRex(module, encoding.Is64Bit, false, false, false, false);
				WriteOperation(module, encoding.Operation);
				WriteRawConstant(module, (long)parameters[1].Value!.To<ConstantHandle>().Value, encoding.InputSizeOfSecond);
				break;
			}

			case EncodingRoute.O: {
				var first = parameters[0].Value!.To<RegisterHandle>().Register;
				var force = IsOverridableRegister(first, encoding.InputSizeOfFirst);
				TryWriteRex(module, encoding.Is64Bit, false, false, IsExtensionRegister(first), force);
				WriteOperation(module, encoding.Operation + first.Name);
				break;
			}

			case EncodingRoute.SO: {
				var second = parameters[1].Value!.To<RegisterHandle>().Register;
				var force = IsOverridableRegister(second, encoding.InputSizeOfSecond);
				TryWriteRex(module, encoding.Is64Bit, false, false, IsExtensionRegister(second), force);
				WriteOperation(module, encoding.Operation + second.Name);
				break;
			}

			case EncodingRoute.D: {
				WriteOperation(module, encoding.Operation);

				if (operation == Instructions.X64.CALL)
				{
					var label = new Label(instruction.Parameters[0].Value!.To<DataSectionHandle>().Identifier);
					module.Calls.Add(new LabelUsageItem(LabelUsageType.CALL, module.Position, label));
				}

				WriteInt32(module, 0);
				break;
			}

			case EncodingRoute.L: {
				module.Labels.Add(new LabelUsageItem(LabelUsageType.LABEL, module.Position, instruction.To<LabelInstruction>().Label));
				break;
			}

			case EncodingRoute.NONE: {
				TryWriteRex(module, encoding.Is64Bit, false, false, false, false);
				WriteOperation(module, encoding.Operation);
				break;
			}

			default: throw new NotSupportedException("Unsupported encoding route");
		}

		return true;
	}

	/// <summary>
	/// Creates modules from the specified instructions by giving each jump its own module.
	/// Example:
	/// [Start of module 1]
	/// ...
	/// Jump L0
	/// [End of module 1]
	/// [Start of module 2]
	/// ...
	/// L0:
	/// ...
	/// Jump L1
	/// [End of module 2]
	/// </summary>
	public static List<EncoderModule> CreateModules(List<Instruction> instructions)
	{
		var modules = new List<EncoderModule>();
		var start = 0;

		while (true)
		{
			#warning You might want to optimize the detection of a jump
			var end = instructions.FindIndex(start, instructions.Count - start, i =>
			{
				// Verify that the instruction represents a jump
				if (i.Type != InstructionType.JUMP && !AssemblyParser.IsJump(i.Operation)) return false;

				// Now ensure that it jumps to a label instead of using registers or memory addresses
				var destination = i.Parameters.First().Value!;
				return destination.Instance == HandleInstanceType.DATA_SECTION;
			}) + 1;

			if (end != 0)
			{
				var label = new Label(instructions[end - 1].Parameters.First().Value!.To<DataSectionHandle>().Identifier);
				var conditional = instructions[end - 1].Operation != Instructions.X64.JUMP;

				var module = new EncoderModule(label, conditional);
				module.Instructions.AddRange(instructions.GetRange(start, end - start));
				module.Output = new byte[MAX_INSTRUCTION_SIZE * module.Instructions.Count(i => !string.IsNullOrEmpty(i.Operation))];
				module.Index = modules.Count;
				modules.Add(module);
			}
			else
			{
				var module = new EncoderModule();
				module.Instructions.AddRange(instructions.GetRange(start, instructions.Count - start));
				module.Output = new byte[MAX_INSTRUCTION_SIZE * module.Instructions.Count(i => !string.IsNullOrEmpty(i.Operation))];
				module.Index = modules.Count;
				modules.Add(module);
				break;
			}

			start = end;
		}

		return modules;
	}

	/// <summary>
	/// Encodes each module using tasks
	/// </summary>
	public static Task<string?>[] Encode(List<EncoderModule> modules)
	{
		var tasks = new Task<string?>[modules.Count];

		for (var i = 0; i < tasks.Length; i++)
		{
			var j = i;

			var module = modules[j];
			var parser = new AssemblyParser();
			var file = new SourceFile(TEMPORARY_ASSEMBLY_FILE, string.Empty, 0);

			tasks[j] = Task.Run<string?>(() =>
			{
				var module = modules[j];
				var parser = new AssemblyParser();

				foreach (var instruction in module.Instructions)
				{
					if (!instruction.IsManual)
					{
						if (!WriteInstruction(module, instruction) && !string.IsNullOrEmpty(instruction.Operation))
						{
							return "Could not understand the instruction";
						}

						continue;
					}

					// Parse the assembly code and then reset the parser for next use
					parser.Parse(file, instruction.Operation.Replace("\r", string.Empty));

					foreach (var subinstruction in parser.Instructions)
					{
						if (!WriteInstruction(module, subinstruction) && !string.IsNullOrEmpty(instruction.Operation))
						{
							return "Could not understand the instruction";
						}
					}

					parser.Reset();
				}

				return null;
			});
			
			// Single threaded version:
			// foreach (var instruction in module.Instructions)
			// {
			// 	if (!instruction.IsManual)
			// 	{
			// 		if (!WriteInstruction(module, instruction) && !string.IsNullOrEmpty(instruction.Operation)) throw new ApplicationException("Could not understand the instruction");
			// 		continue;
			// 	}

			// 	// Parse the assembly code and then reset the parser for next use
			// 	parser.Parse(file, instruction.Operation.Replace("\r", string.Empty));

			// 	foreach (var subinstruction in parser.Instructions)
			// 	{
			// 		if (!WriteInstruction(module, subinstruction) && !string.IsNullOrEmpty(instruction.Operation)) throw new ApplicationException("Could not understand the instruction");
			// 	}

			// 	parser.Reset();
			// }
		}

		return tasks;
	}

	/// <summary>
	/// Finds all labels from the specified modules and gathers them into the specified label dictionary
	/// </summary>
	public static void LoadLabels(List<EncoderModule> modules, Dictionary<Label, LabelDescriptor> labels)
	{
		foreach (var module in modules)
		{
			foreach (var item in module.Labels)
			{
				if (labels.TryAdd(item.Label, new LabelDescriptor(module, item.Position))) continue;
				throw new ApplicationException($"Label {item.Label.Name} is created multiple times");
			}
		}
	}

	/// <summary>
	/// Returns whether currently an extended jump is needed between the specified jump and its destination label
	/// </summary>
	public static bool IsLongJumpNeeded(List<EncoderModule> modules, Dictionary<Label, LabelDescriptor> labels, EncoderModule module, int position)
	{
		// If the label does not exist in the specified labels, it must be an external label which are assumed to require long jumps
		if (!labels.TryGetValue(module.Jump!, out var descriptor)) return true;

		if (module.Index == descriptor.Module.Index)
		{
			var difference = descriptor.Position - position;
			return difference < sbyte.MinValue || difference > sbyte.MaxValue;
		}
		else if (module.Index < descriptor.Module.Index)
		{
			var start = module.Index;
			var end = descriptor.Module.Index;

			// Start             Distance 1         Distance n - 1      Distance n        End
			// [ ... Jump L0 ] [  Module 1  ] ... [  Module n - 1  ] [ ............ L0: ... ]
			var distance = 0;
			distance += descriptor.Position; // Distance n

			// Distances [1, n - 1]
			for (var i = start + 1; i < end; i++) { distance += modules[i].Position; }

			return distance < sbyte.MinValue || distance > sbyte.MaxValue;
		}
		else
		{
			var start = descriptor.Module.Index;
			var end = module.Index;

			// Start          Distace 0     Distance 1         Distance n - 1      Distance n        End
			// [ ... L0: ...............] [  Module 1  ] ... [  Module n - 1  ] [ ............ Jump L0 ]
			var distance = 0;
			distance += descriptor.Module.Position - descriptor.Position; // Distance 0
			distance += position; // Distance n

			// Distances [1, n - 1]
			for (var i = start + 1; i < end; i++) { distance += modules[i].Position; }

			distance = -distance;
			return distance < sbyte.MinValue || distance > sbyte.MaxValue;
		}
	}

	/// <summary>
	/// Returns the distance that specified module jumps. The unit of distance is one module.
	/// If the specified module does not have a jump, this function returns zero.
	/// If the specified module jumps to an external label, this function returns int.MaxValue.
	/// </summary>
	private static int GetModuleJumpDistance(EncoderModule module, Dictionary<Label, LabelDescriptor> labels)
	{
		if (module.Jump == null) return 0;
		if (!labels.TryGetValue(module.Jump, out var label)) return int.MaxValue;

		return Math.Abs(label.Module.Index - module.Index);
	}

	/// <summary>
	/// Goes through the specified modules and decides the jump sizes
	/// </summary>
	public static void CompleteModules(List<EncoderModule> modules, Dictionary<Label, LabelDescriptor> labels)
	{
		// Order the modules so that shorter jumps are completed first
		/// NOTE: This should reduce the error of approximated jump distances, because if shorter jumps are completed first, there should be less uncompleted jumps between longer jumps
		foreach (var module in modules.OrderBy(i => GetModuleJumpDistance(i, labels)).ToList())
		{
			if (module.Jump == null) continue;

			// Express the current position as if the module jump was an 8-bit jump
			var position = module.Position - (JUMP_OFFSET32_SIZE - JUMP_OFFSET8_SIZE);
			if (module.IsConditionalJump) { position = module.Position - (CONDITIONAL_JUMP_OFFSET32_SIZE - CONDITIONAL_JUMP_OFFSET8_SIZE); }

			if (!IsLongJumpNeeded(modules, labels, module, position))
			{
				if (module.IsConditionalJump)
				{
					// Remove the 0x0F-byte
					// 32-bit conditional jumps: 0F $(Operation code) $(32-bit offset)
					// 8-bit conditional jumps:  $(Operation code - 0x10) $(8-bit offset)
					/// NOTE: No need to worry about the offset, because it is not computed yet
					module.Output[position - 2] = (byte)(module.Output[position - 1] - 0x10);
				}
				else
				{
					// Change the operation code in order to represent an 8-bit jump
					module.Output[position - 2] = JUMP_OFFSET8_OPERATION_CODE;
				}

				module.Position = position;
				module.IsShortJump = true;
			}
		}
	}

	/// <summary>
	/// Computes the 'absolute positions' of all modules relative to the start of the first module
	/// </summary>
	public static void ComputeModulePositions(List<EncoderModule> modules)
	{
		// Align all modules
		var position = 0;

		foreach (var module in modules)
		{
			module.Start = position;
			position += module.Position;
		}
	}

	/// <summary>
	/// Finds all jumps and calls and write the their offsets to the binary output
	/// </summary>
	public static void WriteOffsets(List<EncoderModule> modules, Dictionary<Label, LabelDescriptor> labels)
	{
		// Jumps:
		foreach (var module in modules)
		{
			if (module.Jump == null || !labels.TryGetValue(module.Jump, out var descriptor)) continue;

			var from = module.Start + module.Position;
			var to = descriptor.AbsolutePosition;
			var offset = to - from;

			if (module.IsShortJump) Write(module, module.Position - sizeof(byte), offset);
			else WriteInt32(module, module.Position - sizeof(int), offset);
		}

		// Calls:
		foreach (var module in modules)
		{
			for (var i = module.Calls.Count - 1; i >= 0; i--)
			{
				var call = module.Calls[i];

				if (!labels.TryGetValue(call.Label, out var descriptor)) continue;

				// Move the start to the end of the call instruction using the offset 'sizeof(int)'
				var from = module.Start + call.Position + sizeof(int);
				var to = descriptor.AbsolutePosition;
				var offset = to - from;

				WriteInt32(module, call.Position, offset);

				// Remove the call since it is now resolved
				module.Calls.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// Creates a text section and exports the specified labels as symbols
	/// </summary>
	public static EncoderOutput Export(List<EncoderModule> modules, Dictionary<Label, LabelDescriptor> labels, string? debug_file)
	{
		// Mesh all the module binaries into one large binary
		var bytes = modules.Sum(i => i.Position);
		var binary = new byte[bytes];
		var position = 0;

		foreach (var module in modules)
		{
			Array.Copy(module.Output, 0, binary, position, module.Position);
			position += module.Position;
		}

		// Create the text section objects
		var symbols = new Dictionary<string, BinarySymbol>();
		var relocations = new List<BinaryRelocation>();

		var section = new BinarySection(ElfFormat.TEXT_SECTION, BinarySectionType.TEXT, binary);
		section.Flags = BinarySectionFlags.EXECUTE | BinarySectionFlags.ALLOCATE;
		section.Symbols = symbols;
		section.Relocations = relocations;

		// Add local labels
		foreach (var iterator in labels)
		{
			var symbol = new BinarySymbol(iterator.Key.GetName(), iterator.Value.AbsolutePosition, false);
			symbol.Section = section;
			symbols.Add(symbol.Name, symbol);
		}

		// Generate relocations for jumps, which use external symbols
		foreach (var module in modules)
		{
			if (module.Jump == null) continue;

			// Load the name of destination label
			var name = module.Jump.GetName();

			if (!symbols.TryGetValue(name, out var symbol))
			{
				symbol = new BinarySymbol(name, 0, true);
				symbol.Section = section;
				symbols.Add(symbol.Name, symbol);
			}

			// Skip local module jumps, because the offset is known and computed already, therefore relocation is not needed
			if (!symbol.External) continue;

			// Use offset -4, because the jump offset is measured from the end of the jump instruction, which is four bytes after the start of the relocatable symbol
			var relocation = new BinaryRelocation(symbol, module.Start + module.Position - 4, -4, BinaryRelocationType.PROGRAM_COUNTER_RELATIVE);
			relocation.Section = section;

			relocations.Add(relocation);
		}

		// Generate relocations for calls, which use external symbols
		/// NOTE: At this stage all module calls use external symbols, because those which used local symbols were removed
		foreach (var module in modules)
		{
			foreach (var call in module.Calls)
			{
				if (!symbols.TryGetValue(call.Label.GetName(), out var symbol))
				{
					symbol = new BinarySymbol(call.Label.GetName(), 0, true);
					symbol.Section = section;
					symbols.Add(symbol.Name, symbol);
				}

				// Use offset -4, because the call offset is measured from the end of the call instruction, which is four bytes after the start of the relocatable symbol
				var relocation = new BinaryRelocation(symbol, module.Start + call.Position, -4, call.Modifier);
				relocation.Section = section;

				relocations.Add(relocation);
			}
		}

		// Generate relocations for memory addresses in the machine code
		foreach (var module in modules)
		{
			foreach (var relocation in module.MemoryAddressRelocations)
			{
				// Try to find the local version of the relocation symbol, if it is not found, add the relocation symbol as an external symbol
				if (symbols.TryGetValue(relocation.Symbol.Name, out var local)) { relocation.Symbol = local; }
				else { symbols.Add(relocation.Symbol.Name, relocation.Symbol); }
				
				relocation.Offset += module.Start;
				relocation.Section = section;
				relocations.Add(relocation);
			}
		}

		// Return now, if no debugging information is needed
		if (debug_file == null) return new EncoderOutput(section, symbols, relocations, null, null);

		var lines = new DebugLineEncoderModule(debug_file);
		var frames = new DebugFrameEncoderModule(0);

		// Generate debug line information
		foreach (var module in modules)
		{
			foreach (var line in module.DebugLineInformation)
			{
				// Compute the absolute offset of the current debug point and move to that position
				var offset = module.Start + line.Offset;
				lines.Move(section, line.Line, line.Character, offset);
			}
		}

		position = 0;

		// Generate debug frame information
		foreach (var module in modules)
		{
			foreach (var information in module.DebugFrameInformation)
			{
				// Compute the absolute offset of the current debug point and move to that position
				var offset = module.Start + information.Offset;

				if (information.Type == EncoderDebugFrameInformationType.START)
				{
					frames.Start(information.To<EncoderDebugFrameStartInformation>().Symbol, offset);
					position = offset;
				}
				else if (information.Type == EncoderDebugFrameInformationType.SET_FRAME_OFFSET)
				{
					frames.Move(offset - position);
					frames.SetFrameOffset(information.To<EncoderDebugFrameOffsetInformation>().FrameOffset);
					position = offset;
				}
				else if (information.Type == EncoderDebugFrameInformationType.END)
				{
					frames.End(offset);
				}
			}
		}

		return new EncoderOutput(section, symbols, relocations, frames, lines);
	}

	/// <summary>
	/// Encodes the specified instructions
	/// </summary>
	public static EncoderOutput Encode(List<Instruction> instructions, string? debug_file)
	{
		var modules = CreateModules(instructions);
		var labels = new Dictionary<Label, LabelDescriptor>();

		var tasks = Encode(modules);
		Task.WaitAll(tasks); // Encode each module

		// Ensure all the tasks completed successfully
		foreach (var task in tasks)
		{
			if (task.Result != null) throw new ApplicationException(task.Result);
		}

		LoadLabels(modules, labels); // Load all labels into a dictionary where information about their positions can be pulled
		CompleteModules(modules, labels); // Decide jump sizes based on the loaded label information
		ComputeModulePositions(modules);
		WriteOffsets(modules, labels);

		return Export(modules, labels, debug_file);
	}

	/// <summary>
	/// Prints the hexadecimal values of each of the specified modules
	/// </summary>
	public static void Print(List<EncoderModule> modules)
	{
		foreach (var module in modules)
		{
			for (var i = 0; i < module.Position; i++)
			{
				var label = module.Labels.FindIndex(j => j.Position == i);

				if (label >= 0)
				{
					Console.WriteLine();
					Console.WriteLine($"{module.Labels[label].Label.GetName()}:");
				}

				if (i > 0 && i % 10 == 0) Console.WriteLine();
				Console.Write($"{module.Output[i]:x2}");
				Console.Write(' ');
			}
		}

		Console.WriteLine();
	}
}