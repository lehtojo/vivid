using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System;

public class DynamicLinkingInformation
{
	public BinarySection Section { get; set; }
	public List<ElfSymbolEntry> Entries { get; }
	public List<BinarySymbol> Symbols { get; }
	public List<BinaryRelocation> Relocations { get; }

	public DynamicLinkingInformation(BinarySection section, List<ElfSymbolEntry> entries, List<BinarySymbol> symbols, List<BinaryRelocation> relocations)
	{
		Section = section;
		Entries = entries;
		Symbols = symbols;
		Relocations = relocations;
	}
}

public static class Linker
{
	public const ulong VIRTUAL_ADDRESS_START = 0x400000;
	public const ulong SEGMENT_ALIGNMENT = 0x1000;

	/// <summary>
	/// Goes through all the symbols in the specified object files and makes their hidden symbols unique by adding their object file indices to their names.
	/// This way, if multiple object files have hidden symbols with same names, their names are made unique using the object file indices.
	/// </summary>
	private static void MakeLocalSymbolsUnique(List<BinaryObjectFile> objects)
	{
		foreach (var iterator in objects)
		{
			foreach (var symbol in iterator.Sections.SelectMany(i => i.Symbols.Values))
			{
				if (symbol.Export || symbol.External) continue;
				symbol.Name = iterator.Index.ToString(CultureInfo.InvariantCulture) + '.' + symbol.Name;
			}
		}
	}

	/// <summary>
	/// Returns whether the specified section should be loaded into memory when the application starts
	/// </summary>
	public static bool IsLoadableSection(BinarySection section)
	{
		return section.Type == BinarySectionType.TEXT || section.Type == BinarySectionType.DATA;
	}

	/// <summary>
	/// Resolves all external symbols from the specified binary objects by connecting them to the real symbols.
	/// This function throws an exception if no definition for an external symbol is found.
	/// </summary>
	public static Dictionary<string, BinarySymbol> ResolveSymbols(List<BinaryObjectFile> objects)
	{
		var definitions = new Dictionary<string, BinarySymbol>();

		foreach (var symbol in objects.SelectMany(i => i.Sections).SelectMany(i => i.Symbols.Values))
		{
			if (symbol.External || definitions.TryAdd(symbol.Name, symbol)) continue;
			throw new ApplicationException($"Symbol '{symbol.Name}' is defined at least twice");
		}

		foreach (var relocation in objects.SelectMany(i => i.Sections).SelectMany(i => i.Relocations))
		{
			var symbol = relocation.Symbol;

			// If the relocation is not external, the symbol is already resolved
			if (!symbol.External) continue;

			// Try to find the actual symbol
			if (!definitions.TryGetValue(symbol.Name, out var definition)) throw new ApplicationException($"Symbol '{symbol.Name}' is not defined");

			relocation.Symbol = definition;
		}

		return definitions;
	}

	/// <summary>
	/// Combines the loadable sections of the specified object files
	/// </summary>
	public static List<BinarySection> CreateLoadableSections(List<BinarySection> fragments)
	{
		// Merge all sections that have the same type
		var result = new List<BinarySection>();

		// Group all fragments based on their section names
		var section_fragments = fragments.GroupBy(i => i.Name).ToList();
		var allocated_fragments = section_fragments.Count(i => i.First().Type == BinarySectionType.NONE || i.First().Flags.HasFlag(BinarySectionFlag.ALLOCATE));

		for (var i = 0; i < section_fragments.Count; i++)
		{
			var section = section_fragments[i];

			var flags = section.First().Flags;
			var type = section.First().Type;
			var alignment = section.First().Alignment;
			var name = section.Key;
			var load_size = section.Sum(i => i.Data.Length); // Compute how many bytes all the fragments inside the current section take
			var virtual_size = load_size;

			// Expand the section, if it is not the last allocated section, so that the next allocated section is aligned
			if (i + 1 < allocated_fragments) { virtual_size = (load_size / (int)SEGMENT_ALIGNMENT + 1) * (int)SEGMENT_ALIGNMENT; }

			result.Add(new BinarySection(name, flags, type, alignment, Array.Empty<byte>(), virtual_size, load_size));
		}

		return result;
	}

