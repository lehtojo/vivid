using System.Collections.Generic;
using System.Linq;
using System;

public static class Linker
{
	public const ulong VirtualAddressStart = 0x400000;
	public const ulong SegmentAlignment = 0x1000;

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
			if (!definitions.TryGetValue(symbol.Name, out var definition)) continue; // throw new ApplicationException($"Symbol '{symbol.Name}' is not defined");

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

		foreach (var sections in fragments.GroupBy(i => i.Type))
		{
			var type = sections.Key;
			var name = ElfFormat.GetSectionDefaultName(type);
			var bytes = sections.Sum(i => i.Data.Length);
			
			// Align the next loadable section
			bytes = (bytes / (int)SegmentAlignment + 1) * (int)SegmentAlignment;

			result.Add(new BinarySection(name, type, Array.Empty<byte>(), bytes));
		}

		return result;
	}

	/// <summary>
	/// Creates the program headers, meaning the specified section will get their own virtual addreses and be loaded into memory when the created executable is loaded
	/// </summary>
	public static int CreateProgramHeaders(List<BinarySection> sections, List<BinarySection> fragments, List<ElfProgramHeader> headers)
	{
		var virtual_address = VirtualAddressStart + SegmentAlignment;
		var file_position = (int)SegmentAlignment;

		var header = new ElfProgramHeader();
		header.Type = ElfSegmentType.LOADABLE;
		header.Flags = ElfSegmentFlag.READ;
		header.Offset = 0;
		header.VirtualAddress = VirtualAddressStart;
		header.PhysicalAddress = VirtualAddressStart;
		header.SegmentFileSize = (ulong)file_position;
		header.SegmentMemorySize = (ulong)file_position;
		header.Alignment = SegmentAlignment;

		headers.Add(header);

		foreach (var section in sections)
		{
			var flags = section.Type switch
			{
				BinarySectionType.DATA => ElfSegmentFlag.WRITE | ElfSegmentFlag.READ,
				BinarySectionType.TEXT => ElfSegmentFlag.EXECUTE | ElfSegmentFlag.READ,
				_ => ElfSegmentFlag.READ
			};

			header = new ElfProgramHeader();
			header.Type = ElfSegmentType.LOADABLE;
			header.Flags = flags;
			header.Offset = (uint)file_position;
			header.VirtualAddress = virtual_address;
			header.PhysicalAddress = virtual_address;
			header.SegmentFileSize = (uint)section.Size;
			header.SegmentMemorySize = (uint)section.Size;
			header.Alignment = SegmentAlignment;

			section.Offset = file_position;
			section.VirtualAddress = (int)virtual_address;

			// Decide the positions of inner fragments of the current section
			foreach (var fragment in fragments.FindAll(i => i.Type == section.Type))
			{
				fragment.Offset = file_position;
				fragment.VirtualAddress = (int)virtual_address;
				
				file_position += fragment.Data.Length;
				virtual_address += (uint)fragment.Data.Length;
			}

			file_position = section.Offset + section.Size;
			virtual_address = (ulong)(section.VirtualAddress + section.Size);

			headers.Add(header);
		}

		return file_position;
	}

	/// <summary>
	/// Computes relocations inside the specified object files using section virtual addresses
	/// </summary>
	public static void ComputeRelocations(List<BinaryObjectFile> objects)
	{
		foreach (var relocation in objects.SelectMany(i => i.Sections).SelectMany(i => i.Relocations))
		{
			var symbol = relocation.Symbol;
			var symbol_section = symbol.Section; // ?? throw new ApplicationException("Missing symbol definition section");
			var relocation_section = relocation.Section; // ?? throw new ApplicationException("Missing relocation section");

			if (symbol_section == null || relocation_section == null) continue;

			if (relocation.Type == BinaryRelocationType.PROGRAM_COUNTER)
			{
				var from = relocation_section.VirtualAddress + relocation.Offset;
				var to = symbol_section.VirtualAddress + symbol.Offset;
				EncoderX64.WriteInt32(relocation_section.Data, relocation.Offset, to - from + relocation.Addend);
			}
			else if (relocation.Type == BinaryRelocationType.ABSOLUTE)
			{
				EncoderX64.WriteInt32(relocation_section.Data, relocation.Offset, symbol_section.VirtualAddress + symbol.Offset);
			}
			else
			{
				throw new ApplicationException("Unsupported relocation type");
			}
		}
	}

	public static byte[] Link(List<BinaryObjectFile> objects)
	{
		var header = new ElfFileHeader();
		header.Type = ElfObjectFileType.EXECUTABLE;
		header.Machine = ElfMachineType.X64;
		header.FileHeaderSize = ElfFileHeader.Size;
		header.SectionHeaderSize = ElfSectionHeader.Size;

		// Resolves are unresolved symbols and returns all symbols as a list
		var symbols = ResolveSymbols(objects);

		// Create the program headers
		var program_headers = new List<ElfProgramHeader>();

		// Ensure sections are ordered so that sections of same type are next to each other
		var fragments = objects.SelectMany(i => i.Sections).Where(IsLoadableSection).OrderBy(i => i.Type).ToList();

		// Create sections, which cover the fragmented sections
		var sections = CreateLoadableSections(fragments);
		CreateProgramHeaders(sections, fragments, program_headers);

		// Now that sections have their virtual addresses relocations can be computed
		ComputeRelocations(objects);

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
		ElfFormat.CreateSymbolRelatedSections(sections);

		var section_headers = ElfFormat.CreateSectionHeaders(sections, symbols, (int)SegmentAlignment);
		var section_bytes = sections.Sum(i => i.Size);

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
		var entry_point_symbol = symbols["_V4initv_rx"];
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