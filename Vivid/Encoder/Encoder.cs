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

public class EncoderModule
{
	public int Index { get; set; }
	public Label? Jump { get; set; }
	public bool IsConditionalJump { get; set; }
	public bool IsShortJump { get; set; }
	public List<Instruction> Instructions { get; }
	public List<LabelUsageItem> Labels { get; }
	public List<LabelUsageItem> Calls { get; }
	public List<LabelUsageItem> Items { get; }
	public List<BinaryRelocation> MemoryAddressRelocations { get; }
	public byte[] Output { get; set; }
	public int Position { get; set; }
	public int Start { get; set; }

	public EncoderModule(Label jump, bool conditional)
	{
		Jump = jump;
		IsConditionalJump = conditional;
		IsShortJump = false;
		Instructions = new List<Instruction>();
		Labels = new List<LabelUsageItem>();
		Calls = new List<LabelUsageItem>();
		Items = new List<LabelUsageItem>();
		MemoryAddressRelocations = new List<BinaryRelocation>();
		Output = Array.Empty<byte>();
		Position = 0;
		Start = 0;
	}

	public EncoderModule()
	{
		Jump = null;
		IsConditionalJump = false;
		IsShortJump = false;
		Instructions = new List<Instruction>();
		Labels = new List<LabelUsageItem>();
		Calls = new List<LabelUsageItem>();
		Items = new List<LabelUsageItem>();
		MemoryAddressRelocations = new List<BinaryRelocation>();
		Output = Array.Empty<byte>();
		Position = 0;
		Start = 0;
	}
}

public struct EncoderOutput
{
	public BinarySection Section { get; set; }
	public Dictionary<string, BinarySymbol> Symbols { get; }
	public List<BinaryRelocation> Relocations { get; }

	public EncoderOutput(BinarySection section, Dictionary<string, BinarySymbol> symbols, List<BinaryRelocation> relocations)
	{
		Section = section;
		Symbols = symbols;
		Relocations = relocations;
	}
}

public enum EncodingFilterType
{
	REGISTER,
	SPECIFIC_REGISTER,
	MEMORY_ADDRESS,
	CONSTANT,
	SPECIFIC_CONSTANT,
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

public static class EncoderX64
{
	public const int MAX_INSTRUCTION_SIZE = 15;

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
	/// Writes the specified value to the specified position
	/// </summary>
	public static void WriteInt32(byte[] data, int position, long value)
	{
		data[position++] = (byte)(value & 0xFF);
		data[position++] = (byte)((value & 0xFF00) >> 8);
		data[position++] = (byte)((value & 0xFF0000) >> 16);
		data[position++] = (byte)((value & 0xFF000000) >> 24);
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
		var force = IsOverridableRegister(first, encoding.InputSizeOfFirst);
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
		var force = IsOverridableRegister(first, encoding.InputSizeOfFirst);
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

		var force = IsOverridableRegister(first, encoding.InputSizeOfFirst);
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

		var force = IsOverridableRegister(first, encoding.InputSizeOfFirst);
		TryWriteRex(module, encoding.Is64Bit, IsExtensionRegister(first), IsExtensionRegister(index), false, force);

		Write(module, (first & 7) << 3 | Instructions.X64.RSP);
		WriteSIB(module, scale, index.Name, Instructions.X64.RBP);
		Write(module, offset);
	}

	/// <summary>
	/// Writes register and memort address operands
	/// </summmary>
	public static void WriteRegisterAndMemoryAddress(EncoderModule module, InstructionEncoding encoding, int first, int offset)
	{
		var force = IsOverridableRegister(first, encoding.InputSizeOfFirst);
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
			case EncodingFilterType.EXPRESSION: return value.Type == HandleType.EXPRESSION;
			case EncodingFilterType.LABEL: return value.Instance == HandleInstanceType.DATA_SECTION && value.To<DataSectionHandle>().Address;
			default: return false;
		}
	}