	/// <summary>
	/// Creates the program headers, meaning the specified section will get their own virtual addreses and be loaded into memory when the created executable is loaded
	/// </summary>
	public static int CreateProgramHeaders(List<BinarySection> sections, List<BinarySection> fragments, List<ElfProgramHeader> headers, ulong virtual_address)
	{
		var header = new ElfProgramHeader();
		header.Type = ElfSegmentType.LOADABLE;
		header.Flags = ElfSegmentFlag.READ;
		header.Offset = 0;
		header.VirtualAddress = virtual_address;
		header.PhysicalAddress = virtual_address;
		header.SegmentFileSize = SEGMENT_ALIGNMENT;
		header.SegmentMemorySize = SEGMENT_ALIGNMENT;
		header.Alignment = SEGMENT_ALIGNMENT;

		headers.Add(header);

		var file_position = (int)SEGMENT_ALIGNMENT;
		virtual_address += SEGMENT_ALIGNMENT;

		foreach (var section in sections)
		{
			if (section.Name.Length != 0 && !section.Flags.HasFlag(BinarySectionFlag.ALLOCATE))
			{
				// Restore the current virtual address after aligning the fragments
				var previous_virtual_address = virtual_address;

				// All non-allocated sections start from virtual address 0
				virtual_address = 0;

				section.Offset = file_position;
				section.VirtualAddress = (int)virtual_address;

				// Align all the section fragments
				foreach (var fragment in fragments.FindAll(i => i.Name == section.Name))
				{
					fragment.Offset = file_position;
					fragment.VirtualAddress = (int)virtual_address;

					file_position += fragment.Data.Length;
					virtual_address += (uint)fragment.Data.Length;
				}

				// Determine the file position and restore the virtual address
				file_position = section.Offset + section.VirtualSize;
				virtual_address = previous_virtual_address;
				continue;
			}

			// Determine the section flags
			var flags = section.Name switch
			{
				ElfFormat.DATA_SECTION => ElfSegmentFlag.WRITE | ElfSegmentFlag.READ,
				ElfFormat.TEXT_SECTION => ElfSegmentFlag.EXECUTE | ElfSegmentFlag.READ,
				ElfFormat.DYNAMIC_SECTION => ElfSegmentFlag.WRITE | ElfSegmentFlag.READ,
				_ => ElfSegmentFlag.READ
			};

			header = new ElfProgramHeader();
			header.Type = ElfSegmentType.LOADABLE;
			header.Flags = flags;
			header.Offset = (uint)file_position;
			header.VirtualAddress = virtual_address;
			header.PhysicalAddress = virtual_address;
			header.SegmentFileSize = (uint)section.VirtualSize;
			header.SegmentMemorySize = (uint)section.VirtualSize;
			header.Alignment = SEGMENT_ALIGNMENT;
			headers.Add(header);

			// Dynamic sections also need a duplicate section, which is marked as dynamic...
			if (section.Name == ElfFormat.DYNAMIC_SECTION)
			{
				// Add the dynamic section header
				header = new ElfProgramHeader();
				header.Type = ElfSegmentType.DYNAMIC;
				header.Flags = flags;
				header.Offset = (uint)file_position;
				header.VirtualAddress = virtual_address;
				header.PhysicalAddress = virtual_address;
				header.SegmentFileSize = (uint)section.VirtualSize;
				header.SegmentMemorySize = (uint)section.VirtualSize;
				header.Alignment = 8;

				headers.Add(header);
			}

			section.Offset = file_position;
			section.VirtualAddress = (int)virtual_address;

			// Align all the section fragments
			foreach (var fragment in fragments.FindAll(i => i.Name == section.Name))
			{
				fragment.Offset = file_position;
				fragment.VirtualAddress = (int)virtual_address;
				
				file_position += fragment.Data.Length;
				virtual_address += (uint)fragment.Data.Length;
			}

			file_position = section.Offset + section.VirtualSize;
			virtual_address = (ulong)(section.VirtualAddress + section.VirtualSize);
		}

		return file_position;
	}

