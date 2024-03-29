using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
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
	DYNAMIC = 0x02,
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
	HASH = 0x05,
	DYNAMIC = 0x06,
	DYNAMIC_SYMBOLS = 0x0B
}

[Flags]
public enum ElfSectionFlags : int
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

	public bool IsExported => (ElfSymbolBinding)(Info >> 4) == ElfSymbolBinding.GLOBAL && SectionIndex != 0;
}

public enum ElfSymbolType
{
	NONE = 0x00,
	ABSOLUTE64 = 0x01,
	PROGRAM_COUNTER_RELATIVE = 0x02,
	PLT32 = 0x04,
	BASE_RELATIVE_64 = 0x08,
	ABSOLUTE32 = 0x0A
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ElfRelocationEntry
{
	public const int Size = 24;

	public ulong Offset { get; set; }
	public ulong Info { get; set; } = 0;
	public long Addend { get; set; }

	public int Symbol => (int)(Info >> 32);
	public int Type => (int)(Info & 0xFFFFFFFF);

	public ElfRelocationEntry() {}

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

public enum ElfDynamicSectionTag
{
	HashTable = 0x04,
	StringTable = 0x05,
	SymbolTable = 0x06,
	RelocationTable = 0x07,
	RelocationTableSize = 0x08,
	RelocationEntrySize = 0x09,
	StringTableSize = 0x0A,
	SymbolEntrySize = 0x0B,
	RelocationCount = 0x6ffffff9
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ElfDynamicEntry
{
	public const int Size = 16;
	public const int PointerOffset = sizeof(ulong);

	public ulong Tag { get; set; }
	public ulong Value { get; set; }

	public ElfDynamicEntry(ElfDynamicSectionTag tag, ulong value)
	{
		Tag = (ulong)tag;
		Value = value;
	}
}

public static class ElfFormat
{
	public const string TEXT_SECTION = ".text";
	public const string DATA_SECTION = ".data";
	public const string SYMBOL_TABLE_SECTION = ".symtab";
	public const string STRING_TABLE_SECTION = ".strtab";
	public const string SECTION_HEADER_STRING_TABLE_SECTION = ".shstrtab";
	public const string DYNAMIC_SECTION = ".dynamic";
	public const string DYNAMIC_SYMBOL_TABLE_SECTION = ".dynsym";
	public const string DYNAMIC_STRING_TABLE_SECTION = ".dynstr";
	public const string HASH_SECTION = ".hash";
	public const string RELOCATION_TABLE_SECTION_PREFIX = ".rela";
	public const string DYNAMIC_RELOCATIONS_SECTION = RELOCATION_TABLE_SECTION_PREFIX + ".dyn";
	public const string DYNAMIC_SECTION_START = "_DYNAMIC";

	public static ElfSectionType GetSectionType(BinarySection section)
	{
		if (section.Name == ElfFormat.DYNAMIC_SYMBOL_TABLE_SECTION) return ElfSectionType.DYNAMIC_SYMBOLS;

		return section.Type switch
		{
			BinarySectionType.DATA => ElfSectionType.PROGRAM_DATA,
			BinarySectionType.NONE => ElfSectionType.NONE,
			BinarySectionType.RELOCATION_TABLE => ElfSectionType.RELOCATION_TABLE,
			BinarySectionType.STRING_TABLE => ElfSectionType.STRING_TABLE,
			BinarySectionType.SYMBOL_TABLE => ElfSectionType.SYMBOL_TABLE,
			BinarySectionType.TEXT => ElfSectionType.PROGRAM_DATA,
			BinarySectionType.DYNAMIC => ElfSectionType.DYNAMIC,
			BinarySectionType.HASH => ElfSectionType.HASH,
			_ => ElfSectionType.PROGRAM_DATA
		};
	}

	public static ElfSectionFlags GetSectionFlags(BinarySection section)
	{
		var result = ElfSectionFlags.NONE;

		if (section.Type == BinarySectionType.RELOCATION_TABLE && section.Name != ElfFormat.DYNAMIC_RELOCATIONS_SECTION) { result |= ElfSectionFlags.INFO_LINK; }

		if (section.Flags.HasFlag(BinarySectionFlags.WRITE)) { result |= ElfSectionFlags.WRITE; }
		if (section.Flags.HasFlag(BinarySectionFlags.EXECUTE)) { result |= ElfSectionFlags.EXECUTABLE; }
		if (section.Flags.HasFlag(BinarySectionFlags.ALLOCATE)) { result |= ElfSectionFlags.ALLOCATE; }

		return result;
	}

	/// <summary>
	/// Hashes the specified symbol name.
	/// Implementation is provided by the ELF specification.
	/// </summary>
	public static ulong GetSymbolHash(string name)
	{
		var h = 0UL;

		for (var i = 0; i < name.Length; i++)
		{
			h = (h << 4) + name[i];
			var g = h & 0xF0000000;
			if (g != 0) { h ^= g >> 24;}
			h &= ~g;
		}

		return h;
	}

	/// <summary>
	/// Generates a hash section from the specified symbols
	/// </summary>
	public static BinarySection CreateHashSection(List<BinarySymbol> symbols)
	{
		var buckets = new int[symbols.Count];
		var chains = new int[symbols.Count];
		var ends = new int[symbols.Count];

		// Initialize the chain end indices to match the beginning of the chain
		for (var i = 0; i < symbols.Count; i++) { ends[i] = i; }

		for (var i = 0; i < symbols.Count; i++)
		{
			var hash = GetSymbolHash(symbols[i].Name);
			var bucket = (int)(hash % (ulong)buckets.Length);
			
			if (buckets[bucket] == 0)
			{
				buckets[bucket] = i;
				ends[bucket] = i;
			}
			else
			{
				// Load the last symbol index in the chain
				var end = ends[bucket];

				chains[end] = i; // Set the next symbol index in the chain to point to the current symbol
				chains[i] = 0; // Set the chain to stop at the current symbol

				ends[bucket] = i; // Set the end of the chain to be the current symbol
			}
		}

		// Structure of hash section:
		// [Number of buckets]
		// [Number of chains]
		// [Buckets]
		// [Chains]

		// Convert the buckets and chains to bytes
		var data = new byte[(2 + buckets.Length + chains.Length) * sizeof(int)];
		BinaryUtility.WriteInt32(data, 0, buckets.Length);
		BinaryUtility.WriteInt32(data, sizeof(int), chains.Length);

		// Copy the buckets and chains to the data
		Buffer.BlockCopy(buckets, 0, data, sizeof(int) * 2, buckets.Length * sizeof(int));
		Buffer.BlockCopy(chains,  0, data, (buckets.Length + 2) * sizeof(int), chains.Length * sizeof(int));

		return new BinarySection(ElfFormat.HASH_SECTION, BinarySectionType.HASH, data);
	}

	public static List<ElfSectionHeader> CreateSectionHeaders(List<BinarySection> sections, Dictionary<string, BinarySymbol> symbols, int file_position = ElfFileHeader.Size)
	{
		var string_table = new BinaryStringTable();
		var headers = new List<ElfSectionHeader>();

		foreach (var section in sections)
		{
			// Apply the section margin before doing anything
			file_position += section.Margin;

			var header = new ElfSectionHeader();
			header.Name = string_table.Add(section.Name);
			header.Type = GetSectionType(section);
			header.Flags = (ulong)GetSectionFlags(section);
			header.VirtualAddress = (uint)section.VirtualAddress;
			header.SectionFileSize = (ulong)section.LoadSize;
			header.Alignment = (ulong)section.Alignment;

			if (section.Name == ElfFormat.DYNAMIC_RELOCATIONS_SECTION)
			{
				// The section header of relocation table should be linked to the dynamic symbol table
				header.Link = sections.FindIndex(i => i.Name == ElfFormat.DYNAMIC_SYMBOL_TABLE_SECTION);
				header.Info = 0;
				header.EntrySize = ElfRelocationEntry.Size;
			}
			else if (section.Type == BinarySectionType.RELOCATION_TABLE)
			{
				// The section header of relocation table should be linked to the symbol table and its info should point to the text section, since it describes it
				header.Link = sections.FindIndex(i => i.Name == ElfFormat.SYMBOL_TABLE_SECTION);
				header.Info = sections.FindIndex(i => i.Name == section.Name.Substring(RELOCATION_TABLE_SECTION_PREFIX.Length));
				header.EntrySize = ElfRelocationEntry.Size;
			}
			else if (section.Name == ElfFormat.SYMBOL_TABLE_SECTION)
			{
				// The section header of symbol table should be linked to the string table 
				header.Link = sections.FindIndex(i => i.Name == ElfFormat.STRING_TABLE_SECTION);
				header.Info = symbols.Values.Count(i => !i.External) + 1;
				header.EntrySize = ElfSymbolEntry.Size;
			}
			else if (section.Name == ElfFormat.DYNAMIC_SYMBOL_TABLE_SECTION)
			{
				// The section header of dynamic symbol table should be linked to the dynamic string table
				header.Link = sections.FindIndex(i => i.Name == ElfFormat.DYNAMIC_STRING_TABLE_SECTION);
				header.Info = 1;
				header.EntrySize = ElfSymbolEntry.Size;
			}
			else if (section.Name == ElfFormat.DYNAMIC_SECTION)
			{
				header.Link = sections.FindIndex(i => i.Name == ElfFormat.DYNAMIC_STRING_TABLE_SECTION);
				header.Info = 0;
				header.EntrySize = ElfDynamicEntry.Size;
			}
			else if (section.Name == ElfFormat.HASH_SECTION)
			{
				header.Link = sections.FindIndex(i => i.Name == ElfFormat.DYNAMIC_SYMBOL_TABLE_SECTION);
				header.Info = 0;
				header.EntrySize = 4;
			}

			section.Offset = file_position;

			if (section.Type == BinarySectionType.NONE)
			{
				header.Alignment = 0;
				header.Offset = 0;
			}
			else
			{
				header.Offset = (ulong)file_position;
			}

			file_position += section.VirtualSize;

			headers.Add(header);
		}

		// Align the file position
		var aligned_file_position = (file_position + 16 - 1) & (-16);

		var string_table_section_name = string_table.Add(SECTION_HEADER_STRING_TABLE_SECTION);

		var string_table_section = new BinarySection(SECTION_HEADER_STRING_TABLE_SECTION, BinarySectionType.STRING_TABLE, string_table.Export());
		string_table_section.Margin = aligned_file_position - file_position;
		string_table_section.Offset = aligned_file_position;

		var string_table_header = new ElfSectionHeader();
		string_table_header.Name = string_table_section_name;
		string_table_header.Type = ElfSectionType.STRING_TABLE;
		string_table_header.Flags = 0;
		string_table_section.Offset = aligned_file_position;
		string_table_header.Offset = (ulong)aligned_file_position;
		string_table_header.SectionFileSize = (ulong)string_table_section.Data.Length;

		sections.Add(string_table_section);
		headers.Add(string_table_header);

		return headers;
	}

	/// <summary>
	/// Converts the specified relocation type into ELF symbol type
	/// </summary>
	public static uint GetSymbolType(BinaryRelocationType type)
	{
		return type switch
		{
			BinaryRelocationType.PROCEDURE_LINKAGE_TABLE => (uint)ElfSymbolType.PROGRAM_COUNTER_RELATIVE, // Redirect to PC32 for now
			BinaryRelocationType.PROGRAM_COUNTER_RELATIVE => (uint)ElfSymbolType.PROGRAM_COUNTER_RELATIVE,
			BinaryRelocationType.ABSOLUTE64 => (uint)ElfSymbolType.ABSOLUTE64,
			BinaryRelocationType.ABSOLUTE32 => (uint)ElfSymbolType.ABSOLUTE32,
			_ => (uint)ElfSymbolType.NONE
		};
	}

	/// <summary>
	/// Converts the specified ELF symbol type to relocation type
	/// </summary>
	public static BinaryRelocationType GetRelocationTypeFromSymbolType(ElfSymbolType type)
	{
		return type switch
		{
			ElfSymbolType.PROGRAM_COUNTER_RELATIVE => BinaryRelocationType.PROGRAM_COUNTER_RELATIVE,
			ElfSymbolType.PLT32 => BinaryRelocationType.PROGRAM_COUNTER_RELATIVE,
			ElfSymbolType.ABSOLUTE64 => BinaryRelocationType.ABSOLUTE64,
			ElfSymbolType.ABSOLUTE32 => BinaryRelocationType.ABSOLUTE32,
			_ => BinaryRelocationType.ABSOLUTE32
		};
	}

	/// <summary>
	/// Converts the specified ELF section flags to shared section flags
	/// </summary>
	public static BinarySectionFlags GetSharedSectionFlags(ElfSectionFlags flags)
	{
		var result = (BinarySectionFlags)0;

		if (flags.HasFlag(ElfSectionFlags.WRITE)) { result |= BinarySectionFlags.WRITE; }
		if (flags.HasFlag(ElfSectionFlags.EXECUTABLE)) { result |= BinarySectionFlags.EXECUTE; }
		if (flags.HasFlag(ElfSectionFlags.ALLOCATE)) { result |= BinarySectionFlags.ALLOCATE; }

		return result;
	}

	/// <summary>
	/// Creates the symbol table and the relocation table based on the specified symbols
	/// </summary>
	public static BinaryStringTable CreateSymbolRelatedSections(List<BinarySection> sections, List<BinarySection>? fragments, Dictionary<string, BinarySymbol> symbols)
	{
		// Create a string table that contains the names of the specified symbols
		var symbol_name_table = new BinaryStringTable();
		var symbol_entries = new List<ElfSymbolEntry>();
		var relocation_sections = new Dictionary<BinarySection, List<ElfRelocationEntry>>();

		// Add a none-symbol
		var none_symbol = new ElfSymbolEntry();
		none_symbol.Name = (uint)symbol_name_table.Add(string.Empty);
		symbol_entries.Add(none_symbol);

		// Index the sections since the symbols need that
		for (var i = 0; i < sections.Count; i++)
		{
			var section = sections[i];
			section.Index = i;

			if (fragments == null) continue;
			
			// Index the section fragments as well
			foreach (var fragment in fragments)
			{
				if (fragment.Name != section.Name) continue;
				fragment.Index = i;
			}
		}

		// Order the symbols so that local symbols are first and then the external ones
		foreach (var symbol in symbols.Values.Where(i => !(i.External || i.Export)).Concat(symbols.Values.Where(i => i.External || i.Export)))
		{
			var virtual_address = symbol.Section == null ? 0 : symbol.Section.VirtualAddress;

			var symbol_entry = new ElfSymbolEntry();
			symbol_entry.Name = (uint)symbol_name_table.Add(symbol.Name);
			symbol_entry.Value = (ulong)(virtual_address + symbol.Offset);
			symbol_entry.SectionIndex = (ushort)(symbol.External ? 0 : symbol.Section!.Index);
			symbol_entry.SetInfo(symbol.External || symbol.Export ? ElfSymbolBinding.GLOBAL : ElfSymbolBinding.LOCAL, 0);

			symbol.Index = (uint)symbol_entries.Count;
			symbol_entries.Add(symbol_entry);
		}

		// Create the relocation entries
		foreach (var section in sections)
		{
			var relocation_entries = new List<ElfRelocationEntry>();

			foreach (var relocation in section.Relocations)
			{
				var relocation_entry = new ElfRelocationEntry((ulong)relocation.Offset, relocation.Addend);
				relocation_entry.SetInfo(relocation.Symbol.Index, GetSymbolType(relocation.Type));

				relocation_entries.Add(relocation_entry);
			}

			relocation_sections[section] = relocation_entries;
		}

		var symbol_table_section = new BinarySection(SYMBOL_TABLE_SECTION, BinarySectionType.SYMBOL_TABLE, new byte[ElfSymbolEntry.Size * symbol_entries.Count]);
		BinaryUtility.Write(symbol_table_section.Data, 0, symbol_entries);
		sections.Add(symbol_table_section);

		foreach (var iterator in relocation_sections)
		{
			// Add the relocation section if needed
			var relocation_entries = iterator.Value;
			if (!relocation_entries.Any()) continue;

			var relocation_table_section = new BinarySection(RELOCATION_TABLE_SECTION_PREFIX + iterator.Key.Name, BinarySectionType.RELOCATION_TABLE, new byte[ElfRelocationEntry.Size * relocation_entries.Count]);
			BinaryUtility.Write(relocation_table_section.Data, 0, relocation_entries);
			sections.Add(relocation_table_section);
		}

		var string_table_section = new BinarySection(STRING_TABLE_SECTION, BinarySectionType.STRING_TABLE, symbol_name_table.Export());
		sections.Add(string_table_section);

		return symbol_name_table;
	}

	/// <summary>
	/// Creates an object file from the specified sections
	/// </summary>
	public static BinaryObjectFile Create(string name, List<BinarySection> sections, HashSet<string> exports)
	{
		// Create an empty section, so that it is possible to leave section index unspecified in symbols for example
		var none_section = new BinarySection(string.Empty, BinarySectionType.NONE, Array.Empty<byte>());
		sections.Insert(0, none_section);

		var symbols = BinaryUtility.GetAllSymbolsFromSections(sections);

		// Export symbols
		BinaryUtility.ApplyExports(symbols, exports);

		// Update all the relocations before adding them to binary sections
		BinaryUtility.UpdateRelocations(sections, symbols);

		// Add symbols and relocations of each section needing that
		CreateSymbolRelatedSections(sections, null, symbols);

		_ = CreateSectionHeaders(sections, symbols);

		// Now that section positions are set, compute offsets
		BinaryUtility.ComputeOffsets(sections, symbols);

		return new BinaryObjectFile(name, sections, symbols.Values.Where(i => i.Export).Select(i => i.Name).ToHashSet());
	}

	/// <summary>
	/// Aligns the specified sections
	/// </summary>
	public static void AlignSections(List<BinarySection> sections, long file_position)
	{
		foreach (var section in sections)
		{
			if (section.Type == BinarySectionType.NONE) continue;

			var alignment = Math.Max(section.Alignment, 16);
			var aligned_file_position = (file_position + alignment - 1) & (-alignment);

			section.Margin = (int)(aligned_file_position - file_position);
			section.Alignment = alignment;

			file_position = aligned_file_position + section.Data.Length;
		}
	}

	/// <summary>
	/// Creates an object file from the specified sections and converts it to binary format
	/// </summary>
	public static byte[] Build(List<BinarySection> sections, HashSet<string> exports)
	{
		if (sections.Count == 0 || sections.First().Type != BinarySectionType.NONE)
		{
			// Create an empty section, so that it is possible to leave section index unspecified in symbols for example
			var none_section = new BinarySection(string.Empty, BinarySectionType.NONE, Array.Empty<byte>());
			sections.Insert(0, none_section);
		}

		var symbols = BinaryUtility.GetAllSymbolsFromSections(sections);

		// Export symbols
		BinaryUtility.ApplyExports(symbols, exports);

		// Update all the relocations before adding them to binary sections
		BinaryUtility.UpdateRelocations(sections, symbols);

		// Add symbols and relocations of each section needing that
		CreateSymbolRelatedSections(sections, null, symbols);

		var header = new ElfFileHeader();
		header.Type = ElfObjectFileType.RELOCATABLE;
		header.Machine = ElfMachineType.X64;
		header.FileHeaderSize = ElfFileHeader.Size;
		header.SectionHeaderSize = ElfSectionHeader.Size;

		AlignSections(sections, ElfFileHeader.Size);
		var section_headers = CreateSectionHeaders(sections, symbols);

		// Now that section positions are set, compute offsets
		BinaryUtility.ComputeOffsets(sections, symbols);

		// Compute the location of the first section header
		header.SectionHeaderOffset = ElfFileHeader.Size;

		foreach (var section in sections)
		{
			header.SectionHeaderOffset += (ulong)(section.Margin + section.Data.Length);
		}

		// Save the location of the section header table
		header.SectionHeaderTableEntryCount = (short)section_headers.Count;
		header.SectionHeaderSize = ElfSectionHeader.Size;
		header.SectionNameEntryIndex = (short)(section_headers.Count - 1);

		var bytes = (int)header.SectionHeaderOffset + section_headers.Count * ElfSectionHeader.Size;
		var result = new byte[bytes];

		// Write the file header
		BinaryUtility.Write(result, 0, header);

		// Write the actual program data
		foreach (var section in sections)
		{
			Array.Copy(section.Data, 0, result, section.Offset, section.Data.Length);
		}

		// Write the section header table now
		var position = (int)header.SectionHeaderOffset;

		foreach (var section_header in section_headers)
		{
			BinaryUtility.Write(result, position, section_header);
			position += ElfSectionHeader.Size;
		}

		return result;
	}

	/// <summary>
	/// Packs the sections of the specified objects into a raw binary file 
	/// </summary>
	public static byte[] BuildBinaryFile(List<BinaryObjectFile> objects)
	{
		// Index all the specified object files
		for (var i = 0; i < objects.Count; i++) { objects[i].Index = i; }

		// Make all hidden symbols unique by using their object file indices
		Linker.MakeLocalSymbolsUnique(objects);

		// Resolves are unresolved symbols and returns all symbols as a list
		var symbols = Linker.ResolveSymbols(objects);

		// Ensure sections are ordered so that sections of same type are next to each other
		var fragments = objects.SelectMany(i => i.Sections).Where(i => Linker.IsLoadableSection(i)).ToList();

		// Load all the relocations from all the sections
		var relocations = objects.SelectMany(i => i.Sections).SelectMany(i => i.Relocations).ToList();

		// Ensure are relocations are resolved
		foreach (var relocation in relocations)
		{
			if (!relocation.Symbol.External) continue;
			throw new ApplicationException("Symbol " + relocation.Symbol.Name + " is not defined locally or externally");
		}

		// Create sections, which cover the fragmented sections
		var overlays = Linker.CreateLoadableSections(fragments);

		// Compute virtual addresses for the sections
		Linker.CreateProgramHeaders(overlays, fragments, new List<ElfProgramHeader>(), Settings.BaseAddress, false);

		// Now that sections have their virtual addresses relocations can be computed
		Linker.ComputeRelocations(relocations, 0);

		// Compute the number of bytes sections take up
		var bytes = 0;
		foreach (var overlay in overlays) { bytes += overlay.Margin + overlay.VirtualSize; }

		var result = new byte[bytes];

		// Write the fragments
		foreach (var fragment in fragments)
		{
			Buffer.BlockCopy(fragment.Data, 0, result, fragment.Offset, fragment.Data.Length);
		}

		return result;
	}

	/// <summary>
	/// Creates symbol and relocation objects from the raw data inside the specified sections
	/// </summary>
	public static void ImportSymbolsAndRelocations(List<BinarySection> sections, List<KeyValuePair<ElfSectionHeader, byte[]>> section_intermediates)
	{
		// Try to find the symbol table section
		var symbol_table_index = sections.FindIndex(i => i.Type == BinarySectionType.SYMBOL_TABLE);
		if (symbol_table_index < 0) return;

		var symbol_table_section = sections[symbol_table_index];

		// Copy the symbol table into raw memory
		var symbol_table = Marshal.AllocHGlobal(symbol_table_section.Data.Length);
		Marshal.Copy(symbol_table_section.Data, 0, symbol_table, symbol_table_section.Data.Length);

		// Load all the symbol entries from the symbol table
		var symbol_entries = new List<ElfSymbolEntry>();
		var position = 0;

		while (position < symbol_table_section.Data.Length)
		{
			symbol_entries.Add(Marshal.PtrToStructure<ElfSymbolEntry>(symbol_table + position) ?? throw new ApplicationException("Could not load a symbol entry from the symbol table"));
			position += ElfSymbolEntry.Size;
		}

		// Determine the section, which contains the symbol names
		var section_header = section_intermediates[symbol_table_index].Key;
		var symbol_names = sections[section_header.Link].Data;

		// Create the a list of the symbols, which contains the loaded symbols in the order in which they appear in the file
		/// NOTE: This is useful for the relocation table below
		var symbols = new List<BinarySymbol>();

		// Convert the symbol entries into symbols
		foreach (var symbol_entry in symbol_entries)
		{
			// Load the section, which contains the current symbol
			var section = sections[symbol_entry.SectionIndex];

			// Determine the start and the end indices of the symbol name
			var symbol_name_start = (int)symbol_entry.Name;
			var symbol_name_end = symbol_name_start;
			for (; symbol_name_end < symbol_names.Length && symbol_names[symbol_name_end] != 0; symbol_name_end++) { }

			// Load the symbol name
			var symbol_name = Encoding.ASCII.GetString(symbol_names, symbol_name_start, symbol_name_end - symbol_name_start);

			// Sometimes other assemblers give empty names for sections for instance, these are not supported (yet)
			if (string.IsNullOrEmpty(symbol_name)) continue;

			var symbol = new BinarySymbol(symbol_name, (int)symbol_entry.Value, symbol_entry.SectionIndex == 0);
			symbol.Export = symbol_entry.IsExported;
			symbol.Section = section;

			// Add the symbol to the section, which contains it, unless the symbol is external
			if (!symbol.External) section.Symbols.Add(symbol.Name, symbol);

			symbols.Add(symbol);
		}

		// Now, import the relocations
		for (var i = 0; i < sections.Count; i++)
		{
			// Ensure the section represents a relocation table
			var relocation_section = sections[i];
			if (relocation_section.Type != BinarySectionType.RELOCATION_TABLE) continue;

			// Determine the section, which the relocations concern
			var relocation_section_header = section_intermediates[i];
			var section = sections[relocation_section_header.Key.Info];

			// Copy the relocation table into raw memory
			var relocation_table = Marshal.AllocHGlobal(relocation_section.Data.Length);
			Marshal.Copy(relocation_section.Data, 0, relocation_table, relocation_section.Data.Length);

			// Load all the relocation entries
			var relocation_entries = new List<ElfRelocationEntry>();

			position = 0;

			while (position < relocation_section.Data.Length)
			{
				relocation_entries.Add(Marshal.PtrToStructure<ElfRelocationEntry>(relocation_table + position) ?? throw new ApplicationException("Could not load a relocation entry from the relocation table"));
				position += ElfSymbolEntry.Size;
			}

			// Convert the relocation entries into relocation objects
			foreach (var relocation_entry in relocation_entries)
			{
				var symbol = symbols[relocation_entry.Symbol - 1]; // Use -1 because the symbol index is 1-based
				var type = GetRelocationTypeFromSymbolType((ElfSymbolType)relocation_entry.Type);

				var relocation = new BinaryRelocation(symbol, (int)relocation_entry.Offset, (int)relocation_entry.Addend, type);
				relocation.Section = section;

				section.Relocations.Add(relocation);
			}

			Marshal.FreeHGlobal(relocation_table);
		}

		Marshal.FreeHGlobal(symbol_table);
	}
	
	/// <summary>
	/// Load the specified object file and constructs a object structure that represents it
	/// </summary>
	public static BinaryObjectFile Import(string name, byte[] source)
	{
		// Load the file into raw memory
		var bytes = Marshal.AllocHGlobal(source.Length);
		Marshal.Copy(source, 0, bytes, source.Length);

		// Load the file header
		var header = Marshal.PtrToStructure<ElfFileHeader>(bytes) ?? throw new ApplicationException("Could not load the file header");

		// Create a pointer, which points to the start of the section headers
		var section_headers_start = bytes + (int)header.SectionHeaderOffset;

		// Load section intermediates, that is section headers with corresponding section data
		var section_intermediates = new List<KeyValuePair<ElfSectionHeader, byte[]>>();

		for (var i = 0; i < header.SectionHeaderTableEntryCount; i++)
		{
			// Load the section header in order to load the actual section
			var section_header = Marshal.PtrToStructure<ElfSectionHeader>(section_headers_start + ElfSectionHeader.Size * i) ?? throw new ApplicationException("Could not load a section header");

			// Create a pointer, which points to the start of the section data in the file
			var section_data_start = bytes + (int)section_header.Offset;

			// Now load the section data into a buffer
			var section_data = new byte[section_header.SectionFileSize];
			Marshal.Copy(section_data_start, section_data, 0, section_data.Length);

			section_intermediates.Add(new KeyValuePair<ElfSectionHeader, byte[]>(section_header, section_data));
		}

		// Now the section objects can be created, since all section intermediates have been loaded.
		// In order to create the section objects, section names are required and they must be loaded from one of the loaded intermediates
		var sections = new List<BinarySection>();

		// Determine the buffer, which contains the section names
		var section_names = section_intermediates[header.SectionNameEntryIndex].Value;

		foreach (var section_intermediate in section_intermediates)
		{
			// Determine the start and the end indices of the section name
			var section_name_start = section_intermediate.Key.Name;
			var section_name_end = section_name_start;
			for (; section_name_end < section_names.Length && section_names[section_name_end] != 0; section_name_end++) { }

			// Load the section name
			var section_name = Encoding.ASCII.GetString(section_names, section_name_start, section_name_end - section_name_start);

			// Determine the section type
			var section_type = section_name switch
			{
				TEXT_SECTION => BinarySectionType.TEXT,
				DATA_SECTION => BinarySectionType.DATA,
				SYMBOL_TABLE_SECTION => BinarySectionType.SYMBOL_TABLE,
				STRING_TABLE_SECTION => BinarySectionType.STRING_TABLE,
				_ => BinarySectionType.NONE
			};

			// Detect relocation table sections
			if (section_name.StartsWith(RELOCATION_TABLE_SECTION_PREFIX)) { section_type = BinarySectionType.RELOCATION_TABLE; }

			var section = new BinarySection(section_name, section_type, section_intermediate.Value);
			section.Flags = GetSharedSectionFlags((ElfSectionFlags)section_intermediate.Key.Flags);
			section.Alignment = (int)section_intermediate.Key.Alignment;
			section.Offset = (int)section_intermediate.Key.Offset;
			section.VirtualSize = section_intermediate.Value.Length;

			sections.Add(section);
		}

		ImportSymbolsAndRelocations(sections, section_intermediates);

		Marshal.FreeHGlobal(bytes);
		return new BinaryObjectFile(name, sections);
	}

	/// <summary>
	/// Load the specified object file and constructs a object structure that represents it
	/// </summary>
	public static BinaryObjectFile Import(string path)
	{
		return Import(path, File.ReadAllBytes(path));
	}
}