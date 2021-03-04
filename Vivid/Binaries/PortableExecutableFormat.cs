using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

public static class PortableExecutableFormat
{
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
		var data_directories = LoadDataDirectories(bytes, data_directories_offset, header.NumberOfRvaAndSizes);

		if (data_directories == null || header.NumberOfSection < 0)
		{
			return null;
		}

		var section_table_offset = header_offset + HEADER_START_SIZE + header.SizeOfOptionalHeader;
		var sections = LoadSections(bytes, section_table_offset, header.NumberOfSection);

		if (sections == null)
		{
			return null;
		}

		return new PortableExecutableFormatModule(bytes, header, data_directories, sections);
	}

	public static PortableExecutableFormatModule? Import(string file)
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

		return module.Sections.First(i => i.Name == BitConverter.ToInt64(bytes));
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
		var export_directory_table = ToStructure<PortableExecutableFormatExportDirectoryTable>(module.Bytes, export_section.PointerToRawData);

		// Skip the export directory table, the export address table, the name pointer table and the ordinal table
		var export_directory_table_size = Marshal.SizeOf<PortableExecutableFormatExportDirectoryTable>();
		var export_address_table_size = export_directory_table.AddressTableEntries * sizeof(int);
		var name_pointer_table_size = export_directory_table.NumberOfNamePointers * sizeof(int);
		var ordinal_table_size = export_directory_table.NumberOfNamePointers * sizeof(short);

		var start = export_section.PointerToRawData + export_directory_table_size + export_address_table_size + name_pointer_table_size + ordinal_table_size;

		// Load one string more since the first name is the name of the module and it is not counted
		var strings = LoadStrings(module.Bytes, start, export_directory_table.NumberOfNamePointers + 1);

		// Skip the name of the module if the load was successful
		return strings?[1..];
	}
}

[StructLayout(LayoutKind.Sequential)]
public class PortableExecutableFormatHeader
{
	public int Signature { get; set; }
	public short Machine { get; set; }
	public short NumberOfSection { get; set; }
	public int TimeDateStamp { get; set; }
	private int PointerToSymbolTable { get; set; } = 0;
	private int NumberOfSymbolTable { get; set; } = 0;
	public short SizeOfOptionalHeader { get; set; }
	public short Charactericstics { get; set; }

	public short Magic { get; set; }
	public byte MajorLinkerVersion { get; set; }
	public byte MinorLinkerVersion { get; set; }
	public int SizeOfCode { get; set; }
	public int SizeOfInitializedData { get; set; }
	public int SizeOfUninitializedData { get; set; }
	public int AddressOfEntryPoint { get; set; }
	public int BaseOfCode { get; set; }

	public long ImageBase { get; set; }
	public int SectionAlignment { get; set; }
	public int FileAlignment { get; set; }
	public short MajorOperatingSystemVersion { get; set; }
	public short MinorOperatingSystemVersion { get; set; }
	public short MajorImageSystemVersion { get; set; }
	public short MinorImageSystemVersion { get; set; }
	public short MajorSubsystemSystemVersion { get; set; }
	public short MinorSubsystemSystemVersion { get; set; }
	private int Win32VersionValue { get; set; } = 0;
	public int SizeOfImage { get; set; }
	public int SizeOfHeaders { get; set; }
	public int CheckSum { get; set; }
	public short Subsystem { get; set; }
	public short DllCharacteristics { get; set; }
	public long SizeOfStackReserve { get; set; }
	public long SizeOfStackCommit { get; set; }
	public long SizeOfHeapReserve { get; set; }
	public long SizeOfHeapCommit { get; set; }
	private int LoaderFlags { get; set; } = 0;
	public int NumberOfRvaAndSizes { get; set; }
}

[StructLayout(LayoutKind.Sequential)]
public class PortableExecutableFormatDataDirectory
{
	public int RelativeVirtualAddress { get; set; }
	public int Size { get; set; }
}

[StructLayout(LayoutKind.Sequential)]
public class PortableExecutableFormatSection
{
	public long Name { get; set; }
	public int VirtualSize { get; set; }
	public int VirtualAddress { get; set; }
	public int SizeOfRawData { get; set; }
	public int PointerToRawData { get; set; }
	public int PointerToRelocations { get; set; }
	public int PointerToLinenumbers { get; set; }
	public short NumberOfRelocations { get; set; }
	public short NumberOfLinenumbers { get; set; }
	public int Charactericstics { get; set; }
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