	/// <summary>
	/// Computes relocations inside the specified object files using section virtual addresses
	/// </summary>
	public static void ComputeRelocations(List<BinaryObjectFile> objects, List<BinaryRelocation> additional_relocations)
	{
		foreach (var relocation in objects.SelectMany(i => i.Sections).SelectMany(i => i.Relocations).Concat(additional_relocations))
		{
			var symbol = relocation.Symbol;
			var symbol_section = symbol.Section; // ?? throw new ApplicationException("Missing symbol definition section");
			var relocation_section = relocation.Section; // ?? throw new ApplicationException("Missing relocation section");

			if (symbol_section == null || relocation_section == null) continue;

			if (relocation.Type == BinaryRelocationType.PROGRAM_COUNTER_RELATIVE)
			{
				var from = relocation_section.VirtualAddress + relocation.Offset;
				var to = symbol_section.VirtualAddress + symbol.Offset;
				InstructionEncoder.WriteInt32(relocation_section.Data, relocation.Offset, to - from + relocation.Addend);
			}
			else if (relocation.Type == BinaryRelocationType.ABSOLUTE64)
			{
				InstructionEncoder.WriteInt64(relocation_section.Data, relocation.Offset, symbol_section.VirtualAddress + symbol.Offset);
			}
			else if (relocation.Type == BinaryRelocationType.ABSOLUTE32)
			{
				InstructionEncoder.WriteInt32(relocation_section.Data, relocation.Offset, symbol_section.VirtualAddress + symbol.Offset);
			}
			else if (relocation.Type == BinaryRelocationType.FILE_OFFSET_64)
			{
				InstructionEncoder.WriteInt64(relocation_section.Data, relocation.Offset, symbol_section.Offset + symbol.Offset);
			}
			else
			{
				throw new ApplicationException("Unsupported relocation type");
			}
		}
	}

	/// <summary>
	/// Creates all the required dynamic sections needed in a shared library. This includes the dynamic section, the dynamic symbol table, the dynamic string table.
	/// The dynamic symbol table created by this function will only exported symbols.
	/// </summary>
	private static DynamicLinkingInformation CreateDynamicSections(List<BinarySection> sections, Dictionary<string, BinarySymbol> symbols)
	{
		// Build the dynamic section data
		var dynamic_section_data = new byte[ElfDynamicEntry.Size * 6];
		ElfFormat.Write(dynamic_section_data, ElfDynamicEntry.Size * 0, new ElfDynamicEntry(ElfDynamicSectionTag.HashTable, 0));   // File offset of the hash table   (filled in later using a relocation)
		ElfFormat.Write(dynamic_section_data, ElfDynamicEntry.Size * 1, new ElfDynamicEntry(ElfDynamicSectionTag.StringTable, 0)); // File offset of the string table (filled in later using a relocation)
		ElfFormat.Write(dynamic_section_data, ElfDynamicEntry.Size * 2, new ElfDynamicEntry(ElfDynamicSectionTag.SymbolTable, 0)); // File offset of the symbol table (filled in later using a relocation)
		ElfFormat.Write(dynamic_section_data, ElfDynamicEntry.Size * 3, new ElfDynamicEntry(ElfDynamicSectionTag.StringTableSize, 1));
		ElfFormat.Write(dynamic_section_data, ElfDynamicEntry.Size * 4, new ElfDynamicEntry(ElfDynamicSectionTag.SymbolEntrySize, ElfSymbolEntry.Size));

		// Dynamic section:
		var dynamic_section = new BinarySection(ElfFormat.DYNAMIC_SECTION, BinarySectionType.DYNAMIC, dynamic_section_data);
		dynamic_section.Alignment = 8;
		dynamic_section.Flags = BinarySectionFlag.WRITE | BinarySectionFlag.ALLOCATE;

		// Create a symbol, which represents the start of the dynamic section
		var dynamic_section_start = new BinarySymbol(ElfFormat.DYNAMIC_SECTION_START, 0, false);
		dynamic_section_start.Export = true;
		dynamic_section_start.Section = dynamic_section;
		dynamic_section.Symbols.Add(dynamic_section_start.Name, dynamic_section_start);
		symbols.Add(dynamic_section_start.Name, dynamic_section_start);

		// Symbol name table:
		var exported_symbol_name_table = new ElfStringTable();
		var exported_symbol_entries = new List<ElfSymbolEntry>();
		var exported_symbols = symbols.Values.Where(i => i.Export).ToList();
		exported_symbols.Insert(0, new BinarySymbol(string.Empty, 0, false));

		// Create symbol entries for each exported symbol without correct section indices, since they will need to be filled in later
		foreach (var symbol in exported_symbols)
		{
			var symbol_entry = new ElfSymbolEntry();
			symbol_entry.Name = (uint)exported_symbol_name_table.Add(symbol.Name);
			symbol_entry.SetInfo(symbol.External || symbol.Export ? ElfSymbolBinding.GLOBAL : ElfSymbolBinding.LOCAL, 0);

			symbol.Index = (uint)exported_symbol_entries.Count;
			exported_symbol_entries.Add(symbol_entry);
		}

		// Dynamic symbol table:
		var dynamic_symbol_table = new BinarySection(ElfFormat.DYNAMIC_SYMBOL_TABLE_SECTION, BinarySectionType.SYMBOL_TABLE, new byte[ElfSymbolEntry.Size * exported_symbol_entries.Count]);
		dynamic_symbol_table.Alignment = 8;
		dynamic_symbol_table.Flags = BinarySectionFlag.ALLOCATE;

		// Create a symbol, which represents the start of the dynamic symbol table
		// This symbol is used to fill the file offset of the dynamic symbol table in the dynamic section
		var dynamic_symbol_table_start = new BinarySymbol(dynamic_symbol_table.Name, 0, false);
		dynamic_symbol_table_start.Section = dynamic_symbol_table;
		dynamic_symbol_table.Symbols.Add(dynamic_symbol_table.Name, dynamic_symbol_table_start);

		// Dynamic string table:
		var dynamic_string_table = new BinarySection(ElfFormat.DYNAMIC_STRING_TABLE_SECTION, BinarySectionType.STRING_TABLE, exported_symbol_name_table.Export());
		dynamic_string_table.Flags = BinarySectionFlag.ALLOCATE;

		// Create a symbol, which represents the start of the dynamic string table
		// This symbol is used to fill the file offset of the dynamic string table in the dynamic section
		var dynamic_string_table_start = new BinarySymbol(dynamic_string_table.Name, 0, false);
		dynamic_string_table_start.Section = dynamic_string_table;
		dynamic_string_table.Symbols.Add(dynamic_string_table_start.Name, dynamic_string_table_start);

		// Hash section:
		// This section can be used to check efficiently whether a specific symbol exists in the dynamic symbol table
		var hash_section = ElfFormat.CreateHashSection(exported_symbols);
		hash_section.Alignment = 8;
		hash_section.Flags = BinarySectionFlag.ALLOCATE;

		var hash_section_start = new BinarySymbol(hash_section.Name, 0, false);
		hash_section_start.Section = hash_section;
		hash_section.Symbols.Add(hash_section_start.Name, hash_section_start);

		// Add relocations for hash, symbol and string tables in the dynamic section
		var relocations = new List<BinaryRelocation>();
		relocations.Add(new BinaryRelocation(hash_section_start, ElfDynamicEntry.Size * 0 + ElfDynamicEntry.PointerOffset, 0, BinaryRelocationType.FILE_OFFSET_64));
		relocations.Add(new BinaryRelocation(dynamic_string_table_start, ElfDynamicEntry.Size * 1 + ElfDynamicEntry.PointerOffset, 0, BinaryRelocationType.FILE_OFFSET_64));
		relocations.Add(new BinaryRelocation(dynamic_symbol_table_start, ElfDynamicEntry.Size * 2 + ElfDynamicEntry.PointerOffset, 0, BinaryRelocationType.FILE_OFFSET_64));

		// Connect the relocations to the dynamic section
		foreach (var relocation in relocations) { relocation.Section = dynamic_section; }

		// Add the created sections
		sections.Add(hash_section);
		sections.Add(dynamic_section);
		sections.Add(dynamic_symbol_table);
		sections.Add(dynamic_string_table);

		return new DynamicLinkingInformation(dynamic_symbol_table, exported_symbol_entries, exported_symbols, relocations);
	}

