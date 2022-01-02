using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System;

public class DynamicLinkingInformation
{
	public BinarySection DynamicSection { get; set; }
	public BinarySection? RelocationSection { get; set; } = null;
	public List<ElfSymbolEntry> Entries { get; }
	public List<BinarySymbol> Symbols { get; }
	public List<BinaryRelocation> Relocations { get; set; } = new List<BinaryRelocation>();

	public DynamicLinkingInformation(BinarySection section, List<ElfSymbolEntry> entries, List<BinarySymbol> symbols)
	{
		DynamicSection = section;
		Entries = entries;
		Symbols = symbols;
	}
}

public static class Linker
{
	public const ulong DefaultBaseAddress = 0x400000;
	public const ulong SegmentAlignment = 0x1000;

	/// <summary>
	/// Goes through all the symbols in the specified object files and makes their hidden symbols unique by adding their object file indices to their names.
	/// This way, if multiple object files have hidden symbols with same names, their names are made unique using the object file indices.
	/// </summary>
	public static void MakeLocalSymbolsUnique(List<BinaryObjectFile> objects)
	{
		foreach (var iterator in objects)
		{
			foreach (var symbol in iterator.Sections.SelectMany(i => i.Symbols.Values))
			{
				if (symbol.Export || symbol.External) continue;
				symbol.Name = iterator.Index.ToString() + '.' + symbol.Name;
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

	private static void PrintSymbolConflictsAndCrash(List<BinaryObjectFile> objects, BinarySymbol symbol)
	{
		// Find the objects that have the same symbol
		var conflicting_objects = objects.Where(i => i.Exports.Contains(symbol.Name)).ToList();
		throw new ApplicationException($"Symbol '{symbol.Name}' is defined at least twice. Conflicting objects: {string.Join(", ", conflicting_objects.Select(i => i.Name))}");
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
			PrintSymbolConflictsAndCrash(objects, symbol);
		}

		foreach (var relocation in objects.SelectMany(i => i.Sections).SelectMany(i => i.Relocations))
		{
			var symbol = relocation.Symbol;

			// If the relocation is not external, the symbol is already resolved
			if (!symbol.External) continue;

			// Try to find the actual symbol
			#warning Add dynamic symbols for Linux
			if (!definitions.TryGetValue(symbol.Name, out var definition)) continue;

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
		var allocated_fragments = section_fragments.Count(i => i.First().Type == BinarySectionType.NONE || i.First().Flags.HasFlag(BinarySectionFlags.ALLOCATE));

		var file_position = SegmentAlignment;

		for (var i = 0; i < section_fragments.Count; i++)
		{
			var inner_fragments = section_fragments[i].ToList();
			var is_allocated_section = i + 1 <= allocated_fragments;

			var flags = inner_fragments.First().Flags;
			var type = inner_fragments.First().Type;
			var name = inner_fragments.First().Name;

			// Compute the margin needed to align the overlay section
			var alignment = is_allocated_section ? (int)SegmentAlignment : inner_fragments.First().Alignment;
			var overlay_margin = alignment - (int)file_position % alignment;

			// Apply the margin if it is needed for alignment
			if (overlay_margin != alignment) { file_position += (ulong)overlay_margin; }
			else { overlay_margin = 0; }

			// Save the current file position so that the size of the overlay section can be computed below
			var start_file_position = file_position;

			file_position += (ulong)inner_fragments.First().Data.Length;

			// Skip the first fragment, since it is already part of the section
			for (var j = 1; j < inner_fragments.Count; j++)
			{
				var fragment = inner_fragments[j];

				// Compute the margin needed to align the fragment
				alignment = fragment.Alignment;

				var margin = alignment - (int)file_position % alignment;

				// Apply the margin if it is needed for alignment
				if (margin != alignment)
				{
					fragment.Margin = margin;
					file_position += (ulong)margin;
				}

				file_position += (ulong)fragment.Data.Length;
			}

			var overlay_size = (int)(file_position - start_file_position);
			var overlay_section = new BinarySection(name, flags, type, alignment, Array.Empty<byte>(), overlay_margin, overlay_size);

			result.Add(overlay_section);
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
		header.SegmentFileSize = SegmentAlignment;
		header.SegmentMemorySize = SegmentAlignment;
		header.Alignment = SegmentAlignment;

		headers.Add(header);

		var file_position = (int)SegmentAlignment;
		virtual_address += SegmentAlignment;

		foreach (var section in sections)
		{
			// Apply the section margin before doing anything
			file_position += section.Margin;
			virtual_address += (ulong)section.Margin;

			if (section.Name.Length != 0 && !section.Flags.HasFlag(BinarySectionFlags.ALLOCATE))
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

					file_position += fragment.Margin + fragment.Data.Length;
					virtual_address += (uint)(fragment.Margin + fragment.Data.Length);
				}

				// Restore the virtual address
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
			header.Alignment = SegmentAlignment;
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
				// Apply the fragment margin before doing anything, so that the fragment is aligned
				file_position += fragment.Margin;
				virtual_address += (uint)fragment.Margin;

				fragment.Offset = file_position;
				fragment.VirtualAddress = (int)virtual_address;
				
				file_position += fragment.Data.Length;
				virtual_address += (uint)fragment.Data.Length;
			}
		}

		return file_position;
	}

	/// <summary>
	/// Computes relocations inside the specified object files using section virtual addresses
	/// </summary>
	public static void ComputeRelocations(List<BinaryRelocation> relocations, int base_address = 0)
	{
		foreach (var relocation in relocations)
		{
			var symbol = relocation.Symbol;
			var symbol_section = symbol.Section; // ?? throw new ApplicationException("Missing symbol definition section");
			var relocation_section = relocation.Section; // ?? throw new ApplicationException("Missing relocation section");

			if (symbol_section == null || relocation_section == null) continue;

			if (relocation.Type == BinaryRelocationType.PROGRAM_COUNTER_RELATIVE)
			{
				var from = relocation_section.VirtualAddress + relocation.Offset;
				var to = symbol_section.VirtualAddress + symbol.Offset;
				BinaryUtility.WriteInt32(relocation_section.Data, relocation.Offset, to - from + relocation.Addend);
			}
			else if (relocation.Type == BinaryRelocationType.ABSOLUTE64)
			{
				BinaryUtility.WriteInt64(relocation_section.Data, relocation.Offset, (symbol_section.VirtualAddress + symbol.Offset) + base_address);
			}
			else if (relocation.Type == BinaryRelocationType.ABSOLUTE32)
			{
				BinaryUtility.WriteInt32(relocation_section.Data, relocation.Offset, (symbol_section.VirtualAddress + symbol.Offset) + base_address);
			}
			else if (relocation.Type == BinaryRelocationType.SECTION_RELATIVE_64)
			{
				BinaryUtility.WriteInt64(relocation_section.Data, relocation.Offset, (symbol_section.VirtualAddress + symbol.Offset) - symbol_section.BaseVirtualAddress);
			}
			else if (relocation.Type == BinaryRelocationType.SECTION_RELATIVE_32)
			{
				BinaryUtility.WriteInt32(relocation_section.Data, relocation.Offset, (symbol_section.VirtualAddress + symbol.Offset) - symbol_section.BaseVirtualAddress);
			}
			else if (relocation.Type == BinaryRelocationType.FILE_OFFSET_64)
			{
				BinaryUtility.WriteInt64(relocation_section.Data, relocation.Offset, symbol_section.Offset + symbol.Offset);
			}
			else if (relocation.Type == BinaryRelocationType.BASE_RELATIVE_64)
			{
				BinaryUtility.WriteInt64(relocation_section.Data, relocation.Offset, symbol_section.VirtualAddress + symbol.Offset);
			}
			else if (relocation.Type == BinaryRelocationType.BASE_RELATIVE_32)
			{
				BinaryUtility.WriteInt32(relocation_section.Data, relocation.Offset, symbol_section.VirtualAddress + symbol.Offset);
			}
			else
			{
				throw new ApplicationException("Unsupported relocation type");
			}
		}
	}

	/// <summary>
	/// Searches for relocations that must be solved by the dynamic linker and removes them from the specified relocations.
	/// This function creates a dynamic relocation section if required.
	/// </summary>
	private static void CreateDynamicRelocations(List<BinarySection> sections, List<BinaryRelocation> relocations, DynamicLinkingInformation dynamic_linking_information)
	{
		// Find all relocations that are absolute
		var absolute_relocations = relocations.Where(i => i.Type == BinaryRelocationType.ABSOLUTE64).ToList();

		for (var i = relocations.Count - 1; i >= 0; i--)
		{
			var relocation = relocations[i];

			if (relocation.Type == BinaryRelocationType.ABSOLUTE32)
			{
				throw new ApplicationException("32-bit absolute relocations are not supported when building a shared library on 64-bit mode");
			}

			// Take only the 64-bit absolute relocations
			if (relocation.Type == BinaryRelocationType.ABSOLUTE64)
			{
				absolute_relocations.Add(relocation);
				relocations.RemoveAt(i); // Remove the relocation from the list, since now the dynamic linker is responsible for it
			}
		}

		if (!absolute_relocations.Any()) return;

		// Create a new section for the dynamic relocations
		var dynamic_relocations_data = new byte[absolute_relocations.Count * ElfRelocationEntry.Size];
		var dynamic_relocations_section = new BinarySection(ElfFormat.DYNAMIC_RELOCATIONS_SECTION, BinarySectionType.RELOCATION_TABLE, dynamic_relocations_data);
		dynamic_relocations_section.Alignment = 8;
		dynamic_relocations_section.Flags = BinarySectionFlags.ALLOCATE;

		// Finish the absolute relocations later, since they require virtual addresses for sections
		dynamic_linking_information.RelocationSection = dynamic_relocations_section;
		dynamic_linking_information.Relocations = absolute_relocations;

		// Add the dynamic relocations section to the list of sections
		sections.Add(dynamic_relocations_section);
	}

	/// <summary>
	/// Creates all the required dynamic sections needed in a shared library. This includes the dynamic section, the dynamic symbol table, the dynamic string table.
	/// The dynamic symbol table created by this function will only exported symbols.
	/// </summary>
	private static DynamicLinkingInformation CreateDynamicSections(List<BinarySection> sections, Dictionary<string, BinarySymbol> symbols, List<BinaryRelocation> relocations)
	{
		// Build the dynamic section data
		var dynamic_section_entries = new List<ElfDynamicEntry>();
		dynamic_section_entries.Add(new ElfDynamicEntry(ElfDynamicSectionTag.HashTable, 0)); // The address is filled in later using a relocation
		dynamic_section_entries.Add(new ElfDynamicEntry(ElfDynamicSectionTag.StringTable, 0));  // The address is filled in later using a relocation
		dynamic_section_entries.Add(new ElfDynamicEntry(ElfDynamicSectionTag.SymbolTable, 0));  // The address is filled in later using a relocation
		dynamic_section_entries.Add(new ElfDynamicEntry(ElfDynamicSectionTag.SymbolEntrySize, ElfSymbolEntry.Size));
		dynamic_section_entries.Add(new ElfDynamicEntry(ElfDynamicSectionTag.StringTableSize, 1));

		// Dynamic section:
		var dynamic_section = new BinarySection(ElfFormat.DYNAMIC_SECTION, BinarySectionType.DYNAMIC, Array.Empty<byte>());
		dynamic_section.Alignment = 8;
		dynamic_section.Flags = BinarySectionFlags.WRITE | BinarySectionFlags.ALLOCATE;

		// Create a symbol, which represents the start of the dynamic section
		var dynamic_section_start = new BinarySymbol(ElfFormat.DYNAMIC_SECTION_START, 0, false);
		dynamic_section_start.Export = true;
		dynamic_section_start.Section = dynamic_section;
		dynamic_section.Symbols.Add(dynamic_section_start.Name, dynamic_section_start);
		symbols.Add(dynamic_section_start.Name, dynamic_section_start);

		// Symbol name table:
		var exported_symbol_name_table = new BinaryStringTable();
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
		dynamic_symbol_table.Flags = BinarySectionFlags.ALLOCATE;

		// Create a symbol, which represents the start of the dynamic symbol table
		// This symbol is used to fill the file offset of the dynamic symbol table in the dynamic section
		var dynamic_symbol_table_start = new BinarySymbol(dynamic_symbol_table.Name, 0, false);
		dynamic_symbol_table_start.Section = dynamic_symbol_table;
		dynamic_symbol_table.Symbols.Add(dynamic_symbol_table.Name, dynamic_symbol_table_start);

		// Dynamic string table:
		var dynamic_string_table = new BinarySection(ElfFormat.DYNAMIC_STRING_TABLE_SECTION, BinarySectionType.STRING_TABLE, exported_symbol_name_table.Export());
		dynamic_string_table.Flags = BinarySectionFlags.ALLOCATE;

		// Create a symbol, which represents the start of the dynamic string table
		// This symbol is used to fill the file offset of the dynamic string table in the dynamic section
		var dynamic_string_table_start = new BinarySymbol(dynamic_string_table.Name, 0, false);
		dynamic_string_table_start.Section = dynamic_string_table;
		dynamic_string_table.Symbols.Add(dynamic_string_table_start.Name, dynamic_string_table_start);

		// Hash section:
		// This section can be used to check efficiently whether a specific symbol exists in the dynamic symbol table
		var hash_section = ElfFormat.CreateHashSection(exported_symbols);
		hash_section.Alignment = 8;
		hash_section.Flags = BinarySectionFlags.ALLOCATE;

		var hash_section_start = new BinarySymbol(hash_section.Name, 0, false);
		hash_section_start.Section = hash_section;
		hash_section.Symbols.Add(hash_section_start.Name, hash_section_start);

		var dynamic_linking_information = new DynamicLinkingInformation(dynamic_symbol_table, exported_symbol_entries, exported_symbols);
		CreateDynamicRelocations(sections, relocations, dynamic_linking_information);

		// Add relocations for hash, symbol and string tables in the dynamic section
		var additional_relocations = new List<BinaryRelocation>();
		additional_relocations.Add(new BinaryRelocation(hash_section_start, ElfDynamicEntry.Size * 0 + ElfDynamicEntry.PointerOffset, 0, BinaryRelocationType.FILE_OFFSET_64));
		additional_relocations.Add(new BinaryRelocation(dynamic_string_table_start, ElfDynamicEntry.Size * 1 + ElfDynamicEntry.PointerOffset, 0, BinaryRelocationType.FILE_OFFSET_64));
		additional_relocations.Add(new BinaryRelocation(dynamic_symbol_table_start, ElfDynamicEntry.Size * 2 + ElfDynamicEntry.PointerOffset, 0, BinaryRelocationType.FILE_OFFSET_64));

		if (dynamic_linking_information.RelocationSection != null)
		{
			// Create a symbol, which represents the start of the dynamic relocation table
			var dynamic_relocations_section_start = new BinarySymbol(dynamic_linking_information.RelocationSection!.Name, 0, false);
			dynamic_relocations_section_start.Section = dynamic_linking_information.RelocationSection;
			dynamic_linking_information.RelocationSection.Symbols.Add(dynamic_relocations_section_start.Name, dynamic_relocations_section_start);

			// Save the index where the relocation table entry will be placed
			var relocation_table_entry_index = dynamic_section_entries.Count;

			// Add a relocation table entry to the dynamic section entries so that the dynamic linker knows where to find the relocation table
			dynamic_section_entries.Add(new ElfDynamicEntry(ElfDynamicSectionTag.RelocationTable, 0));  // The address is filled in later using a relocation
			dynamic_section_entries.Add(new ElfDynamicEntry(ElfDynamicSectionTag.RelocationTableSize, (ulong)dynamic_linking_information.RelocationSection.Data.Length));
			dynamic_section_entries.Add(new ElfDynamicEntry(ElfDynamicSectionTag.RelocationEntrySize, ElfRelocationEntry.Size));
			dynamic_section_entries.Add(new ElfDynamicEntry(ElfDynamicSectionTag.RelocationCount, (ulong)dynamic_linking_information.Relocations.Count));

			additional_relocations.Add(new BinaryRelocation(dynamic_relocations_section_start, ElfDynamicEntry.Size * relocation_table_entry_index + ElfDynamicEntry.PointerOffset, 0, BinaryRelocationType.FILE_OFFSET_64));
		}

		// Connect the relocations to the dynamic section
		foreach (var relocation in additional_relocations) { relocation.Section = dynamic_section; }

		relocations.AddRange(additional_relocations);

		// Add the created sections
		sections.Add(hash_section);
		sections.Add(dynamic_section);
		sections.Add(dynamic_symbol_table);
		sections.Add(dynamic_string_table);

		// Output the dynamic section entries into the dynamic section
		dynamic_section.Data = new byte[ElfDynamicEntry.Size * (dynamic_section_entries.Count + 1)]; // Allocate one more entry so that the last entry is a none-entry
		BinaryUtility.Write(dynamic_section.Data, 0, dynamic_section_entries);

		return dynamic_linking_information;
	}

	/// <summary>
	/// Finish the specified dynamic linking information by filling symbol section indices into the symbol entires and writing them to the dynamic symbol table.
	/// </summary>
	private static void FinishDynamicLinkingInformation(DynamicLinkingInformation information)
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
		BinaryUtility.Write(information.DynamicSection.Data, 0, information.Entries);

		// Finish dynamic relocations if there are any
		if (information.RelocationSection == null) return;

		var relocations_entries = new List<ElfRelocationEntry>();

		// Generate relocations for all the collected absolute relocations
		// Absolute relocations in a shared library can be expressed as follows:
		// <Base address of the shared library> + <offset of the symbol in the shared library>
		// ELF-standard has a special relocation type for this, which is R_X86_64_RELATIVE.
		foreach (var relocation in information.Relocations)
		{
			var symbol = relocation.Symbol;

			// Determine the offset of the symbol in the shared library
			var relocation_offset = relocation.Section!.VirtualAddress + relocation.Offset;

			// Now we need to compute the offset of the symbol in the shared library
			var symbol_offset = symbol.Section!.VirtualAddress + symbol.Offset;

			// Create a ELF relocation entry for the relocation
			var relocation_entry = new ElfRelocationEntry((ulong)relocation_offset, symbol_offset);
			relocation_entry.SetInfo(0, (uint)ElfSymbolType.BASE_RELATIVE_64);

			relocations_entries.Add(relocation_entry);
		}

		// Write the modified absolute relocations into the dynamic relocation section
		BinaryUtility.Write(information.RelocationSection.Data, 0, relocations_entries);
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

		// Load all the relocations from all the sections
		var relocations = objects.SelectMany(i => i.Sections).SelectMany(i => i.Relocations).ToList();

		// Add dynamic sections if needed
		var dynamic_linking_information = executable ? null : CreateDynamicSections(fragments, symbols, relocations);

		// Order the fragments so that allocated fragments come first
		var allocated_fragments = fragments.Where(i => i.Type == BinarySectionType.NONE || i.Flags.HasFlag(BinarySectionFlags.ALLOCATE)).ToList();
		var data_fragments = fragments.Where(i => i.Type != BinarySectionType.NONE && !i.Flags.HasFlag(BinarySectionFlags.ALLOCATE)).ToList();

		fragments = allocated_fragments.Concat(data_fragments).ToList();

		// Create sections, which cover the fragmented sections
		var overlays = CreateLoadableSections(fragments);

		CreateProgramHeaders(overlays, fragments, program_headers, executable ? DefaultBaseAddress : 0UL);

		// Now that sections have their virtual addresses relocations can be computed
		ComputeRelocations(relocations);

		// Create an empty section, so that it is possible to leave section index unspecified in symbols for example.
		// This section is used to align the first loadable section
		var none_section = new BinarySection(string.Empty, BinarySectionType.NONE, Array.Empty<byte>());
		overlays.Insert(0, none_section);

		// Group the symbols by their section types
		foreach (var section_symbols in symbols.Values.GroupBy(i => i.Section!.Type))
		{
			var section = overlays.Find(i => i.Type == section_symbols.Key);
			if (section == null) throw new ApplicationException("Symbol did not have a corresponding linker export section");

			foreach (var symbol in section_symbols) { section.Symbols.Add(symbol.Name, symbol); }
		}

		// Form the symbol table
		ElfFormat.CreateSymbolRelatedSections(overlays, fragments, symbols);

		// Finish the specified dynamic linking information by filling symbol section indices into the symbol entires and writing them to the dynamic symbol table
		if (dynamic_linking_information != null) FinishDynamicLinkingInformation(dynamic_linking_information);

		var section_headers = ElfFormat.CreateSectionHeaders(overlays, symbols, (int)SegmentAlignment);
		var section_bytes = overlays.Sum(i => i.Margin + i.VirtualSize);

		var bytes = (int)SegmentAlignment + section_bytes + section_headers.Count * ElfSectionHeader.Size;

		// Save the location of the program header table
		header.ProgramHeaderOffset = ElfFileHeader.Size;
		header.ProgramHeaderEntryCount = (short)program_headers.Count;
		header.ProgramHeaderSize = ElfProgramHeader.Size;

		// Save the location of the section header table
		header.SectionHeaderOffset = SegmentAlignment + (ulong)section_bytes;
		header.SectionHeaderTableEntryCount = (short)section_headers.Count;
		header.SectionHeaderSize = ElfSectionHeader.Size;
		header.SectionNameEntryIndex = (short)(section_headers.Count - 1);

		// Compute the entry point location
		var entry_point_symbol = symbols[entry];
		header.Entry = (ulong)(entry_point_symbol.Section!.VirtualAddress + entry_point_symbol.Offset);

		var result = new byte[bytes];

		// Write the file header
		BinaryUtility.Write(result, 0, header);

		// Write the program header table now
		var position = (int)header.ProgramHeaderOffset;

		foreach (var program_header in program_headers)
		{
			BinaryUtility.Write(result, position, program_header);
			position += ElfProgramHeader.Size;
		}

		foreach (var section in overlays)
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
			BinaryUtility.Write(result, position, section_header);
			position += ElfSectionHeader.Size;
		}

		return result;
	}
}