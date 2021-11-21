using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Linq;
using System.IO;
using System;

public enum PeMachineType
{
	X64 = 0x8664,
	ARM64 = 0xAA64,
}

public enum PeFormatStorageClass
{
	EXTERNAL = 2,
	LABEL = 6
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PeSymbolEntry
{
	public const int Size = 18;

	public ulong Name { get; set; }
	public uint Value { get; set; }
	public short SectionNumber { get; set; }
	public ushort Type { get; set; } = 0;
	public byte StorageClass { get; set; } = 0;
	public byte NumberOfAuxiliarySymbols { get; set; } = 0;
}

public enum PeFormatRelocationType
{
	ABSOLUTE64 = 0x1,
	ABSOLUTE32 = 0x2,
	PROGRAM_COUNTER_RELATIVE_32 = 0x4
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PeRelocationEntry
{
	public const int Size = 10;

	public uint VirtualAddress { get; set; }
	public uint SymbolTableIndex { get; set; }
	public ushort Type { get; set; }
}

[Flags]
public enum PeFormatSectionCharacteristics : uint
{
	CODE = 0x20,
	INITIALIZED_DATA = 0x40,
	ALIGN_1 = 0x00100000,
	EXECUTE = 0x20000000,
	READ = 0x40000000,
	WRITE = 0x80000000,
}

public enum PeFormatImageCharacteristics : uint
{
	RELOCATIONS_STRIPPED = 0x0001,
	EXECUTABLE = 0x0002,
	LINENUMBERS_STRIPPED = 0x0004,
	LARGE_ADDRESS_AWARE = 0x0020,
	DEBUG_STRIPPED = 0x0200,
	DLL = 0x2000,
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PeLegacyHeader
{
	public const int Size = 64;
	public const int PeHeaderPointerOffset = Size - sizeof(int);

	public int Signature { get; set; } = 0x5A4D;
}

public class PeImportDirectoryTable
{
	public const int Size = 20;
	public const int NameOffset = 12;
	public const int ImportAddressTableOffset = 16;

	public uint ImportLookupTable { get; set; } // List of all the importers
	public uint TimeStamp { get; set; } = 0;
	public uint ForwarderChain { get; set; } = 0;
	public uint Name { get; set; }
	public uint ImportAddressTable { get; set; } // List of all the function pointer to the imported functions
}

public static class PeFormat
{
	public const int FileSectionAlignment = 0x200;
	public const int VirtualSectionAlignment = 0x1000;

	public const int NumberOfDataDirectories = 16;

	public const int ExporterSectionIndex = 0;
	public const int ImporterSectionIndex = 1;
	public const int ImportAddressSectionIndex = 12;

	public const int SIGNATURE = 0x00004550; // 'PE\0\0'
	public const string RELOCATION_TABLE_SECTION_PREFIX = ".r";

	public const int HEADER_ADDRESS_OFFSET = 0x3C;
	public const int HEADER_START_SIZE = 24;
	public const string EXPORT_TABLE_NAME = ".edata\0\0";
	
	/// <summary>
	/// Loads the offset of the PE header in the image file
	/// </summary>
	public static int GetHeaderOffset(byte[] bytes)
	{
		return BitConverter.ToInt32(bytes, HEADER_ADDRESS_OFFSET);
	}

	/// <summary>
	/// Reads the specified type from the specified bytes at the specified offset
	/// </summary>
	public static T Read<T>(byte[] bytes, int offset)
	{
		var size = Marshal.SizeOf<T>();
		var buffer = Marshal.AllocHGlobal(size);
		Marshal.Copy(bytes, offset, buffer, size);

		var result = Marshal.PtrToStructure<T>(buffer) ?? throw new ApplicationException("Could not convert byte array to type");

		Marshal.FreeHGlobal(buffer);
		return result;
	}

	/// <summary>
	/// Loads all the data directories from the specified image file bytes starting from the specified offset
	/// </summary>
	public static PeDataDirectory[]? LoadDataDirectories(byte[] bytes, int start, int count)
	{
		if (count == 0) return Array.Empty<PeDataDirectory>();

		var directories = new List<PeDataDirectory>();
		var length = Marshal.SizeOf<PeDataDirectory>();
		var end = false;

		while (start + length <= bytes.Length)
		{
			if (count-- <= 0)
			{
				end = true;
				break;
			}

			directories.Add(Read<PeDataDirectory>(bytes, start));
			start += length;
		}

		return end ? directories.ToArray() : null;
	}

	/// <summary>
	/// Loads the specified number of section tables from the specified image file bytes starting from the specified offset
	/// </summary>
	public static PeSectionTable[]? LoadSectionTables(byte[] bytes, int start, int count)
	{
		if (count == 0) return Array.Empty<PeSectionTable>();

		var directories = new List<PeSectionTable>();
		var length = Marshal.SizeOf<PeSectionTable>();
		var end = false;

		while (start + length <= bytes.Length)
		{
			if (count-- <= 0)
			{
				end = true;
				break;
			}

			directories.Add(Read<PeSectionTable>(bytes, start));
			start += length;
		}

		return end ? directories.ToArray() : null;
	}

