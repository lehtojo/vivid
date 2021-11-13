using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

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
	DLL = 0x2000,
}

public static class PortableExecutableFormat
{
	public const int FileSectionAlignment = 0x200;
	public const int VirtualSectionAlignment = 0x1000;

	public const int SIGNATURE = 0x00004550; // 'PE\0\0'
	public const string RELOCATION_TABLE_SECTION_PREFIX = ".r";

	public const int HEADER_ADDRESS_OFFSET = 0x3C;
	public const int HEADER_START_SIZE = 24;
	public const string EXPORT_TABLE_NAME = ".edata\0\0";

	public static int GetHeaderOffset(byte[] bytes)
	{
		return BitConverter.ToInt32(bytes, HEADER_ADDRESS_OFFSET);
	}

	public static T ToStructure<T>(byte[] bytes, int offset)
	{
		var length = Marshal.SizeOf<T>();

		var buffer = Marshal.AllocHGlobal(Marshal.SizeOf<T>());
		Marshal.Copy(bytes, offset, buffer, length);

		var result = Marshal.PtrToStructure<T>(buffer) ?? throw new ApplicationException("Could not convert byte array to a structure");

		Marshal.FreeHGlobal(buffer);

		return result;
	}

	public static bool IsEndOfDataDirectories(byte[] bytes, int position)
	{
		return BitConverter.ToInt64(bytes, position) == 0;
	}

	public static PortableExecutableFormatDataDirectory[]? LoadDataDirectories(byte[] bytes, int start, int count)
	{
		if (count == 0)
		{
			return Array.Empty<PortableExecutableFormatDataDirectory>();
		}

		var directories = new List<PortableExecutableFormatDataDirectory>();
		var length = Marshal.SizeOf<PortableExecutableFormatDataDirectory>();
		var end = false;

		while (start + length <= bytes.Length)
		{
			if (count-- <= 0)
			{
				end = true;
				break;
			}

			directories.Add(ToStructure<PortableExecutableFormatDataDirectory>(bytes, start));
			start += length;
		}

		return end ? directories.ToArray() : null;
	}

	public static PortableExecutableFormatSection[]? LoadSections(byte[] bytes, int start, int count)
	{
		if (count == 0)
		{
			return Array.Empty<PortableExecutableFormatSection>();
		}

		var directories = new List<PortableExecutableFormatSection>();
		var length = Marshal.SizeOf<PortableExecutableFormatSection>();
		var end = false;

		while (start + length <= bytes.Length)
		{
			if (count-- <= 0)
			{
				end = true;
				break;
			}

			directories.Add(ToStructure<PortableExecutableFormatSection>(bytes, start));
			start += length;
		}

		return end ? directories.ToArray() : null;
	}

	private static PortableExecutableFormatModule? ImportFile(string file)
	{
		var bytes = File.ReadAllBytes(file);
		var header_offset = PortableExecutableFormat.GetHeaderOffset(bytes);

		if (header_offset < 0 || header_offset + Marshal.SizeOf<PortableExecutableFormatHeader>() > bytes.Length)
		{
			return null;
		}

		var header = ToStructure<PortableExecutableFormatHeader>(bytes, header_offset);

		var data_directories_offset = header_offset + 112;
		var data_directories = LoadDataDirectories(bytes, data_directories_offset, header.NumberOfDataDirectories);

		if (data_directories == null || header.NumberOfSections < 0)
		{
			return null;
		}

		var section_table_offset = header_offset + HEADER_START_SIZE + header.SizeOfOptionalHeader;
		var sections = LoadSections(bytes, section_table_offset, header.NumberOfSections);

		if (sections == null)
		{
			return null;
		}

		return new PortableExecutableFormatModule(bytes, header, data_directories, sections);
	}

	public static PortableExecutableFormatModule? ImportMetadata(string file)
	{
		try
		{
			return ImportFile(file);
		}
		catch
		{
			return null;
		}
	}

	public static PortableExecutableFormatSection? FindSection(PortableExecutableFormatModule module, string name)
	{
		var bytes = Encoding.UTF8.GetBytes(name);

		if (bytes.Length != 8)
		{
			throw new ArgumentException("Section name must be eight characters long. If the actual section name is shorter, it must be padded with null characters");
		}

		return module.Sections.First(i => i.Name == BitConverter.ToUInt64(bytes));
	}

	public static PortableExecutableFormatSection? FindExportSection(PortableExecutableFormatModule module)
	{
		return FindSection(module, EXPORT_TABLE_NAME);
	}