	/// <summary>
	/// Finish the specified dynamic linking information by filling symbol section indices into the symbol entires and writing them to the dynamic symbol table.
	/// </summary>
	private static void FinishDynamicLinkingInformation(DynamicLinkingInformation information, List<BinarySection> sections)
	{
		// Fill in the symbol section indices
		for (var i = 0; i < information.Symbols.Count; i++)
		{
			var symbol = information.Symbols[i];
			var symbol_entry = information.Entries[i];

			// Fill in the virtual address of the symbol
			var virtual_address = symbol.Section == null ? 0 : symbol.Section.VirtualAddress;
			symbol_entry.Value = (ulong)(virtual_address + symbol.Offset);

			if (symbol.Section == null) continue;
			symbol_entry.SectionIndex = (ushort)symbol.Section!.Index;
		}

		// Write the symbol entries into the dynamic symbol table
		ElfFormat.Write(information.Section.Data, 0, information.Entries);
	}

	public static byte[] Link(List<BinaryObjectFile> objects, string entry, bool executable)
	{
		// Index all the specified object files
		for (var i = 0; i < objects.Count; i++) { objects[i].Index = i; }

		// Make all hidden symbols unique by using their object file indices
		MakeLocalSymbolsUnique(objects);

		var header = new ElfFileHeader();
		header.Type = executable ? ElfObjectFileType.EXECUTABLE : ElfObjectFileType.DYNAMIC;
		header.Machine = ElfMachineType.X64;
		header.FileHeaderSize = ElfFileHeader.Size;
		header.SectionHeaderSize = ElfSectionHeader.Size;

		// Resolves are unresolved symbols and returns all symbols as a list
		var symbols = ResolveSymbols(objects);

		// Create the program headers
		var program_headers = new List<ElfProgramHeader>();

		// Ensure sections are ordered so that sections of same type are next to each other
		var fragments = objects.SelectMany(i => i.Sections).Where(IsLoadableSection).ToList();

		// Add dynamic sections if needed
		var dynamic_linking_information = executable ? null : CreateDynamicSections(fragments, symbols);

		// Order the fragments so that allocated fragments come first
		var allocated_fragments = fragments.Where(i => i.Type == BinarySectionType.NONE || i.Flags.HasFlag(BinarySectionFlag.ALLOCATE)).ToList();
		var data_fragments = fragments.Where(i => i.Type != BinarySectionType.NONE && !i.Flags.HasFlag(BinarySectionFlag.ALLOCATE)).ToList();

		fragments = allocated_fragments.Concat(data_fragments).ToList();

		// Create sections, which cover the fragmented sections
		var sections = CreateLoadableSections(fragments);

		CreateProgramHeaders(sections, fragments, program_headers, executable ? VIRTUAL_ADDRESS_START : 0UL);

		// Now that sections have their virtual addresses relocations can be computed
		ComputeRelocations(objects, dynamic_linking_information != null ? dynamic_linking_information.Relocations : new List<BinaryRelocation>());

		// Create an empty section, so that it is possible to leave section index unspecified in symbols for example.
		// This section is used to align the first loadable section
		var none_section = new BinarySection(string.Empty, BinarySectionType.NONE, Array.Empty<byte>());
		sections.Insert(0, none_section);

		// Group the symbols by their section types
		foreach (var section_symbols in symbols.Values.GroupBy(i => i.Section!.Type))
		{
			var section = sections.Find(i => i.Type == section_symbols.Key);
			if (section == null) throw new ApplicationException("Symbol did not have a corresponding linker export section");

			foreach (var symbol in section_symbols) { section.Symbols.Add(symbol.Name, symbol); }
		}

		// Form the symbol table
		ElfFormat.CreateSymbolRelatedSections(sections, fragments, symbols);

		// Finish the specified dynamic linking information by filling symbol section indices into the symbol entires and writing them to the dynamic symbol table
		if (dynamic_linking_information != null) FinishDynamicLinkingInformation(dynamic_linking_information, sections);

		var section_headers = ElfFormat.CreateSectionHeaders(sections, symbols, (int)SEGMENT_ALIGNMENT);
		var section_bytes = sections.Sum(i => i.VirtualSize);

		var bytes = (int)SEGMENT_ALIGNMENT + section_bytes + section_headers.Count * ElfSectionHeader.Size;

		// Save the location of the program header table
		header.ProgramHeaderOffset = ElfFileHeader.Size;
		header.ProgramHeaderEntryCount = (short)program_headers.Count;
		header.ProgramHeaderSize = ElfProgramHeader.Size;

		// Save the location of the section header table
		header.SectionHeaderOffset = SEGMENT_ALIGNMENT + (ulong)section_bytes;
		header.SectionHeaderTableEntryCount = (short)section_headers.Count;
		header.SectionHeaderSize = ElfSectionHeader.Size;
		header.SectionNameEntryIndex = (short)(section_headers.Count - 1);

		// Compute the entry point location
		var entry_point_symbol = symbols[entry];
		header.Entry = (ulong)(entry_point_symbol.Section!.VirtualAddress + entry_point_symbol.Offset);

		var result = new byte[bytes];

		// Write the file header
		ElfFormat.Write(result, 0, header);

		// Write the program header table now
		var position = (int)header.ProgramHeaderOffset;

		foreach (var program_header in program_headers)
		{
			ElfFormat.Write(result, position, program_header);
			position += ElfProgramHeader.Size;
		}

		foreach (var section in sections)
		{
			// Loadable sections are handled with the fragments
			if (IsLoadableSection(section)) continue;

			Array.Copy(section.Data, 0, result, section.Offset, section.Data.Length);
		}

		// Write the loadable sections
		foreach (var fragment in fragments)
		{
			Array.Copy(fragment.Data, 0, result, fragment.Offset, fragment.Data.Length);
		}

		// Write the section header table now
		position = (int)header.SectionHeaderOffset;

		foreach (var section_header in section_headers)
		{
			ElfFormat.Write(result, position, section_header);
			position += ElfSectionHeader.Size;
		}

		return result;
	}
}