using System.Collections.Generic;

public enum BinarySectionType
{
	NONE,
	TEXT,
	DATA,
	STRING_TABLE,
	SYMBOL_TABLE,
	RELOCATION_TABLE
}

public class BinarySection
{
	public string Name { get; set; }
	public int Index { get; set; } = 0;
	public BinarySectionType Type { get; set; }
	public byte[] Data { get; set; }
	public int Size { get; set; } = 0;
	public int Offset { get; set; } = 0;
	public int VirtualAddress { get; set; } = 0;
	public Dictionary<string, BinarySymbol> Symbols { get; set; } = new Dictionary<string, BinarySymbol>();
	public List<BinaryRelocation> Relocations { get; set; } = new List<BinaryRelocation>();
	public List<BinaryOffset> Offsets { get; set; } = new List<BinaryOffset>();

	public BinarySection(string name, BinarySectionType type, byte[] data)
	{
		Name = name;
		Type = type;
		Data = data;
		Size = data.Length;
	}

	public BinarySection(string name, BinarySectionType type, byte[] data, int size)
	{
		Name = name;
		Type = type;
		Data = data;
		Size = size;
	}
}

public class BinarySymbol
{
	public string Name { get; set; }
	public int Offset { get; set; }
	public bool External { get; set; }
	public bool Export { get; set; } = false;
	public bool Hidden { get; set; } = false;
	public uint Index { get; set; } = 0;
	public BinarySection? Section { get; set; } = null;

	public BinarySymbol(string name, int offset, bool external, bool hidden = false)
	{
		Name = name;
		Offset = offset;
		External = external;
		Hidden = hidden;
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
	SECTION_RELATIVE,
	PROCEDURE_LINKAGE_TABLE,
	PROGRAM_COUNTER_RELATIVE,
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
}

public class BinaryObjectFile
{
	public int Index { get; set; } = 0;
	public List<BinarySection> Sections { get; } = new List<BinarySection>();

	public BinaryObjectFile(List<BinarySection> sections)
	{
		Sections = sections;
	}
}