	/// <summary>
	/// Loads strings the specified amount starting from the specified index.
	/// </summary>
	public static string[]? LoadStrings(byte[] bytes, int position, int count)
	{
		if (position < 0)
		{
			return null;
		}

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

	public static string[]? LoadExportedSymbols(PortableExecutableFormatModule module, PortableExecutableFormatSection export_section)
	{
		var export_directory_table = ToStructure<PortableExecutableFormatExportDirectoryTable>(module.Bytes, (int)export_section.PointerToRawData);

		// Skip the export directory table, the export address table, the name pointer table and the ordinal table
		var export_directory_table_size = Marshal.SizeOf<PortableExecutableFormatExportDirectoryTable>();
		var export_address_table_size = export_directory_table.AddressTableEntries * sizeof(int);
		var name_pointer_table_size = export_directory_table.NumberOfNamePointers * sizeof(int);
		var ordinal_table_size = export_directory_table.NumberOfNamePointers * sizeof(short);

		var start = export_section.PointerToRawData + export_directory_table_size + export_address_table_size + name_pointer_table_size + ordinal_table_size;

		// Load one string more since the first name is the name of the module and it is not counted
		var strings = LoadStrings(module.Bytes, (int)start, export_directory_table.NumberOfNamePointers + 1);

		// Skip the name of the module if the load was successful
		return strings?[1..];
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
	public static BinaryStringTable CreateSymbolRelatedSections(List<BinarySection> sections, List<BinarySection>? fragments, Dictionary<string, BinarySymbol> symbols)
	{
		var symbol_name_table = new BinaryStringTable(true);
		var symbol_entries = new List<PeSymbolEntry>();

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

		foreach (var symbol in symbols.Values)
		{
			var virtual_address = symbol.Section == null ? 0 : symbol.Section.VirtualAddress;

			var symbol_entry = new PeSymbolEntry();
			symbol_entry.Value = (uint)(virtual_address + symbol.Offset);
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

			var relocation_table_name = RELOCATION_TABLE_SECTION_PREFIX + section.Name[1..];
			var relocation_table_section = new BinarySection(relocation_table_name, BinarySectionType.RELOCATION_TABLE, new byte[PeRelocationEntry.Size * relocation_entries.Count]);
			ElfFormat.Write(relocation_table_section.Data, 0, relocation_entries);
			sections.Add(relocation_table_section);
		}

		// Export the data from the generated string table, since it has to come directly after the symbol table
		var symbol_name_table_data = symbol_name_table.Export();

		var symbol_table_section = new BinarySection(ElfFormat.SYMBOL_TABLE_SECTION, BinarySectionType.SYMBOL_TABLE, new byte[PeSymbolEntry.Size * symbol_entries.Count + symbol_name_table_data.Length]);
		ElfFormat.Write(symbol_table_section.Data, 0, symbol_entries);
		Array.Copy(symbol_name_table_data, 0, symbol_table_section.Data, symbol_entries.Count * PeSymbolEntry.Size, symbol_name_table_data.Length);
		sections.Add(symbol_table_section);

		return symbol_name_table;
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
		CreateSymbolRelatedSections(sections, null, symbols);

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

		// Add symbols and relocations of each section needing that
		CreateSymbolRelatedSections(sections, null, symbols);

		var header = new PeObjectFileHeader
		{
			NumberOfSections = (short)sections.Count,
			Machine = (ushort)PeMachineType.X64,
			TimeDateStamp = (uint)DateTime.Now.ToFileTimeUtc(),
			Characteristics = (short)(PeFormatImageCharacteristics.LARGE_ADDRESS_AWARE | PeFormatImageCharacteristics.LINENUMBERS_STRIPPED),
		};

		if (!sections.Exists(i => i.Type == BinarySectionType.RELOCATION_TABLE))
		{
			header.Characteristics |= (short)PeFormatImageCharacteristics.RELOCATIONS_STRIPPED;
		}

		// Create initial versions of section tables and finish them later when section offsets are known
		var section_tables = new List<PortableExecutableFormatSection>();

		foreach (var section in sections)
		{
			if (section.Name.Length > 8) throw new Exception("Section name is too long");

			var bytes = Encoding.UTF8.GetBytes(section.Name).Concat(new byte[8 - section.Name.Length]).ToArray();

			var section_table = new PortableExecutableFormatSection
			{
				Name = BitConverter.ToUInt64(bytes),
				VirtualAddress = (uint)section.VirtualAddress,
				SizeOfRawData = section.Data.Length,
				PointerToRawData = 0, // Fill in later when the section offsets are decided
				PointerToRelocations = 0, // Fill in later when the section offsets are decided
				PointerToLinenumbers = 0, // Not used
				NumberOfRelocations = 0, // Fill in later
				NumberOfLinenumbers = 0, // Not used
				Characteristics = GetSectionCharacteristics(section),
			};

			section_tables.Add(section_table);
		}

		header.SizeOfOptionalHeader = 0;

		// Decide section offsets
		var file_position = PeObjectFileHeader.Size + section_tables.Count * PortableExecutableFormatSection.Size;

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
			header.NumberOfSymbols = symbol_table.Data.Length / PeSymbolEntry.Size;
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

	/// <summary>
	/// Creates an object file from the specified sections
	/// </summary>
	public static byte[] Link(List<BinaryObjectFile> objects, string entry, bool executable)
	{
		#warning Some of the functions inside the ELF-format are very general and should be moved to a separate class
		#warning Add support for .xdata section
		#warning Sections are aligned incorrectly, since virtual addresses use file alignment instead of proper alignments

		// Index all the specified object files
		for (var i = 0; i < objects.Count; i++) { objects[i].Index = i; }

		// Make all hidden symbols unique by using their object file indices
		Linker.MakeLocalSymbolsUnique(objects);

		var header = new PortableExecutableFormatHeader
		{
			Machine = (ushort)PeMachineType.X64,
			TimeDateStamp = (uint)DateTime.Now.ToFileTimeUtc(),
			SectionAlignment = VirtualSectionAlignment,
			Characteristics = (short)(PeFormatImageCharacteristics.LARGE_ADDRESS_AWARE | PeFormatImageCharacteristics.LINENUMBERS_STRIPPED),
		};

		// Resolves are unresolved symbols and returns all symbols as a list
		var symbols = Linker.ResolveSymbols(objects);

		// Ensure sections are ordered so that sections of same type are next to each other
		var fragments = objects.SelectMany(i => i.Sections).Where(Linker.IsLoadableSection).ToList();

		// Load all the relocations from all the sections
		var relocations = objects.SelectMany(i => i.Sections).SelectMany(i => i.Relocations).ToList();

		// Create sections, which cover the fragmented sections
		var overlays = Linker.CreateLoadableSections(fragments);

		// Store the size of the code
		var text_section = overlays.Find(i => i.Type == BinarySectionType.TEXT);
		if (text_section != null) { header.SizeOfCode = (int)text_section.VirtualSize; }

		// Store the size of the data
		var data_section = overlays.Find(i => i.Name == ElfFormat.DATA_SECTION);
		if (data_section != null) { header.SizeOfInitializedData = (int)data_section.VirtualSize; }

		if (!overlays.Exists(i => i.Type == BinarySectionType.RELOCATION_TABLE))
		{
			header.Characteristics |= (short)PeFormatImageCharacteristics.RELOCATIONS_STRIPPED;
		}

		var data_directories = new List<PortableExecutableFormatDataDirectory>();
		// Add data directories here...

		// TODO: Filter out relocations that must be imported from DLLs

		// Add a none-entry to the data directory
		data_directories.Add(new PortableExecutableFormatDataDirectory());

		// Store the amount of proper data directories
		header.NumberOfDataDirectories = data_directories.Count - 1;

		// Create initial versions of section tables and finish them later when section offsets are known
		var section_tables = new List<PortableExecutableFormatSection>();

		foreach (var section in overlays)
		{
			if (section.Name.Length > 8) throw new Exception("Section name is too long");

			var bytes = Encoding.UTF8.GetBytes(section.Name).Concat(new byte[8 - section.Name.Length]).ToArray();

			var section_table = new PortableExecutableFormatSection
			{
				Name = BitConverter.ToUInt64(bytes),
				VirtualAddress = (uint)section.VirtualAddress,
				SizeOfRawData = section.Data.Length,
				PointerToRawData = 0, // Fill in later when the section offsets are decided
				PointerToRelocations = 0, // Fill in later when the section offsets are decided
				PointerToLinenumbers = 0, // Not used
				NumberOfRelocations = 0, // Fill in later
				NumberOfLinenumbers = 0, // Not used
				Characteristics = GetSectionCharacteristics(section),
			};

			section_tables.Add(section_table);
		}

		header.SizeOfOptionalHeader = (short)(PortableExecutableFormatHeader.Size - PortableExecutableFormatHeader.OptionalHeaderOffset +
			PortableExecutableFormatDataDirectory.Size * data_directories.Count);

		// Decide section offsets and virtual addresses
		var file_position = PortableExecutableFormatHeader.Size +
			PortableExecutableFormatDataDirectory.Size * data_directories.Count +
			PortableExecutableFormatSection.Size * section_tables.Count;

		var virtual_address = executable ? (int)Linker.VIRTUAL_ADDRESS_START : 0;

		foreach (var section in overlays)
		{
			// Determine the correct section alignment
			// The minimum alignment is 512 bytes, but it can be larger, if the section requires that
			var alignment = Math.Max(section.Alignment, FileSectionAlignment);
			var margin = alignment - (file_position % alignment);

			if (margin != alignment)
			{
				section.Margin = margin;
				virtual_address += margin;
				file_position += margin;
			}

			section.Offset = file_position;
			section.VirtualAddress = virtual_address;

			// Align all the section fragments
			foreach (var fragment in fragments.FindAll(i => i.Name == section.Name))
			{
				// Apply the fragment margin before doing anything, so that the fragment is aligned
				file_position += fragment.Margin;
				virtual_address += fragment.Margin;

				fragment.Offset = file_position;
				fragment.VirtualAddress = (int)virtual_address;

				file_position += fragment.Data.Length;
				virtual_address += fragment.Data.Length;
			}
		}

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

		// Add symbols and relocations of each section needing that
		CreateSymbolRelatedSections(overlays, null, symbols);

		// Compute the entry point location
		var entry_point_symbol = symbols[entry];
		header.AddressOfEntryPoint = entry_point_symbol.Section!.VirtualAddress + entry_point_symbol.Offset;

		// Store the number of sections to the header
		header.NumberOfSections = (short)overlays.Count;

		// Store the location of the symbol table
		var symbol_table = overlays.Find(i => i.Name == ElfFormat.SYMBOL_TABLE_SECTION);
		if (symbol_table != null) { header.PointerToSymbolTable = (uint)symbol_table.Offset; }

		// Create the binary file
		var binary = new byte[file_position];

		// Write the file header
		ElfFormat.Write(binary, 0, header);

		// Write the data directories
		ElfFormat.Write(binary, PortableExecutableFormatHeader.Size, data_directories);

		// Write the section tables
		ElfFormat.Write(binary, PortableExecutableFormatHeader.Size + PortableExecutableFormatDataDirectory.Size * data_directories.Count, section_tables);

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
	public static void ImportSymbolsAndRelocations(PeObjectFileHeader header, List<BinarySection> sections, List<PortableExecutableFormatSection> section_tables, IntPtr bytes)
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

		// Now, import the relocations
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
				var relocation = new BinaryRelocation(symbol, (int)relocation_entry.VirtualAddress, 0, relocation_type);
				relocation.Section = section;

				section.Relocations.Add(relocation);
				file_position += PeRelocationEntry.Size;
			}
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
		var section_tables = new List<PortableExecutableFormatSection>();

		for (var i = 0; i < header.NumberOfSections; i++)
		{
			// Load the section table in order to load the actual section
			var section_table = Marshal.PtrToStructure<PortableExecutableFormatSection>(file_position) ?? throw new ApplicationException("Could not load a section header");

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
			file_position += PortableExecutableFormatSection.Size;
		}

		ImportSymbolsAndRelocations(header, sections, section_tables, bytes);

		Marshal.FreeHGlobal(bytes);
		return new BinaryObjectFile(sections);
	}
}

[StructLayout(LayoutKind.Sequential)]
public class PortableExecutableFormatHeader
{
	public const int Size = 0x78;
	public const int OptionalHeaderOffset = 0x18;

	public int Signature { get; set; } = PortableExecutableFormat.SIGNATURE;
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
public class PortableExecutableFormatDataDirectory
{
	public const int Size = 8;

	public int RelativeVirtualAddress { get; set; } = 0;
	public int PhysicalSize { get; set; } = 0;
}

[StructLayout(LayoutKind.Sequential)]
public class PortableExecutableFormatSection
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
public class PortableExecutableFormatModule
{
	public byte[] Bytes { get; set; }
	public PortableExecutableFormatHeader Header { get; set; }
	public PortableExecutableFormatDataDirectory[] DataDirectories { get; set; }
	public PortableExecutableFormatSection[] Sections { get; set; }

	public PortableExecutableFormatModule(byte[] bytes, PortableExecutableFormatHeader header, PortableExecutableFormatDataDirectory[] data_directories, PortableExecutableFormatSection[] sections)
	{
		Bytes = bytes;
		Header = header;
		DataDirectories = data_directories;
		Sections = sections;
	}
}

[StructLayout(LayoutKind.Sequential)]
public class PortableExecutableFormatExportDirectoryTable
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