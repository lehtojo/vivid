using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;

public enum ElfObjectFileType : short
{
	RELOCATABLE = 0x01,
	EXECUTABLE = 0x02,
	DYNAMIC = 0x03
}

public enum ElfMachineType : short
{
	X64 = 0x3E,
	ARM64 = 0xB7
}

public enum ElfSegmentType : int
{
	LOADABLE = 0x01,
	PROGRAM_HEADER = 0x06
}

public enum ElfSegmentFlag : int
{
	EXECUTE = 0x01,
	WRITE = 0x02,
	READ = 0x04
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ElfFileHeader
{
	public const int Size = 0x40;

	public uint MagicNumber { get; set; } = 1179403647; // 0x464c457F
	public byte Class { get; set; } = 2;
	public byte Endianness { get; set; } = 1;
	public byte Version { get; set; } = 1;
	public byte OsAbi { get; set; } = 0;
	public byte AbiVersion { get; set; } = 0;
	public int Padding1 { get; set; } = 0;
	public short Padding2 { get; set; } = 0;
	public byte Padding3 { get; set; } = 0;
	public ElfObjectFileType Type { get; set; }
	public ElfMachineType Machine { get; set; }
	public int Version2 { get; set; } = 1;
	public ulong Entry { get; set; } = 0;
	public ulong ProgramHeaderOffset { get; set; }
	public ulong SectionHeaderOffset { get; set; }
	public int Flags { get; set; }
	public short FileHeaderSize { get; set; }
	public short ProgramHeaderSize { get; set; }
	public short ProgramHeaderEntryCount { get; set; }
	public short SectionHeaderSize { get; set; }
	public short SectionHeaderTableEntryCount { get; set; }
	public short SectionNameEntryIndex { get; set; }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ElfProgramHeader
{
	public const int Size = 0x38;

	public ElfSegmentType Type { get; set; }
	public ElfSegmentFlag Flags { get; set; }
	public ulong Offset { get; set; }
	public ulong VirtualAddress { get; set; }
	public ulong PhysicalAddress { get; set; }
	public ulong SegmentFileSize { get; set; }
	public ulong SegmentMemorySize { get; set; }
	public ulong Alignment { get; set; }
}

public enum ElfSectionType : int
{
	NONE = 0x00,
	PROGRAM_DATA = 0x1,
	SYMBOL_TABLE = 0x02,
	STRING_TABLE = 0x03,
	RELOCATION_TABLE = 0x04,
}

public enum ElfSectionFlag : int
{
	NONE = 0x00,
	WRITE = 0x01,
	ALLOCATE = 0x02,
	EXECUTABLE = 0x04,
	INFO_LINK = 0x40
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ElfSectionHeader
{
	public const int Size = 0x40;

	public int Name { get; set; }
	public ElfSectionType Type { get; set; }
	public ulong Flags { get; set; }
	public ulong VirtualAddress { get; set; } = 0;
	public ulong Offset { get; set; }
	public ulong SectionFileSize { get; set; }
	public int Link { get; set; } = 0;
	public int Info { get; set; } = 0;
	public ulong Alignment { get; set; } = 1;
	public ulong EntrySize { get; set; } = 0;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ElfStringTable
{
	public List<string> Items { get; } = new List<string>();
	public int Position { get; set; } = 0;

	public int Add(string item)
	{
		var position = Position;
		Items.Add(item);
		Position += item.Length + 1;
		return position;
	}

	public byte[] Export()
	{
		return Encoding.UTF8.GetBytes(string.Join('\0', Items) + '\0');
	}
}

public enum ElfSymbolBinding : int
{
	LOCAL = 0x00,
	GLOBAL = 0x01
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ElfSymbolEntry
{
	public const int Size = 24;

	public uint Name { get; set; } = 0;
	public byte Info { get; set; } = 0;
	public byte Other { get; set; } = 0;
	public ushort SectionIndex { get; set; } = 0;
	public ulong Value { get; set; } = 0;
	public ulong SymbolSize { get; set; } = 0;

	public void SetInfo(ElfSymbolBinding binding, int type)
	{
		Info = (byte)(((int)binding << 4) | type);
	}
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ElfRelocationEntry
{
	public const int Size = 24;

	public ulong Offset { get; set; }
	public ulong Info { get; set; } = 0;
	public long Addend { get; set; }

	public ElfRelocationEntry(ulong offset, long addend)
	{
		Offset = offset;
		Addend = addend;
	}

	public void SetInfo(uint symbol, uint type)
	{
		Info = ((ulong)symbol << 32) | (ulong)type;
	}
}

public static class ElfFormat
{
	public const string SECTION_HEADER_STRING_TABLE_SECTION = ".shstrtab";
	public const string STRING_TABLE_SECTION = ".strtab";
	public const string SYMBOL_TABLE_SECTION = ".symtab";
	public const string TEXT_RELOCATION_TABLE_SECTION = ".rela.text";
	public const string DATA_RELOCATION_TABLE_SECTION = ".rela.data";
	public const string TEXT_SECTION = ".text";
	public const string DATA_SECTION = ".data";
	public const string RELOCATION_TABLE_SECTION_PREFIX = ".rela";

	public static string GetSectionDefaultName(BinarySectionType type)
	{
		return type switch
		{
			BinarySectionType.DATA => DATA_SECTION,
			BinarySectionType.RELOCATION_TABLE => TEXT_RELOCATION_TABLE_SECTION,
			BinarySectionType.STRING_TABLE => STRING_TABLE_SECTION,
			BinarySectionType.SYMBOL_TABLE => SYMBOL_TABLE_SECTION,
			BinarySectionType.TEXT => TEXT_SECTION,
			_ => string.Empty
		};
	}

	public static int Write<T>(byte[] destination, int offset, T source)
	{
		var size = Marshal.SizeOf(source);
		var buffer = Marshal.AllocHGlobal(size);
		Marshal.StructureToPtr(source!, buffer, false);
		Marshal.Copy(buffer, destination, offset, size);
		Marshal.FreeHGlobal(buffer);
		return offset + size;
	}

	public static void Write<T>(byte[] destination, int offset, List<T> source)
	{
		foreach (var element in source)
		{
			offset = Write(destination, offset, element);
		}
	}

	public static ElfSectionType GetSectionType(BinarySection section)
	{
		return section.Type switch
		{
			BinarySectionType.DATA => ElfSectionType.PROGRAM_DATA,
			BinarySectionType.NONE => ElfSectionType.NONE,
			BinarySectionType.RELOCATION_TABLE => ElfSectionType.RELOCATION_TABLE,
			BinarySectionType.STRING_TABLE => ElfSectionType.STRING_TABLE,
			BinarySectionType.SYMBOL_TABLE => ElfSectionType.SYMBOL_TABLE,
			BinarySectionType.TEXT => ElfSectionType.PROGRAM_DATA,
			_ => ElfSectionType.PROGRAM_DATA
		};
	}

	public static ElfSectionFlag GetSectionFlags(BinarySection section)
	{
		return section.Type switch
		{
			BinarySectionType.DATA => ElfSectionFlag.ALLOCATE | ElfSectionFlag.WRITE,
			BinarySectionType.RELOCATION_TABLE => ElfSectionFlag.INFO_LINK,
			BinarySectionType.TEXT => ElfSectionFlag.ALLOCATE | ElfSectionFlag.EXECUTABLE,
			_ => ElfSectionFlag.NONE
		};
	}

	public static List<ElfSectionHeader> CreateSectionHeaders(List<BinarySection> sections, Dictionary<string, BinarySymbol> symbols, int file_position = ElfFileHeader.Size)
	{
		var string_table = new ElfStringTable();
		var headers = new List<ElfSectionHeader>();

		foreach (var section in sections)
		{
			var header = new ElfSectionHeader();
			header.Name = string_table.Add(section.Name);
			header.Type = GetSectionType(section);
			header.Flags = (ulong)GetSectionFlags(section);
			header.VirtualAddress = (uint)section.VirtualAddress;
			header.SectionFileSize = (ulong)section.Size;
			
			if (section.Type == BinarySectionType.RELOCATION_TABLE)
			{
				// The section header of relocation table should be linked to the symbol table and its info should point to the text section, since it describes it
				header.Link = sections.FindIndex(i => i.Type == BinarySectionType.SYMBOL_TABLE);
				header.Info = sections.FindIndex(i => i.Name == section.Name.Substring(RELOCATION_TABLE_SECTION_PREFIX.Length));
				header.EntrySize = ElfRelocationEntry.Size;
			}
			else if (section.Type == BinarySectionType.SYMBOL_TABLE)
			{
				// The section header of symbol table should be linked to the string table 
				header.Link = sections.FindIndex(i => i.Type == BinarySectionType.STRING_TABLE);
				header.Info = symbols.Values.Count(i => !i.External) + 1;
				header.EntrySize = ElfSymbolEntry.Size;
			}
			
			section.Offset = file_position;
			header.Offset = (ulong)file_position;

			file_position += section.Size;

			headers.Add(header);
		}

		var string_table_section_name = string_table.Add(SECTION_HEADER_STRING_TABLE_SECTION);
		var string_table_section = new BinarySection(SECTION_HEADER_STRING_TABLE_SECTION, BinarySectionType.STRING_TABLE, string_table.Export());
		var string_table_header = new ElfSectionHeader();

		string_table_section.Offset = file_position;

		string_table_header.Name = string_table_section_name;
		string_table_header.Type = ElfSectionType.STRING_TABLE;
		string_table_header.Flags = 0;
		string_table_header.Offset = (ulong)file_position;
		string_table_header.SectionFileSize = (ulong)string_table_section.Data.Length;
		
		sections.Add(string_table_section);
		headers.Add(string_table_header);

		return headers;
	}

	public static uint GetElfSymbolType(BinaryRelocationType type)
	{
		return type switch
		{
			BinaryRelocationType.PROCEDURE_LINKAGE_TABLE => 4,
			BinaryRelocationType.PROGRAM_COUNTER => 2,
			_ => 0
		};
	}

	/// <summary>
	/// Creates the symbol table and the relocation table based on the specified symbols
	/// </summary>
	public static ElfStringTable CreateSymbolRelatedSections(List<BinarySection> sections)
	{
		// Create a string table that contains the names of the specified symbols
		var symbol_name_table = new ElfStringTable();
		var symbol_entries = new List<ElfSymbolEntry>();
		var relocation_sections = new Dictionary<BinarySection, List<ElfRelocationEntry>>();

		// Add a none-symbol
		var none_symbol = new ElfSymbolEntry();
		none_symbol.Name = (uint)symbol_name_table.Add(string.Empty);
		symbol_entries.Add(none_symbol);

		// Index the sections
		for (var i = 0; i < sections.Count; i++)
		{
			var section = sections[i];

			foreach (var symbol in section.Symbols.Values)
			{
				var virtual_address = symbol.Section == null ? 0 : symbol.Section.VirtualAddress;

				var symbol_entry = new ElfSymbolEntry();
				symbol_entry.Name = (uint)symbol_name_table.Add(symbol.Name);
				symbol_entry.Value = (ulong)(virtual_address + symbol.Offset);
				symbol_entry.SectionIndex = (ushort)(symbol.External ? 0 : i);
				symbol_entry.SetInfo(symbol.External ? ElfSymbolBinding.GLOBAL : ElfSymbolBinding.LOCAL, 0);

				symbol.Index = (uint)symbol_entries.Count;
				symbol_entries.Add(symbol_entry);
			}

			var relocation_entries = new List<ElfRelocationEntry>();

			foreach (var relocation in section.Relocations)
			{
				var relocation_entry = new ElfRelocationEntry((ulong)relocation.Offset, relocation.Addend);
				relocation_entry.SetInfo(relocation.Symbol.Index, GetElfSymbolType(relocation.Type));

				relocation_entries.Add(relocation_entry);
			}

			relocation_sections[section] = relocation_entries;
		}

		var symbol_table_section = new BinarySection(SYMBOL_TABLE_SECTION, BinarySectionType.SYMBOL_TABLE, new byte[ElfSymbolEntry.Size * symbol_entries.Count]);
		Write(symbol_table_section.Data, 0, symbol_entries);
		sections.Add(symbol_table_section);

		foreach (var iterator in relocation_sections)
		{
			// Add the relocation section if needed
			var relocation_entries = iterator.Value;
			if (!relocation_entries.Any()) continue;

			var relocation_table_section = new BinarySection(RELOCATION_TABLE_SECTION_PREFIX + iterator.Key.Name, BinarySectionType.RELOCATION_TABLE, new byte[ElfRelocationEntry.Size * relocation_entries.Count]);
			Write(relocation_table_section.Data, 0, relocation_entries);
			sections.Add(relocation_table_section);
		}

		var string_table_section = new BinarySection(STRING_TABLE_SECTION, BinarySectionType.STRING_TABLE, symbol_name_table.Export());
		sections.Add(string_table_section);

		return symbol_name_table;
	}

	/// <summary>
	/// Returns a list of all symbols in the specified sections
	/// </summary>
	public static Dictionary<string, BinarySymbol> GetAllSymbolsFromSections(List<BinarySection> sections)
	{
		var symbols = new Dictionary<string, BinarySymbol>();

		foreach (var symbol in sections.SelectMany(i => i.Symbols.Values))
		{
			// 1. Just continue, if the symbol can be added
			// 2. If this is executed, it means that some version of the current symbol is already added.
			// However, if the current symbol is external, it does not matter.
			if (symbols.TryAdd(symbol.Name, symbol) || symbol.External) continue;

			// If the version of the current symbol in the dictionary is not external, the current symbol is defined at least twice
			var conflict = symbols[symbol.Name];
			if (!conflict.External) throw new ApplicationException($"Symbol {symbol.Name} is created at least twice");

			// Since the version of the current symbol in the dictionary is external, it can be replaced with the actual definition (current symbol)
			symbols[symbol.Name] = symbol;
		}

		return symbols;
	}

	public static BinaryObjectFile CreateObjectX64(Dictionary<string, BinarySymbol> symbols, List<BinaryRelocation> relocations, List<BinarySection> sections)
	{
		// Create an empty section, so that it is possible to leave section index unspecified in symbols for example
		var none_section = new BinarySection(string.Empty, BinarySectionType.NONE, Array.Empty<byte>());
		sections.Insert(0, none_section);

		// Add symbols and relocations of each section needing that
		CreateSymbolRelatedSections(sections);

		_ = CreateSectionHeaders(sections, GetAllSymbolsFromSections(sections));

		//#error Check these relocations, they probably discard some stuff
		return new BinaryObjectFile(sections);
	}

	public static byte[] BuildObjectX64(Dictionary<string, BinarySymbol> symbols, List<BinaryRelocation> relocations, List<BinarySection> sections)
	{
		// Create an empty section, so that it is possible to leave section index unspecified in symbols for example
		var none_section = new BinarySection(string.Empty, BinarySectionType.NONE, Array.Empty<byte>());
		sections.Insert(0, none_section);

		// Add symbols and relocations of each section needing that
		CreateSymbolRelatedSections(sections);

		var header = new ElfFileHeader();
		header.Type = ElfObjectFileType.RELOCATABLE;
		header.Machine = ElfMachineType.X64;
		header.FileHeaderSize = ElfFileHeader.Size;
		header.SectionHeaderSize = ElfSectionHeader.Size;

		var section_headers = CreateSectionHeaders(sections, symbols);
		var section_bytes = sections.Sum(i => i.Data.Length);

		// Save the location of the section header table
		header.SectionHeaderOffset = (ulong)(ElfFileHeader.Size + section_bytes);
		header.SectionHeaderTableEntryCount = (short)section_headers.Count;
		header.SectionHeaderSize = ElfSectionHeader.Size;
		header.SectionNameEntryIndex = (short)(section_headers.Count - 1);

		var bytes = ElfFileHeader.Size + section_bytes + section_headers.Count * ElfSectionHeader.Size;
		var result = new byte[bytes];

		// Write the file header
		Write(result, 0, header);

		// Write the actual program data
		foreach (var section in sections)
		{
			Array.Copy(section.Data, 0, result, section.Offset, section.Data.Length);
		}

		// Write the section header table now
		var position = (int)header.SectionHeaderOffset;

		foreach (var section_header in section_headers)
		{
			Write(result, position, section_header);
			position += ElfSectionHeader.Size;
		}

		return result;
	}
}