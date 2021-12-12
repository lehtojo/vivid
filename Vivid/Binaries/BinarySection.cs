using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;

public enum BinarySectionType
{
	NONE,
	TEXT,
	DATA,
	STRING_TABLE,
	SYMBOL_TABLE,
	RELOCATION_TABLE,
	DYNAMIC,
	HASH
}

[Flags]
public enum BinarySectionFlags
{
	WRITE = 1,
	EXECUTE = 2,
	ALLOCATE = 4,
}

public class BinarySection
{
	public string Name { get; set; }
	public int Index { get; set; } = 0;
	public BinarySectionFlags Flags { get; set; } = 0;
	public BinarySectionType Type { get; set; }
	public byte[] Data { get; set; }
	public int VirtualSize { get; set; } = 0;
	public int LoadSize { get; set; } = 0;
	public int Alignment { get; set; } = 1;
	public int Margin { get; set; } = 0;
	public int Offset { get; set; } = 0;
	public int VirtualAddress { get; set; } = 0;
	public int BaseVirtualAddress { get; set; } = 0;
	public Dictionary<string, BinarySymbol> Symbols { get; set; } = new Dictionary<string, BinarySymbol>();
	public List<BinaryRelocation> Relocations { get; set; } = new List<BinaryRelocation>();
	public List<BinaryOffset> Offsets { get; set; } = new List<BinaryOffset>();

	public BinarySection(string name, BinarySectionType type, byte[] data)
	{
		Name = name;
		Type = type;
		Data = data;
		VirtualSize = data.Length;
		LoadSize = data.Length;
	}

	public BinarySection(string name, BinarySectionFlags flags, BinarySectionType type, int alignment, byte[] data, int margin, int size)
	{
		Name = name;
		Flags = flags;
		Type = type;
		Data = data;
		VirtualSize = size;
		LoadSize = size;
		Alignment = alignment;
		Margin = margin;
	}
}

public class BinarySymbol
{
	public string Name { get; set; }
	public int Offset { get; set; }
	public bool External { get; set; }
	public bool Export { get; set; } = false;
	public uint Index { get; set; } = 0;
	public BinarySection? Section { get; set; } = null;

	public BinarySymbol(string name, int offset, bool external)
	{
		Name = name;
		Offset = offset;
		External = external;
	}

	public BinarySymbol(string name, int offset, bool external, BinarySection section)
	{
		Name = name;
		Offset = offset;
		External = external;
		Section = section;
	}

	public override bool Equals(object? other)
	{
		return other is BinarySymbol symbol && Name == symbol.Name;
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}
}

public struct BinaryOffset
{
	public int Position { get; set; }
	public Offset Offset { get; set; }
	public int Bytes { get; set; }

	public BinaryOffset(int position, Offset offset, int bytes)
	{
		Position = position;
		Offset = offset;
		Bytes = bytes;
	}
}

public enum BinaryRelocationType
{
	ABSOLUTE64,
	ABSOLUTE32,
	SECTION_RELATIVE_32,
	SECTION_RELATIVE_64,
	PROCEDURE_LINKAGE_TABLE,
	PROGRAM_COUNTER_RELATIVE,
	FILE_OFFSET_64,
	BASE_RELATIVE_64,
	BASE_RELATIVE_32,
}

public class BinaryRelocation
{
	public BinarySymbol Symbol { get; set; }
	public int Offset { get; set; }
	public int Addend { get; set; }
	public int Bytes { get; set; }
	public BinaryRelocationType Type { get; set; }
	public BinarySection? Section { get; set; }

	public BinaryRelocation(BinarySymbol symbol, int offset, int addend, DataSectionModifier modifier, int bytes = 4)
	{
		/// TODO: Support symbols using the global offset table?
		Symbol = symbol;
		Offset = offset;
		Addend = addend;
		Bytes = bytes;
		Type = modifier switch
		{
			DataSectionModifier.NONE => BinaryRelocationType.PROGRAM_COUNTER_RELATIVE,
			DataSectionModifier.GLOBAL_OFFSET_TABLE=> BinaryRelocationType.PROGRAM_COUNTER_RELATIVE,
			DataSectionModifier.PROCEDURE_LINKAGE_TABLE => BinaryRelocationType.PROCEDURE_LINKAGE_TABLE,
			_ => 0
		};
	}

	public BinaryRelocation(BinarySymbol symbol, int offset, int addend, BinaryRelocationType type, int bytes = 4)
	{
		Symbol = symbol;
		Offset = offset;
		Addend = addend;
		Bytes = bytes;
		Type = type;
	}

	public BinaryRelocation(BinarySymbol symbol, int offset, int addend, BinaryRelocationType type, BinarySection section, int bytes = 4)
	{
		Symbol = symbol;
		Offset = offset;
		Addend = addend;
		Bytes = bytes;
		Type = type;
		Section = section;
	}
}

public class BinaryObjectFile
{
	public int Index { get; set; } = 0;
	public List<BinarySection> Sections { get; } = new List<BinarySection>();
	public HashSet<string> Exports { get; } = new HashSet<string>();

	public BinaryObjectFile(List<BinarySection> sections)
	{
		Sections = sections;
	}

	public BinaryObjectFile(List<BinarySection> sections, HashSet<string> exports)
	{
		Sections = sections;
		Exports = exports;
	}
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class BinaryStringTable
{
	public List<string> Items { get; } = new List<string>();
	public int Position { get; set; } = 0;
	public bool Size { get; set; } = false;

	public BinaryStringTable(bool size = false)
	{
		Size = size;
		Position = size ? sizeof(int) : 0;
	}

	public int Add(string item)
	{
		var position = Position;
		Items.Add(item);
		Position += item.Length + 1;
		return position;
	}

	public byte[] Export()
	{
		var content = Encoding.UTF8.GetBytes(string.Join('\0', Items) + '\0');

		return Size
			? BitConverter.GetBytes(sizeof(int) + content.Length).Concat(content).ToArray()
			: content;
	}
}