	/// <summary>
	/// Loads library metadata including the PE-header, data directories and section tables
	/// </summary>
	public static PeMetadata? LoadLibraryMetadata(string file)
	{
		try
		{
			// Load the image file and determine the PE header offset
			var bytes = File.ReadAllBytes(file);
			var header_offset = PeFormat.GetHeaderOffset(bytes);

			if (header_offset < 0 || header_offset + Marshal.SizeOf<PeHeader>() > bytes.Length) return null;

			// Read the PE-header
			var header = Read<PeHeader>(bytes, header_offset);

			// Load the data directories, which come after the header
			var data_directories_offset = header_offset + PeHeader.Size;
			var data_directories = LoadDataDirectories(bytes, data_directories_offset, header.NumberOfDataDirectories);

			if (data_directories == null || header.NumberOfSections < 0) return null;

			// Load the section tables, which come after the data directories
			var section_table_offset = header_offset + PeHeader.OptionalHeaderOffset + header.SizeOfOptionalHeader;
			var sections = LoadSectionTables(bytes, section_table_offset, header.NumberOfSections);

			if (sections == null) return null;

			return new PeMetadata(bytes, header, data_directories, sections);
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// Finds a section with the specified name from the specified metadata.
	/// Ensure the specified name is exactly eight characters long, padded with null characters if necessary.
	/// </summary>
	public static PeSectionTable? FindSection(PeMetadata module, string name)
	{
		var bytes = Encoding.UTF8.GetBytes(name);

		if (bytes.Length != 8)
		{
			throw new ArgumentException("Section name must be eight characters long. If the actual section name is shorter, it must be padded with null characters");
		}

		return module.Sections.FirstOrDefault(i => i.Name == BitConverter.ToUInt64(bytes));
	}

	/// <summary>
	/// Tries to find an export section from the specified metadata.
	/// </summary>
	public static PeSectionTable? FindExportSection(PeMetadata module)
	{
		return FindSection(module, EXPORT_TABLE_NAME);
	}

	/// <summary>
	/// Loads strings the specified amount starting from the specified position.
	/// </summary>
	public static string[]? LoadNumberOfStrings(byte[] bytes, int position, int count)
	{
		if (position < 0) return null;

		var strings = new string[count];

		for (var i = 0; i < count; i++)
		{
			var end = position;
			for (; end < bytes.Length && bytes[end] != 0; end++) { }

			strings[i] = Encoding.UTF8.GetString(bytes[position..end]);

			position = end + 1;
		}

		return strings;
	}

	/// <summary>
	/// Loads strings starting from the specified position until the limit is reached.
	/// </summary>
	public static string[]? LoadStringsUntil(byte[] bytes, int position, int limit)
	{
		if (position < 0) return null;

		var strings = new List<string>();

		while (position < limit)
		{
			var end = position;
			for (; end < limit && bytes[end] != 0; end++) { }

			strings.Add(Encoding.UTF8.GetString(bytes[position..end]));

			position = end + 1;
		}

		return strings.ToArray();
	}

	/// <summary>
	/// Extracts the exported symbols from the specified export section.
	/// </summary>
	public static string[]? LoadExportedSymbols(PeMetadata module)
	{
		// Check if the library has an export table
		var export_data_directory = module.DataDirectories[ExporterSectionIndex];
		if (export_data_directory.RelativeVirtualAddress == 0) return null;

		var export_directory_table = Read<PeExportDirectoryTable>(module.Bytes, (int)export_data_directory.RelativeVirtualAddress);

		// Skip the export directory table, the export address table, the name pointer table and the ordinal table
		var export_directory_table_size = Marshal.SizeOf<PeExportDirectoryTable>();
		var export_address_table_size = export_directory_table.AddressTableEntries * sizeof(int);
		var name_pointer_table_size = export_directory_table.NumberOfNamePointers * sizeof(int);
		var ordinal_table_size = export_directory_table.NumberOfNamePointers * sizeof(short);

		var start = export_data_directory.RelativeVirtualAddress + export_directory_table_size + export_address_table_size + name_pointer_table_size + ordinal_table_size;

		// Load one string more since the first name is the name of the module and it is not counted
		var strings = LoadStringsUntil(module.Bytes, (int)start, export_data_directory.RelativeVirtualAddress + export_data_directory.PhysicalSize);

		// Skip the name of the module if the load was successful
		return strings?[1..];
	}

	/// <summary>
	/// Loads the exported symbols from the specified library.
	/// </summary>
	public static string[]? LoadExportedSymbols(string library)
	{
		var metadata = LoadLibraryMetadata(library);
		if (metadata == null) return null;

		return LoadExportedSymbols(metadata);
	}

	/// <summary>
	/// Converts the relocation type to corresponding PE relocation type
	/// </summary>
	public static ushort GetRelocationType(BinaryRelocationType type)
	{
		return type switch
		{
			BinaryRelocationType.PROCEDURE_LINKAGE_TABLE => (ushort)PeFormatRelocationType.PROGRAM_COUNTER_RELATIVE_32,
			BinaryRelocationType.PROGRAM_COUNTER_RELATIVE => (ushort)PeFormatRelocationType.PROGRAM_COUNTER_RELATIVE_32,
			BinaryRelocationType.ABSOLUTE64 => (ushort)PeFormatRelocationType.ABSOLUTE64,
			BinaryRelocationType.ABSOLUTE32 => (ushort)PeFormatRelocationType.ABSOLUTE32,
			_ => 0
		};
	}
	
	/// <summary>
	/// Determines appropriate section flags for the specified section
	/// </summary>
	public static uint GetSectionCharacteristics(BinarySection section)
	{
		var type = section.Type;
		var characteristics = (uint)PeFormatSectionCharacteristics.READ;

		if (type == BinarySectionType.TEXT)
		{
			characteristics |= (uint)PeFormatSectionCharacteristics.EXECUTE;
			characteristics |= (uint)PeFormatSectionCharacteristics.CODE;
		}
		else if (type == BinarySectionType.DATA)
		{
			characteristics |= (uint)PeFormatSectionCharacteristics.WRITE;
			characteristics |= (uint)PeFormatSectionCharacteristics.INITIALIZED_DATA;
		}

		// Now, compute the alignment flag
		// 1-byte alignment:    0x00100000
		// 2-byte alignment:    0x00200000
		//                          .
		//                          .
		//                          .
		// 8192-byte alignment: 0x00E00000
		characteristics |= (uint)(PeFormatSectionCharacteristics.ALIGN_1 + (uint)Math.Log2(section.Alignment)) << 20;

		return characteristics;
	}

	/// <summary>
	/// Creates the symbol table and the relocation table based on the specified symbols
	/// </summary>
	public static BinarySection CreateSymbolRelatedSections(BinaryStringTable symbol_name_table, List<BinarySection> sections, List<BinarySection>? fragments, Dictionary<string, BinarySymbol> symbols, int file_position)
	{
		var symbol_entries = new List<PeSymbolEntry>();

		// Index the sections since the symbols need that
		for (var i = 0; i < sections.Count; i++)
		{
			var section = sections[i];
			section.Index = i;

			if (fragments == null) continue;
			
			// Index the section fragments as well according to the overlay section index
			// Store the virtual address of the first fragment into all the fragments (base virtual address)
			foreach (var fragment in fragments)
			{
				if (fragment.Name != section.Name) continue;
				fragment.BaseVirtualAddress = section.VirtualAddress;
				fragment.Index = i;
			}
		}

		foreach (var symbol in symbols.Values)
		{
			var base_virtual_address = symbol.Section == null ? 0 : symbol.Section.BaseVirtualAddress;
			var virtual_address = symbol.Section == null ? 0 : symbol.Section.VirtualAddress;

			var symbol_entry = new PeSymbolEntry();
			symbol_entry.Value = (uint)(virtual_address + symbol.Offset - base_virtual_address); // Symbol locations are relative to the start of the section
			symbol_entry.SectionNumber = (short)(symbol.External ? 0 : symbol.Section!.Index + 1);
			symbol_entry.StorageClass = symbol.External ? (byte)PeFormatStorageClass.EXTERNAL : (byte)PeFormatStorageClass.LABEL;

			// Now we need to attach the symbol name to the symbol entry
			// If the length of the name is greater than 8, we need to create a new string table entry
			// Otherwise, we can just store the name in the symbol entry
			if (symbol.Name.Length > 8)
			{
				// Add the symbol name into the string table and receive its offset
				var offset = symbol_name_table.Add(symbol.Name);

				// Set the name offset in the symbol entry
				// The offset must start after four zero bytes so that it can be distinguished from an inlined name (see the other branch below)
				symbol_entry.Name = (ulong)offset << 32;
			}
			else
			{
				// Store the characters inside a 64-bit integer
				var bytes = Encoding.UTF8.GetBytes(symbol.Name).Concat(new byte[8 - symbol.Name.Length]).ToArray();
				symbol_entry.Name = BitConverter.ToUInt64(bytes);
			}

			symbol.Index = (uint)symbol_entries.Count;
			symbol_entries.Add(symbol_entry);
		}

		// Create the relocation section for all sections that have relocations
		var n = sections.Count;

		for (var i = 0; i < n; i++)
		{
			var section = sections[i];
			var relocation_entries = new List<PeRelocationEntry>();

			foreach (var relocation in section.Relocations)
			{
				var relocation_entry = new PeRelocationEntry();
				relocation_entry.VirtualAddress = (uint)(relocation.Section!.Offset + relocation.Offset);
				relocation_entry.SymbolTableIndex = (ushort)relocation.Symbol!.Index;
				relocation_entry.Type = GetRelocationType(relocation.Type);

				relocation_entries.Add(relocation_entry);
			}

			// Determine the name of the relocation section
			var relocation_table_name = RELOCATION_TABLE_SECTION_PREFIX + section.Name[1..];

			// Create a new section for the relocation table
			var relocation_table_section = new BinarySection(relocation_table_name, BinarySectionType.RELOCATION_TABLE, new byte[PeRelocationEntry.Size * relocation_entries.Count]);
			relocation_table_section.Offset = file_position;
			ElfFormat.Write(relocation_table_section.Data, 0, relocation_entries);

			sections.Add(relocation_table_section); // Add the relocation table section to the list of sections

			// Update the file position
			file_position += relocation_table_section.Data.Length;
		}

		// Export the data from the generated string table, since it has to come directly after the symbol table
		var symbol_name_table_data = symbol_name_table.Export();

		// Create the symbol table section
		var symbol_table_section = new BinarySection(ElfFormat.SYMBOL_TABLE_SECTION, BinarySectionType.SYMBOL_TABLE, new byte[PeSymbolEntry.Size * symbol_entries.Count + symbol_name_table_data.Length]);
		symbol_table_section.Offset = file_position;

		// Write the symbol table entries
		ElfFormat.Write(symbol_table_section.Data, 0, symbol_entries);

		// Store the string table data into the symbol table section as well
		Array.Copy(symbol_name_table_data, 0, symbol_table_section.Data, symbol_entries.Count * PeSymbolEntry.Size, symbol_name_table_data.Length);

		sections.Add(symbol_table_section); // Add the symbol table section to the list of sections

		return symbol_table_section;
	}

	/// <summary>
	/// Creates an object file from the specified sections
	/// </summary>
	public static BinaryObjectFile Create(List<BinarySection> sections, HashSet<string> exports)
	{
		#warning Some of the functions inside the ELF-format are very general and should be moved to a separate class

		// Load all the symbols from the specified sections
		var symbols = ElfFormat.GetAllSymbolsFromSections(sections);

		// Export the specified symbols
		ElfFormat.ApplyExports(symbols, exports);

		// Update all the relocations before adding them to binary sections
		ElfFormat.UpdateRelocations(sections, symbols);

		// Add symbols and relocations of each section needing that
		CreateSymbolRelatedSections(new BinaryStringTable(true), sections, null, symbols, 0);

		// Now that section positions are set, compute offsets
		ElfFormat.ComputeOffsets(sections, symbols);

		return new BinaryObjectFile(sections);
	}

	/// <summary>
	/// Creates an object file from the specified sections
	/// </summary>
	public static byte[] Build(List<BinarySection> sections, HashSet<string> exports)
	{
		#warning Some of the functions inside the ELF-format are very general and should be moved to a separate class
		
		// Load all the symbols from the specified sections
		var symbols = ElfFormat.GetAllSymbolsFromSections(sections);

		// Export the specified symbols
		ElfFormat.ApplyExports(symbols, exports);

		// Update all the relocations before adding them to binary sections
		ElfFormat.UpdateRelocations(sections, symbols);

		var symbol_name_table = new BinaryStringTable(true);

		// Create initial versions of section tables and finish them later when section offsets are known
		var section_tables = new List<PeSectionTable>();

		foreach (var section in sections)
		{
			var section_name = section.Name;

			// If the section name is too long, move it into the string table and point to that name by using the pattern '/<Section name offset in the string table>'
			if (section.Name.Length > 8)
			{
				section_name = '/' + symbol_name_table.Add(section.Name).ToString(CultureInfo.InvariantCulture);
			}

			var bytes = Encoding.UTF8.GetBytes(section_name).Concat(new byte[8 - section.Name.Length]).ToArray();

			var section_table = new PeSectionTable
			{
				Name = BitConverter.ToUInt64(bytes),
				VirtualAddress = 0,
				VirtualSize = section.VirtualSize,
				SizeOfRawData = section.VirtualSize,
				PointerToRawData = 0, // Fill in later when the section offsets are decided
				PointerToRelocations = 0, // Fill in later when the section offsets are decided
				PointerToLinenumbers = 0, // Not used
				NumberOfRelocations = 0, // Fill in later
				NumberOfLinenumbers = 0, // Not used
				Characteristics = GetSectionCharacteristics(section),
			};

			section_tables.Add(section_table);
		}

		// Exclude the sections created below and go with the existing ones, since the ones created below are not needed in the section tables
		var header = new PeObjectFileHeader
		{
			NumberOfSections = (short)sections.Count,
			Machine = (ushort)PeMachineType.X64,
			TimeDateStamp = (uint)DateTime.Now.ToFileTimeUtc(),
			Characteristics = (short)(PeFormatImageCharacteristics.LARGE_ADDRESS_AWARE | PeFormatImageCharacteristics.LINENUMBERS_STRIPPED | PeFormatImageCharacteristics.DEBUG_STRIPPED),
		};

		// Add symbols and relocations of each section needing that
		CreateSymbolRelatedSections(symbol_name_table, sections, null, symbols, 0);

		if (!sections.Exists(i => i.Type == BinarySectionType.RELOCATION_TABLE))
		{
			header.Characteristics |= (short)PeFormatImageCharacteristics.RELOCATIONS_STRIPPED;
		}

		header.SizeOfOptionalHeader = 0;

		// Decide section offsets
		var file_position = PeObjectFileHeader.Size + section_tables.Count * PeSectionTable.Size;

		foreach (var section in sections)
		{
			section.Offset = file_position;
			file_position += section.Data.Length;
		}

		// Now, finish the section tables
		for (var i = 0; i < section_tables.Count; i++)
		{
			var section_table = section_tables[i];
			var section = sections[i];

			section_table.PointerToRawData = (uint)section.Offset;

			// Skip relocations if there are none
			if (!section.Relocations.Any()) continue;

			// Why does PE-format restrict the number of relocations to 2^16 in a single section...
			if (section.Relocations.Count > ushort.MaxValue) throw new ApplicationException("Too many relocations");

			// Find the relocation table for this section
			var relocation_table_name = RELOCATION_TABLE_SECTION_PREFIX + section.Name[1..];
			var relocation_table = sections.Find(i => i.Name == relocation_table_name) ?? throw new ApplicationException("Missing relocation section");

			section_table.PointerToRelocations = (uint)relocation_table.Offset;
			section_table.NumberOfRelocations = (ushort)section.Relocations.Count;
		}

		// Now that section positions are set, compute offsets
		ElfFormat.ComputeOffsets(sections, symbols);

		// Store the location of the symbol table
		var symbol_table = sections.Find(i => i.Name == ElfFormat.SYMBOL_TABLE_SECTION);

		if (symbol_table != null)
		{
			header.NumberOfSymbols = symbols.Count;
			header.PointerToSymbolTable = (uint)symbol_table.Offset;
		}

		// Create the binary file
		var binary = new byte[file_position];

		// Write the file header
		ElfFormat.Write(binary, 0, header);

		// Write the section tables
		ElfFormat.Write(binary, PeObjectFileHeader.Size, section_tables);

		// Write the sections
		foreach (var section in sections)
		{
			var section_data = section.Data;

			// Write the section data
			Array.Copy(section_data, 0, binary, section.Offset, section_data.Length);
		}

		return binary;
	}

	public static int AlignSections(List<BinarySection> overlays, List<BinarySection> fragments, int file_position)
	{
		// Align the file positions and virtual addresses of the overlays.
		foreach (var section in overlays)
		{
			// Determine the virtual address alignment to use, since the section can request for larger alignment than the default.
			var alignment = Math.Max(section.Alignment, VirtualSectionAlignment);

			// Align the file position
			file_position = (file_position + alignment - 1) & ~(alignment - 1);

			section.VirtualAddress = file_position;
			section.Offset = file_position;

			// Now, decide the file position and virtual address for the fragments
			foreach (var fragment in fragments)
			{
				if (fragment.Name != section.Name) continue;

				// Align the file position with the fragment alignment
				file_position = (file_position + fragment.Alignment - 1) & ~(fragment.Alignment - 1);

				fragment.VirtualAddress = file_position;
				fragment.Offset = file_position;

				// Move to the next fragment
				file_position += fragment.Data.Length;
			}

			// Update the overlay size
			section.VirtualSize = file_position - section.Offset;
			section.LoadSize = section.VirtualSize;
		}

		return file_position;
	}

	/// <summary>
	/// Generates dynamic linkage information for the specified imported symbols. This function generates the following structures:
	//
	// .section .idata
	// <Directory table 1>:
	// <import-lookup-table-1> (4 bytes)
	// Timestamp: 0x00000000
	// Forwarded chain: 0x00000000
	// <library-name-1> (4 bytes)
	// <import-address-table-1> (4 bytes)
	//
	// <Directory table 2>:
	// <import-lookup-table-2> (4 bytes)
	// Timestamp: 0x00000000
	// Forwarded chain: 0x00000000
	// <library-name-2> (4 bytes)
	// <import-address-table-2> (4 bytes)
	// 
	// ...
	//
	// <Directory table n>:
	// Import lookup table: 0x00000000
	// Timestamp: 0x00000000
	// Forwarded chain: 0x00000000
	// Name: 0x00000000
	// Import address table: 0x00000000
	//
	// <import-lookup-table-1>:
	// 0x00000000 <.string.<function-1>>
	// 0x00000000 <.string.<function-2>>
	// 0x0000000000000000
	// <import-lookup-table-2>:
	// 0x00000000 <.string.<function-3>>
	// 0x00000000 <.string.<function-4>>
	// 0x0000000000000000
	//
	// ...
	//
	// <import-lookup-table-n>:
	// 0x00000000 <.string.<function-(n-1)>>
	// 0x00000000 <.string.<function-n>>
	// 0x0000000000000000
	//
	// <library-name-1>: ... 0
	// <library-name-2>: ... 0
	//
	// ...
	//
	// <library-name-n>: ... 0
	//
	// <.string.<function-1>>: ... 0
	// <.string.<function-2>>: ... 0
	//
	// ...
	//
	// <.string.<function-n>>: ... 0
	//
	// .section .text
	// <imports-1>:
	// <function-1>: jmp qword [.import.<function-1>]
	// <function-2>: jmp qword [.import.<function-2>]
	//
	// ...
	//
	// <function-n>: jmp qword [.import.<function-n>]
	//
	// <imports-2>:
	// ...
	// <imports-n>:
	// 
	// .section .data
	// <import-address-table-1>:
	// <.import.<function-1>>: .qword 0
	// <.import.<function-2>>: .qword 0
	// ...
	// <.import.<function-n>>: .qword 0
	//
	// <import-address-table-2>:
	// ...
	// <import-address-table-n>:
	/// </summary>
	public static BinarySection CreateDynamicLinkage(List<BinaryRelocation> relocations, List<string> imports, List<BinarySection> fragments)
	{
		var externals = relocations.Where(i => i.Symbol.External).ToList();
		var exports = imports.Select(LoadExportedSymbols).ToArray();

		// There can be multiple relocations, which refer to the same symbol but the symbol object instances are different (relocations can be in different objects).
		// Therefore, we need to create a dictionary, which we will use to connect all the relocations into shared symbols.
		var relocation_symbols = new Dictionary<string, BinarySymbol>();
		var importer_section = new BinarySection(ElfFormat.IMPORTER_SECTION, BinarySectionType.DATA, Array.Empty<byte>());
		var import_section_instructions = new List<Instruction>();
		var import_address_section_builder = new DataEncoderModule();
		var import_lists = new Dictionary<string, List<string>>();
		var string_table = new DataEncoderModule();

		import_address_section_builder.Name = ElfFormat.IMPORTER_SECTION;

		foreach (var relocation in externals)
		{
			// If the relocation symbol can be found from the import section symbols, the library which defines the symbol is already found
			if (relocation_symbols.TryGetValue(relocation.Symbol.Name, out BinarySymbol? relocation_symbol))
			{
				relocation.Symbol = relocation_symbol; // Connect the relocation to the shared symbol
				continue;
			}

			// Go through all the libraries and find the one which has the external symbol
			var library = (string?)null;

			for (var i = 0; i < imports.Count; i++)
			{
				var symbols = exports[i];

				// Not being able to load the exported symbols is not fatal, so we can continue, however we should notify the user
				if (symbols == null)
				{
					Console.Error.WriteLine($"{Errors.WARNING_BEGIN}Warning{Errors.WARNING_END}: Could not load exported symbols from library '{imports[i]}'");
					continue;
				}

				if (!symbols.Contains(relocation.Symbol.Name)) continue;

				// Ensure the external symbol is not defined in multiple libraries, because this could cause weird behaviour depending on the order of the imported libraries
				if (library != null) throw new ApplicationException($"Symbol {relocation.Symbol.Name} is defined in both {library} and {imports[i]}");

				library = imports[i];
			}

			// Ensure the library was found
			if (library == null) throw new ApplicationException($"Symbol {relocation.Symbol.Name} is not defined locally or externally");

			// Add the symbol to the import list linked to the library
			if (import_lists.TryGetValue(library, out List<string>? import_list)) { import_list.Add(relocation.Symbol.Name); }
			else { import_lists[library] = new List<string>() { relocation.Symbol.Name }; }

			// Make the symbol local, since it will be defined below as an indirect jump to the actual implementation
			relocation.Symbol.External = false;

			relocation_symbols[relocation.Symbol.Name] = relocation.Symbol;
			importer_section.Symbols.Add(relocation.Symbol.Name, relocation.Symbol);
		}

		// Compute where the string table starts. This is also the amount of bytes needed for the other importer data.
		var string_table_start = (import_lists.Count + 1) * PeImportDirectoryTable.Size + import_lists.Sum(i => i.Value.Count + 1) * sizeof(long);
		importer_section.Data = new byte[string_table_start];

		var import_lookup_table_starts = new BinarySymbol[import_lists.Count];
		var import_address_table_starts = new BinarySymbol[import_lists.Count];
		var position = 0;

		// Write the directory tables
		for (var i = 0; i < import_lists.Count; i++)
		{
			var import_library = imports[i];
			var import_list = import_lists[import_library];

			// Create symbols for the import lookup table, library name and import address table that describe their virtual addresses
			var import_lookup_table_start = new BinarySymbol($".lookup.{i}", 0, false, importer_section);
			var import_library_name_start = new BinarySymbol($".library.{i}", 0, false, importer_section);
			var import_address_table_start = new BinarySymbol($".imports.{i}", 0, false);

			// Add the library name to the string table and compute its offset
			string_table.WriteInt16(0);
			import_library_name_start.Offset = string_table_start + string_table.Position;

			var library_filename = Path.GetFileName(import_library);
			string_table.String(library_filename);

			// Align the next entry on an even boundary
			if (string_table.Position % 2 != 0) string_table.Write(0);

			import_lookup_table_starts[i] = import_lookup_table_start;
			import_address_table_starts[i] = import_address_table_start;

			// Fill the locations of the symbols into the import directory when their virtual addresses have been decided
			relocations.Add(new BinaryRelocation(import_lookup_table_start, position, 0, BinaryRelocationType.ABSOLUTE32, importer_section));
			relocations.Add(new BinaryRelocation(import_library_name_start, position + PeImportDirectoryTable.NameOffset, 0, BinaryRelocationType.ABSOLUTE32, importer_section));
			relocations.Add(new BinaryRelocation(import_address_table_start, position + PeImportDirectoryTable.ImportAddressTableOffset, 0, BinaryRelocationType.ABSOLUTE32, importer_section));

			// Move to the next directory table
			position += PeImportDirectoryTable.Size;
		}

		position += PeImportDirectoryTable.Size; // Skip the empty directory table at the end

		// Populate the string table with the imported symbols and create the import lookup tables
		for (var i = 0; i < import_lists.Count; i++)
		{
			var import_list = import_lists[imports[i]];

			// Store the relative offset of the import address table in the import address table section (Imports)
			import_address_table_starts[i].Offset = import_address_section_builder.Position;

			// Store the location of this import lookup table in the symbol, which represents it (Importer)
			import_lookup_table_starts[i].Offset = position;

			foreach (var import_symbol_name in import_list)
			{
				#warning Add support for the order number?

				// Create a symbol for the import so that a relocation can be made
				var import_symbol_offset = string_table_start + string_table.Position;
				string_table.WriteInt16(0); // This 16-bit index is used for quick lookup of the symbol in the imported library, do not care about it for now
				string_table.String(import_symbol_name);

				// Align the next entry on an even boundary
				if (string_table.Position % 2 != 0) string_table.Write(0);

				var import_symbol = new BinarySymbol(".string." + import_symbol_name, import_symbol_offset, false, importer_section);

				// Fill in the location of the imported symbol when its virtual address is decided
				relocations.Add(new BinaryRelocation(import_symbol, position, 0, BinaryRelocationType.ABSOLUTE32, importer_section));
				position += sizeof(long);

				// Reserve space for the address of the imported function when it is loaded, also create a symbol which represents the location of the address
				var import_address_symbol = import_address_section_builder.CreateLocalSymbol(".import." + import_symbol_name, import_address_section_builder.Position, false);
				import_address_section_builder.Relocations.Add(new BinaryRelocation(import_symbol, import_address_section_builder.Position, 0, BinaryRelocationType.ABSOLUTE32));
				import_address_section_builder.WriteInt64(0);

				#warning Support Arm64
				if (Assembler.IsArm64) throw new NotImplementedException("Import code generation is not implemented for Arm64");

				// Create a label, which defines the imported function so that the other code can jump indirectly to the actual implementation
				// Instruction: <function>:
				import_section_instructions.Add(new LabelInstruction(null!, new Label(import_symbol_name)));

				// Create an instruction, which jumps to the location of the imported function address
				// Instruction: jmp qword [.import.<function>]
				var instruction = new Instruction(null!, InstructionType.JUMP);
				instruction.Operation = Instructions.X64.JUMP;

				// Create a handle, which the instruction uses refer to the imported function
				var import_address_handle = new DataSectionHandle(import_address_symbol.Name);
				instruction.Parameters.Add(new InstructionParameter(import_address_handle, ParameterFlag.NONE));

				import_section_instructions.Add(instruction);
			}

			position += sizeof(long); // Skip the none-symbol at the end of each import lookup table
		}

		// Build the import address section
		var import_address_section = import_address_section_builder.Export();

		// Build the import section
		var import_section_build = InstructionEncoder.Encode(import_section_instructions, null);
		var import_section = import_section_build.Section;

		// Connect the indirect jump instruction relocations to the import address section
		ElfFormat.UpdateRelocations(import_section_build.Relocations, import_address_section.Symbols);

		// Export the new relocation needed by the import section. The relocations point to import address tables.
		relocations.AddRange(import_section_build.Relocations);
		relocations.AddRange(import_address_section.Relocations);

		// Now, currently the relocations which use the imported functions, have symbols which basically point to nothing. We need to connect them to the indirect jumps.
		foreach (var relocation_symbol in relocation_symbols)
		{
			var indirect_jump_symbol = import_section.Symbols[relocation_symbol.Key];

			// Copy the properties of the indirect jump symbol to the relocation symbol
			relocation_symbol.Value.Offset = indirect_jump_symbol.Offset;
			relocation_symbol.Value.Section = indirect_jump_symbol.Section;
		}

		// Connect the import address table symbols to the import address section
		foreach (var import_address_table_start in import_address_table_starts) { import_address_table_start.Section = import_address_section; }

		// Mesh the importer data with the string table
		importer_section.Data = importer_section.Data.Concat(string_table.Export().Data).ToArray();

		// Export the generated sections
		fragments.Add(importer_section);
		fragments.Add(import_section);
		fragments.Add(import_address_section);

		return import_address_section;
	}

	/// <summary>
	/// Creates an object file from the specified sections
	/// </summary>
	public static byte[] Link(List<BinaryObjectFile> objects, List<string> imports, string entry, bool executable)
	{
		#warning Some of the functions inside the ELF-format are very general and should be moved to a separate class

		// Index all the specified object files
		for (var i = 0; i < objects.Count; i++) { objects[i].Index = i; }

		// Make all hidden symbols unique by using their object file indices
		Linker.MakeLocalSymbolsUnique(objects);

		var header = new PeHeader
		{
			Machine = (ushort)PeMachineType.X64,
			TimeDateStamp = (uint)DateTime.Now.ToFileTimeUtc(),
			FileAlignment = FileSectionAlignment,
			SectionAlignment = VirtualSectionAlignment,
			Characteristics = (short)(PeFormatImageCharacteristics.LARGE_ADDRESS_AWARE | PeFormatImageCharacteristics.LINENUMBERS_STRIPPED | PeFormatImageCharacteristics.EXECUTABLE | PeFormatImageCharacteristics.DEBUG_STRIPPED),
			NumberOfDataDirectories = NumberOfDataDirectories,
			SizeOfOptionalHeader = (short)(PeHeader.Size - PeHeader.OptionalHeaderOffset + PeDataDirectory.Size * NumberOfDataDirectories)
		};

		// Resolves are unresolved symbols and returns all symbols as a list
		var symbols = Linker.ResolveSymbols(objects);

		// Ensure sections are ordered so that sections of same type are next to each other
		var fragments = objects.SelectMany(i => i.Sections).Where(Linker.IsLoadableSection).ToList();

		// Load all the relocations from all the sections
		var relocations = objects.SelectMany(i => i.Sections).SelectMany(i => i.Relocations).ToList();

		// Create the import section
		var import_address_section = CreateDynamicLinkage(relocations, imports, fragments);

		// Create sections, which cover the fragmented sections
		var overlays = Linker.CreateLoadableSections(fragments);

		if (!overlays.Exists(i => i.Type == BinarySectionType.RELOCATION_TABLE))
		{
			header.Characteristics |= (short)PeFormatImageCharacteristics.RELOCATIONS_STRIPPED;
		}

		// Store the number of sections to the header
		// Exclude the sections created below and go with the existing ones, since the ones created below are not needed in the section tables
		header.NumberOfSections = (short)overlays.Count;

		// Decide section offsets and virtual addresses
		var file_position = PeLegacyHeader.Size + PeHeader.Size + PeDataDirectory.Size * NumberOfDataDirectories + PeSectionTable.Size * overlays.Count;
		file_position = AlignSections(overlays, fragments, file_position);

		// Store the size of the data
		header.SizeOfInitializedData = overlays.Where(i => i.Type != BinarySectionType.TEXT).Sum(i => i.VirtualSize);

		header.ImageBase = executable ? (long)Linker.VIRTUAL_ADDRESS_START : 0;

		// Section tables:
		// Create initial versions of section tables and finish them later when section offsets are known
		var symbol_name_table = new BinaryStringTable(true);
		var section_tables = new List<PeSectionTable>();

		foreach (var overlay in overlays)
		{
			var overlay_name = overlay.Name;

			// If the section name is too long, move it into the string table and point to that name by using the pattern '/<Section name offset in the string table>'
			if (overlay.Name.Length > 8)
			{
				overlay_name = '/' + symbol_name_table.Add(overlay.Name).ToString(CultureInfo.InvariantCulture);
			}

			var bytes = Encoding.UTF8.GetBytes(overlay_name).Concat(new byte[8 - overlay.Name.Length]).ToArray();

			var section_table = new PeSectionTable
			{
				Name = BitConverter.ToUInt64(bytes),
				VirtualAddress = (uint)overlay.VirtualAddress,
				VirtualSize = overlay.VirtualSize,
				SizeOfRawData = overlay.VirtualSize,
				PointerToRawData = 0, // Fill in later when the section offsets are decided
				PointerToRelocations = 0, // Fill in later when the section offsets are decided
				PointerToLinenumbers = 0, // Not used
				NumberOfRelocations = 0, // Fill in later
				NumberOfLinenumbers = 0, // Not used
				Characteristics = GetSectionCharacteristics(overlay),
			};

			section_tables.Add(section_table);
		}

		// Add symbols and relocations of each section needing that
		var symbol_table_section = CreateSymbolRelatedSections(symbol_name_table, overlays, fragments, symbols, file_position);
		file_position = symbol_table_section.Offset + symbol_table_section.VirtualSize;

		// Section table pointers:
		// Now, finish the section tables
		for (var i = 0; i < section_tables.Count; i++)
		{
			var section_table = section_tables[i];
			var section = overlays[i];

			section_table.PointerToRawData = (uint)section.Offset;
			section_table.VirtualAddress = (uint)section.VirtualAddress;

			// Skip relocations if there are none
			if (!section.Relocations.Any()) continue;

			// Why does PE-format restrict the number of relocations to 2^16 in a single section...
			if (section.Relocations.Count > ushort.MaxValue) throw new ApplicationException("Too many relocations");

			// Find the relocation table for this section
			var relocation_table_name = RELOCATION_TABLE_SECTION_PREFIX + section.Name[1..];
			var relocation_table = overlays.Find(i => i.Name == relocation_table_name) ?? throw new ApplicationException("Missing relocation section");

			section_table.PointerToRelocations = (uint)relocation_table.Offset;
			section_table.NumberOfRelocations = (ushort)section.Relocations.Count;
		}

		// Now that sections have their virtual addresses relocations can be computed
		/// NOTE: Maybe you could pipe all relocations which can not be resolved and then they can be searched from DLLs?
		Linker.ComputeRelocations(relocations);

		// Compute the entry point location
		var entry_point_symbol = symbols[entry];
		header.AddressOfEntryPoint = entry_point_symbol.Section!.VirtualAddress + entry_point_symbol.Offset;

		// Store the size of the code
		var text_section = overlays.Find(i => i.Type == BinarySectionType.TEXT);
		if (text_section != null) { header.SizeOfCode = (int)text_section.VirtualSize; }

		// Register the symbol table to the PE-header
		if (symbol_table_section != null)
		{
			header.NumberOfSymbols = symbols.Count;
			header.PointerToSymbolTable = (uint)symbol_table_section.Offset;
		}

		// Compute the image size, which is the memory needed to load all the sections in place
		var last_loaded_section = overlays.MaxBy(i => i.VirtualAddress) ?? throw new ApplicationException("At least one section should be loaded");
		var image_size = last_loaded_section.VirtualAddress + last_loaded_section.VirtualSize;

		// The actual stored image size must be multiple of section alignment
		header.SizeOfImage = (image_size + header.SectionAlignment - 1) & ~(header.SectionAlignment - 1);

		// Compute the total size of all headers
		var headers_size = PeLegacyHeader.Size + PeHeader.Size + PeDataDirectory.Size * NumberOfDataDirectories + PeSectionTable.Size * overlays.Count;

		// The actual stored size of headers must be multiple of file alignments
		header.SizeOfHeaders = (headers_size + header.FileAlignment - 1) & ~(header.FileAlignment - 1);

		// Create the binary file
		var binary = new byte[file_position];

		// Write the legacy header
		ElfFormat.Write(binary, 0, new PeLegacyHeader());

		// Write the pointer to the PE-header
		InstructionEncoder.WriteInt32(binary, PeLegacyHeader.PeHeaderPointerOffset, PeLegacyHeader.Size);

		// Write the file header
		ElfFormat.Write(binary, PeLegacyHeader.Size, header);

		// Write the data directories
		var importer_section = overlays.Find(i => i.Name == ElfFormat.IMPORTER_SECTION);

		if (importer_section != null) ElfFormat.Write(binary, PeLegacyHeader.Size + PeHeader.Size + PeDataDirectory.Size * ImporterSectionIndex, new PeDataDirectory(importer_section.VirtualAddress, importer_section.VirtualSize));
		if (import_address_section != null) ElfFormat.Write(binary, PeLegacyHeader.Size + PeHeader.Size + PeDataDirectory.Size * ImportAddressSectionIndex, new PeDataDirectory(import_address_section.VirtualAddress, import_address_section.VirtualSize));

		// Write the section tables
		ElfFormat.Write(binary, PeLegacyHeader.Size + PeHeader.Size + PeDataDirectory.Size * NumberOfDataDirectories, section_tables);

		// Write the section overlays
		foreach (var section in overlays)
		{
			// Loadable sections are handled with the fragments
			if (Linker.IsLoadableSection(section)) continue;

			Array.Copy(section.Data, 0, binary, section.Offset, section.Data.Length);
		}

		// Write the loadable sections
		foreach (var fragment in fragments)
		{
			Array.Copy(fragment.Data, 0, binary, fragment.Offset, fragment.Data.Length);
		}

		return binary;
	}

	/// <summary>
	/// Determines shared section flags from the specified section characteristics
	/// </summary>
	public static BinarySectionFlag GetSharedSectionFlags(PeFormatSectionCharacteristics characteristics)
	{
		var flags = (BinarySectionFlag)0;

		if (characteristics.HasFlag(PeFormatSectionCharacteristics.WRITE)) { flags |= BinarySectionFlag.WRITE; }
		if (characteristics.HasFlag(PeFormatSectionCharacteristics.EXECUTE)) { flags |= BinarySectionFlag.EXECUTE; }

		if (characteristics.HasFlag((PeFormatSectionCharacteristics.CODE))) { flags |= BinarySectionFlag.ALLOCATE; }
		else if (characteristics.HasFlag((PeFormatSectionCharacteristics.INITIALIZED_DATA))) { flags |= BinarySectionFlag.ALLOCATE; }

		return flags;
	}

	/// <summary>
	/// Extracts the section alignment from the specified section characteristics
	/// </summary>
	public static int GetSectionAlignment(uint characteristics)
	{
		// Alignment flags are stored as follows:
		// 1-byte alignment:    0x00100000
		// 2-byte alignment:    0x00200000
		//                          .
		//                          .
		//                          .
		// 8192-byte alignment: 0x00E00000
		var exponent = (characteristics >> 20) & 15; // Take out the first four bits: 15 = 0b1111
		return 2 << (int)(exponent - 1); // 2^(exponent - 1)
	}

	/// <summary>
	/// Converts the PE-format relocation type to shared relocation type
	/// </summary>
	public static BinaryRelocationType GetSharedRelocationType(PeFormatRelocationType type)
	{
		return type switch
		{
			PeFormatRelocationType.PROGRAM_COUNTER_RELATIVE_32 => BinaryRelocationType.PROGRAM_COUNTER_RELATIVE,
			PeFormatRelocationType.ABSOLUTE64 => BinaryRelocationType.ABSOLUTE64,
			PeFormatRelocationType.ABSOLUTE32 => BinaryRelocationType.ABSOLUTE32,
			_ => 0
		};
	}

	/// <summary>
	/// Imports all symbols and relocations from the represented object file
	/// </summary>
	public static void ImportSymbolsAndRelocations(PeObjectFileHeader header, List<BinarySection> sections, List<PeSectionTable> section_tables, IntPtr bytes)
	{
		var file_position = bytes + (int)header.PointerToSymbolTable;
		var symbol_name_table_start = file_position + (int)header.NumberOfSymbols * PeSymbolEntry.Size;

		/// NOTE: This is useful for the relocation table below
		var symbols = new List<BinarySymbol>();

		for (var i = 0; i < header.NumberOfSymbols; i++)
		{
			// Load the next symbol entry
			var symbol_entry = Marshal.PtrToStructure<PeSymbolEntry>(file_position) ?? throw new ApplicationException("Could not load a symbol entry");
			var symbol_name = (string?)null;
			
			// If the section number is a positive integer, the symbol is defined locally inside some section
			var section = symbol_entry.SectionNumber > 0 ? sections[symbol_entry.SectionNumber - 1] : null;

			// Extract the symbol name:
			// If the first four bytes are zero, the symbol name is located in the string table
			// Otherwise, the symbol name is stored inside the integer
			if ((symbol_entry.Name & 0xFFFFFFFF) == 0)
			{
				var symbol_name_offset = symbol_entry.Name >> 32;
				symbol_name = Marshal.PtrToStringUTF8(symbol_name_table_start + (int)symbol_name_offset) ?? throw new ApplicationException("Could not extract symbol name");
			}
			else
			{
				// Extract the symbol name from the integer
				symbol_name = Encoding.UTF8.GetString(BitConverter.GetBytes(symbol_entry.Name)).TrimEnd('\0');
			}

			var symbol = new BinarySymbol(symbol_name, (int)symbol_entry.Value, symbol_entry.SectionNumber == 0);
			symbol.Export = symbol_entry.SectionNumber != 0 && symbol_entry.StorageClass == (int)PeFormatStorageClass.EXTERNAL;
			symbol.Section = section;

			// Define the symbol inside its section, if it has a section
			if (section != null) section.Symbols.Add(symbol.Name, symbol);

			symbols.Add(symbol);

			file_position += PeSymbolEntry.Size;
		}

		// Import relocations
		for (var i = 0; i < sections.Count; i++)
		{
			var section = sections[i];
			var section_table = section_tables[i];

			// Skip sections, which do not have relocations
			if (section_table.PointerToRelocations == 0) continue;

			// Determine the location of the first relocation
			file_position = bytes + (int)section_table.PointerToRelocations;

			for (var j = 0; j < section_table.NumberOfRelocations; j++)
			{
				// Load the relocation entry from raw bytes
				var relocation_entry = Marshal.PtrToStructure<PeRelocationEntry>(file_position) ?? throw new ApplicationException("Could not load a relocation entry");

				var symbol = symbols[(int)relocation_entry.SymbolTableIndex];
				var relocation_type = GetSharedRelocationType((PeFormatRelocationType)relocation_entry.Type);
				var relocation = new BinaryRelocation(symbol, (int)relocation_entry.VirtualAddress, -sizeof(int), relocation_type);
				relocation.Section = section;

				// Set the default addend if the relocation type is program counter relative
				if (relocation_type == BinaryRelocationType.PROGRAM_COUNTER_RELATIVE) { relocation.Addend = -sizeof(int); }

				section.Relocations.Add(relocation);
				file_position += PeRelocationEntry.Size;
			}
		}

		// Now, fix section names that use the pattern '/<Section name offset in the string table>'
		foreach (var section in sections)
		{
			if (!section.Name.StartsWith('/')) continue;

			// Extract the section offset in the string table
			var section_name_offset = int.Parse(section.Name[1..], CultureInfo.InvariantCulture);

			// Load the section name from the string table
			section.Name = Marshal.PtrToStringUTF8(symbol_name_table_start + (int)section_name_offset) ?? throw new ApplicationException("Could not extract section name from the string table");
		}
	}

	/// <summary>
	/// Load the specified object file and constructs a object structure that represents it
	/// </summary>
	public static BinaryObjectFile Import(string path)
	{
		// Load the file into raw memory
		var source = File.ReadAllBytes(path);
		var bytes = Marshal.AllocHGlobal(source.Length);
		Marshal.Copy(source, 0, bytes, source.Length);

		// Load the file header
		var header = Marshal.PtrToStructure<PeObjectFileHeader>(bytes) ?? throw new ApplicationException("Could not load the file header");

		// Load all the section tables
		var file_position = bytes + PeObjectFileHeader.Size;
		var sections = new List<BinarySection>();

		// Store the section tables for usage after the loop
		var section_tables = new List<PeSectionTable>();

		for (var i = 0; i < header.NumberOfSections; i++)
		{
			// Load the section table in order to load the actual section
			var section_table = Marshal.PtrToStructure<PeSectionTable>(file_position) ?? throw new ApplicationException("Could not load a section header");

			// Create a pointer, which points to the start of the section data in the file
			var section_data_start = bytes + (int)section_table.PointerToRawData;

			// Now load the section data into a buffer
			var section_data = new byte[section_table.SizeOfRawData];
			Marshal.Copy(section_data_start, section_data, 0, section_data.Length);

			// Extract the section name from the section table
			var section_name = Encoding.UTF8.GetString(BitConverter.GetBytes(section_table.Name)).TrimEnd('\0');

			// Determine the section type
			var section_type = section_name switch
			{
				ElfFormat.TEXT_SECTION => BinarySectionType.TEXT,
				ElfFormat.DATA_SECTION => BinarySectionType.DATA,
				ElfFormat.SYMBOL_TABLE_SECTION => BinarySectionType.SYMBOL_TABLE,
				ElfFormat.STRING_TABLE_SECTION => BinarySectionType.STRING_TABLE,
				_ => BinarySectionType.NONE
			};

			// Detect relocation table sections
			if (section_name.StartsWith(RELOCATION_TABLE_SECTION_PREFIX)) { section_type = BinarySectionType.RELOCATION_TABLE; }

			var section = new BinarySection(section_name, section_type, section_data);
			section.Flags = GetSharedSectionFlags((PeFormatSectionCharacteristics)section_table.Characteristics);
			section.Alignment = GetSectionAlignment(section_table.Characteristics);
			section.Offset = (int)section_table.PointerToRawData;
			section.VirtualSize = section_table.SizeOfRawData;

			section_tables.Add(section_table);
			sections.Add(section);

			// Move to the next section table
			file_position += PeSectionTable.Size;
		}

		ImportSymbolsAndRelocations(header, sections, section_tables, bytes);

		Marshal.FreeHGlobal(bytes);
		return new BinaryObjectFile(sections);
	}
}

[StructLayout(LayoutKind.Sequential)]
public class PeHeader
{
	public const int Size = 136;
	public const int OptionalHeaderOffset = 0x18;

	public int Signature { get; set; } = PeFormat.SIGNATURE;
	public ushort Machine { get; set; }
	public short NumberOfSections { get; set; }
	public uint TimeDateStamp { get; set; }
	public uint PointerToSymbolTable { get; set; } = 0;
	public int NumberOfSymbols { get; set; } = 0;
	public short SizeOfOptionalHeader { get; set; }
	public short Characteristics { get; set; }

	public short Magic { get; set; } = 0x20B;
	public byte MajorLinkerVersion { get; set; } = 2;
	public byte MinorLinkerVersion { get; set; } = 30;
	public int SizeOfCode { get; set; }
	public int SizeOfInitializedData { get; set; }
	public int SizeOfUninitializedData { get; set; }
	public int AddressOfEntryPoint { get; set; }
	public int BaseOfCode { get; set; }

	public long ImageBase { get; set; }
	public int SectionAlignment { get; set; }
	public int FileAlignment { get; set; }
	public short MajorOperatingSystemVersion { get; set; } = 4;
	public short MinorOperatingSystemVersion { get; set; } = 0;
	public short MajorImageSystemVersion { get; set; } = 0;
	public short MinorImageSystemVersion { get; set; } = 0;
	public short MajorSubsystemSystemVersion { get; set; } = 5;
	public short MinorSubsystemSystemVersion { get; set; } = 2;
	private int Win32VersionValue { get; set; } = 0;
	public int SizeOfImage { get; set; }
	public int SizeOfHeaders { get; set; }
	public int CheckSum { get; set; } = 0;
	public short Subsystem { get; set; } = 3;
	public short DllCharacteristics { get; set; } = 0;
	public long SizeOfStackReserve { get; set; } = 0x200000;
	public long SizeOfStackCommit { get; set; } = 0x1000;
	public long SizeOfHeapReserve { get; set; } = 0x100000;
	public long SizeOfHeapCommit { get; set; } = 0x1000;
	public int LoaderFlags { get; set; } = 0;
	public int NumberOfDataDirectories { get; set; }
}

[StructLayout(LayoutKind.Sequential)]
public class PeObjectFileHeader
{
	public const int Size = 0x14;

	public ushort Machine { get; set; }
	public short NumberOfSections { get; set; }
	public uint TimeDateStamp { get; set; }
	public uint PointerToSymbolTable { get; set; } = 0;
	public int NumberOfSymbols { get; set; } = 0;
	public short SizeOfOptionalHeader { get; set; }
	public short Characteristics { get; set; }
}

[StructLayout(LayoutKind.Sequential)]
public class PeDataDirectory
{
	public const int Size = 8;

	public int RelativeVirtualAddress { get; set; } = 0;
	public int PhysicalSize { get; set; } = 0;

	public PeDataDirectory() { }

	public PeDataDirectory(int relative_virtual_address, int physical_size)
	{
		RelativeVirtualAddress = relative_virtual_address;
		PhysicalSize = physical_size;
	}
}

[StructLayout(LayoutKind.Sequential)]
public class PeSectionTable
{
	public const int Size = 40;

	public ulong Name { get; set; }
	public int VirtualSize { get; set; }
	public uint VirtualAddress { get; set; }
	public int SizeOfRawData { get; set; }
	public uint PointerToRawData { get; set; }
	public uint PointerToRelocations { get; set; }
	public int PointerToLinenumbers { get; set; }
	public ushort NumberOfRelocations { get; set; }
	public short NumberOfLinenumbers { get; set; }
	public uint Characteristics { get; set; }
}

[StructLayout(LayoutKind.Sequential)]
public class PeMetadata
{
	public byte[] Bytes { get; set; }
	public PeHeader Header { get; set; }
	public PeDataDirectory[] DataDirectories { get; set; }
	public PeSectionTable[] Sections { get; set; }

	public PeMetadata(byte[] bytes, PeHeader header, PeDataDirectory[] data_directories, PeSectionTable[] sections)
	{
		Bytes = bytes;
		Header = header;
		DataDirectories = data_directories;
		Sections = sections;
	}
}

[StructLayout(LayoutKind.Sequential)]
public class PeExportDirectoryTable
{
	private int ExportFlags { get; set; } = 0;
	public int TimeDateStamp { get; set; }
	public short MajorVersion { get; set; }
	public short MinorVersion { get; set; }
	public int NameRelativeVirtualAddress { get; set; }
	public int OrdinalBase { get; set; }
	public int AddressTableEntries { get; set; }
	public int NumberOfNamePointers { get; set; }
	public int ExportAddressTableRelativeVirtualAddress { get; set; }
	public int NamePointerRelativeVirtualAddress { get; set; }
	public int OrdinalTableRelativeVirtualAddress { get; set; }
}