	/// <summary>
	/// Returns whether the specified handle passes the configured filter
	/// </summary>
	private static bool PassesSize(Handle value, short size)
	{
		if (value.Instance == HandleInstanceType.CONSTANT) return value.To<ConstantHandle>().Bits / 8 <= size;
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
			if (!PassesSize(first, encoding.InputSizeOfFirst)) continue;
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
			if (!PassesSize(first, encoding.InputSizeOfFirst) || !PassesSize(second, encoding.InputSizeOfSecond)) continue;
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
			if (!PassesSize(first, encoding.InputSizeOfFirst) || !PassesSize(second, encoding.InputSizeOfSecond) || !PassesSize(third, encoding.InputSizeOfThird)) continue;
			if (!PassesFilter(encoding.FilterTypeOfFirst, encoding.FilterOfFirst, first) || !PassesFilter(encoding.FilterTypeofSecond, encoding.FilterOfSecond, second) || !PassesFilter(encoding.FilterTypeOfThird, encoding.FilterOfThird, third)) continue;
			return encoding;
		}

		throw new ApplicationException("Could not find instruction encoding");
	}

	/// <summary>
	/// Returns the unique operation index of the specified instruction.
	/// This function will be removed, because instruction will use operation indices instead of text identifiers in the future.
	/// </summary>
	public static int GetInstructionIndex(Instruction instruction)
	{
		if (instruction.Type == InstructionType.LABEL) return Instructions.X64._LABEL;

		if (instruction.Operation == Instructions.Shared.RETURN) return Instructions.X64._RETURN;
		if (instruction.Operation == Instructions.X64.EXTEND_QWORD) return Instructions.X64._CQO;
		if (instruction.Operation == Instructions.X64.SYSTEM_CALL) return Instructions.X64._SYSCALL;
		if (instruction.Operation == "fld1") return Instructions.X64._FLD1;
		if (instruction.Operation == "fyl2x") return Instructions.X64._FYL2x;
		if (instruction.Operation == "f2xm1") return Instructions.X64._F2XM1;
		if (instruction.Operation == "faddp") return Instructions.X64._FADDP;
		if (instruction.Operation == "fcos") return Instructions.X64._FCOS;
		if (instruction.Operation == "fsin") return Instructions.X64._FSIN;

		if (instruction.Operation == Instructions.X64.PUSH) return Instructions.X64._PUSH;
		if (instruction.Operation == Instructions.X64.POP) return Instructions.X64._POP;
		if (instruction.Operation == Instructions.X64.JUMP_ABOVE) return Instructions.X64._JA;
		if (instruction.Operation == Instructions.X64.JUMP_ABOVE_OR_EQUALS) return Instructions.X64._JAE;
		if (instruction.Operation == Instructions.X64.JUMP_BELOW) return Instructions.X64._JB;
		if (instruction.Operation == Instructions.X64.JUMP_BELOW_OR_EQUALS) return Instructions.X64._JBE;
		if (instruction.Operation == Instructions.X64.JUMP_EQUALS) return Instructions.X64._JE;
		if (instruction.Operation == Instructions.X64.JUMP_GREATER_THAN) return Instructions.X64._JG;
		if (instruction.Operation == Instructions.X64.JUMP_GREATER_THAN_OR_EQUALS) return Instructions.X64._JGE;
		if (instruction.Operation == Instructions.X64.JUMP_LESS_THAN) return Instructions.X64._JL;
		if (instruction.Operation == Instructions.X64.JUMP_LESS_THAN_OR_EQUALS) return Instructions.X64._JLE;
		if (instruction.Operation == Instructions.X64.JUMP) return Instructions.X64._JMP;
		if (instruction.Operation == Instructions.X64.JUMP_NOT_EQUALS) return Instructions.X64._JNE;
		if (instruction.Operation == Instructions.X64.JUMP_NOT_ZERO) return Instructions.X64._JNZ;
		if (instruction.Operation == Instructions.X64.JUMP_ZERO) return Instructions.X64._JZ;
		if (instruction.Operation == Instructions.X64.CALL) return Instructions.X64._CALL;
		if (instruction.Operation == "fild") return Instructions.X64._FILD;
		if (instruction.Operation == "fld") return Instructions.X64._FLD;
		if (instruction.Operation == "fistp") return Instructions.X64._FISTP;
		if (instruction.Operation == "fstp") return Instructions.X64._FSTP;
		if (instruction.Operation == Instructions.Shared.NEGATE) return Instructions.X64._NEG;
		if (instruction.Operation == Instructions.X64.NOT) return Instructions.X64._NOT;

		if (instruction.Operation == Instructions.Shared.MOVE) return Instructions.X64._MOVE;
		if (instruction.Operation == Instructions.Shared.ADD) return Instructions.X64._ADD;
		if (instruction.Operation == Instructions.Shared.SUBTRACT) return Instructions.X64._SUBTRACT;
		if (instruction.Operation == Instructions.X64.SIGNED_MULTIPLY) return Instructions.X64._SIGNED_MULTIPLY;
		if (instruction.Operation == Instructions.X64.UNSIGNED_MULTIPLY) return Instructions.X64._UNSIGNED_MULTIPLY;
		if (instruction.Operation == Instructions.X64.SIGNED_DIVIDE) return Instructions.X64._SIGNED_DIVIDE;
		if (instruction.Operation == Instructions.X64.UNSIGNED_DIVIDE) return Instructions.X64._UNSIGNED_DIVIDE;
		if (instruction.Operation == Instructions.X64.SHIFT_LEFT) return Instructions.X64._SHIFT_LEFT;
		if (instruction.Operation == Instructions.X64.SHIFT_RIGHT) return Instructions.X64._SHIFT_RIGHT;
		if (instruction.Operation == Instructions.X64.UNSIGNED_CONVERSION_MOVE) return Instructions.X64._MOVZX;
		if (instruction.Operation == Instructions.X64.SIGNED_CONVERSION_MOVE) return Instructions.X64._MOVSX;
		if (instruction.Operation == Instructions.X64.SIGNED_DWORD_CONVERSION_MOVE) return Instructions.X64._MOVSXD;
		if (instruction.Operation == Instructions.X64.EVALUATE) return Instructions.X64._LEA;
		if (instruction.Operation == Instructions.Shared.COMPARE) return Instructions.X64._CMP;
		if (instruction.Operation == Instructions.X64.DOUBLE_PRECISION_ADD) return Instructions.X64._ADDSD;
		if (instruction.Operation == Instructions.X64.DOUBLE_PRECISION_SUBTRACT) return Instructions.X64._SUBSD;
		if (instruction.Operation == Instructions.X64.DOUBLE_PRECISION_MULTIPLY) return Instructions.X64._MULSD;
		if (instruction.Operation == Instructions.X64.DOUBLE_PRECISION_DIVIDE) return Instructions.X64._DIVSD;
		if (instruction.Operation == Instructions.X64.DOUBLE_PRECISION_MOVE) return Instructions.X64._MOVSD;
		if (instruction.Operation == Instructions.X64.RAW_MEDIA_REGISTER_MOVE) return Instructions.X64._MOVQ;
		if (instruction.Operation == Instructions.X64.CONVERT_INTEGER_TO_DOUBLE_PRECISION) return Instructions.X64._CVTSI2SD;
		if (instruction.Operation == Instructions.X64.CONVERT_DOUBLE_PRECISION_TO_INTEGER) return Instructions.X64._CVTTSD2SI;
		if (instruction.Operation == Instructions.Shared.AND) return Instructions.X64._AND;
		if (instruction.Operation == Instructions.X64.XOR) return Instructions.X64._XOR;
		if (instruction.Operation == Instructions.X64.OR) return Instructions.X64._OR;
		if (instruction.Operation == Instructions.X64.DOUBLE_PRECISION_COMPARE) return Instructions.X64._COMISD;
		if (instruction.Operation == Instructions.X64.TEST) return Instructions.X64._TEST;
		if (instruction.Operation == Instructions.X64.UNALIGNED_XMMWORD_MOVE) return Instructions.X64._MOVUPS;
		if (instruction.Operation == "sqrtsd") return Instructions.X64._SQRTSD;
		if (instruction.Operation == Instructions.X64.EXCHANGE) return Instructions.X64._XCHG;
		if (instruction.Operation == Instructions.X64.MEDIA_REGISTER_BITWISE_XOR) return Instructions.X64._PXOR;
		if (instruction.Operation == Instructions.X64.SHIFT_RIGHT_UNSIGNED) return Instructions.X64._SHR;

		return -1;
	}

	public static void WriteInstruction(EncoderModule module, Instruction instruction)
	{
		var parameters = instruction.Parameters;
		var encoding = new InstructionEncoding();

		var identifier = GetInstructionIndex(instruction);
		if (identifier < 0) return;

		// Find the correct encoding
		if (parameters.Count == 0) { encoding = FindEncoding(identifier); }
		if (parameters.Count == 1) { encoding = FindEncoding(identifier, parameters[0].Value!); }
		if (parameters.Count == 2) { encoding = FindEncoding(identifier, parameters[0].Value!, parameters[1].Value!); }
		if (parameters.Count == 3) { encoding = FindEncoding(identifier, parameters[0].Value!, parameters[1].Value!, parameters[2].Value!); }

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

				if (instruction.Type == InstructionType.CALL)
				{
					var label = new Label(instruction.To<CallInstruction>().Parameters[0].Value!.To<DataSectionHandle>().Identifier);
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
			var end = instructions.FindIndex(start, instructions.Count - start, i => i.Type == InstructionType.JUMP && i.Parameters.First().Value!.Type != HandleType.REGISTER) + 1;

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
	public static Task[] Encode(List<EncoderModule> modules)
	{
		var tasks = new Task[modules.Count];

		for (var i = 0; i < tasks.Length; i++)
		{
			var j = i;

			var module = modules[j];
			foreach (var instruction in module.Instructions) { WriteInstruction(module, instruction); }

			#warning Enable multithreading in the future
			/*
			tasks[j] = Task.Run(() =>
			{
				var module = modules[j];
				foreach (var instruction in module.Instructions) { WriteInstruction(module, instruction); }
			});
			*/
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
				labels.Add(item.Label, new LabelDescriptor(module, item.Position));
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
	/// Goes through the specified modules and decides the jump sizes
	/// </summary>
	public static void CompleteModules(List<EncoderModule> modules, Dictionary<Label, LabelDescriptor> labels)
	{
		// Order the modules so that shorter jumps are completed first
		/// NOTE: This should reduce the error of approximated jump distances, because if shorter jumps are completed first, there should be less uncompleted jumps between longer jumps
		modules = modules.OrderBy(i => i.Jump == null ? 0 : Math.Abs(labels[i.Jump].Module.Index - i.Index)).ToList();

		foreach (var module in modules)
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
	public static EncoderOutput Export(List<EncoderModule> modules, Dictionary<Label, LabelDescriptor> labels)
	{
		var bytes = modules.Sum(i => i.Position);
		var binary = new byte[bytes];
		var position = 0;

		foreach (var module in modules)
		{
			Array.Copy(module.Output, 0, binary, position, module.Position);
			position += module.Position;
		}

		var symbols = new Dictionary<string, BinarySymbol>();
		var relocations = new List<BinaryRelocation>();

		var section = new BinarySection(ElfFormat.TEXT_SECTION, BinarySectionType.TEXT, binary);
		section.Symbols = symbols;
		section.Relocations = relocations;

		// Add local labels
		foreach (var iterator in labels)
		{
			var symbol = new BinarySymbol(iterator.Key.GetName(), iterator.Value.AbsolutePosition, false);
			symbol.Section = section;
			symbols.Add(symbol.Name, symbol);
		}

		// Add calls which use external symbols
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

				symbols.TryAdd(symbol.Name, symbol);
				relocations.Add(relocation);
			}
		}

		/// TODO: Symbols in memory addresses are not added to relocations?
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

		return new EncoderOutput(section, symbols, relocations);
	}

	/// <summary>
	/// Encodes the specified instructions
	/// </summary>
	public static EncoderOutput Encode(List<Instruction> instructions)
	{
		var modules = CreateModules(instructions);
		var labels = new Dictionary<Label, LabelDescriptor>();

		var tasks = Encode(modules); // Encode each module

		#warning Enable multithreading in the future
		//Task.WaitAll(tasks);

		LoadLabels(modules, labels); // Load all labels into a dictionary where information about their positions can be pulled
		CompleteModules(modules, labels); // Decide jump sizes based on the loaded label information
		ComputeModulePositions(modules);
		WriteOffsets(modules, labels);

		//Print(modules);

		return Export(modules, labels);
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