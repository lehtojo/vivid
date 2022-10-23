using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System;

public class StaticLibraryFormatFile
{
	public string Name { get; set; }
	public uint Position { get; set; } = 0;
	public string[] Symbols { get; }
	private byte[]? Bytes { get; set; } = null;

	public StaticLibraryFormatFile(string name, string[] symbols)
	{
		Name = name;
		Symbols = symbols;
	}

	public StaticLibraryFormatFile(string name, string[] symbols, byte[] bytes)
	{
		Name = name;
		Symbols = symbols;
		Bytes = bytes;
	}

	public void Load()
	{
		if (Bytes != null) return;
		Bytes = File.ReadAllBytes(Name);
	}

	public byte[] GetBytes()
	{
		if (Bytes != null) return Bytes;
		Bytes = File.ReadAllBytes(Name);
		return Bytes;
	}
}

public class StaticLibraryFormatFileHeader
{
	public string Filename { get; set; }
	public int Size { get; set; }
	public int PointerOfData { get; set; }

	public StaticLibraryFormatFileHeader(string filename, int size, int pointer_of_data)
	{
		Filename = filename;
		Size = size;
		PointerOfData = pointer_of_data;
	}
}

public static class StaticLibraryFormat
{
	public const string SIGNATURE = "!<arch>\n";
	public const string EXPORT_TABLE_FILENAME = "/";
	public const string FILENAME_TABLE_NAME = "//";
	public const string EXPORT_TABLE_FILEMODE = "0";
	public const string DEFAULT_FILEMODE = "100666";
	public const string END_COMMAND = "\x60\n";

	public const byte FILE_HEADER_LENGTH = FILENAME_LENGTH + TIMESTAMP_LENGTH + IDENTITY_LENGTH * 2 + FILEMODE_LENGTH + SIZE_LENGTH + 2;

	public const int FILENAME_LENGTH = 16;
	public const int TIMESTAMP_LENGTH = 12;
	public const int IDENTITY_LENGTH = 6;
	public const byte FILEMODE_LENGTH = 8;
	public const byte SIZE_LENGTH = 10;

	public const byte PADDING_VALUE = 0x20;

	public static readonly DateTime TIMESTAMP_START = new(1970, 1, 1);

	private static void WritePadding(MemoryStream builder, int length)
	{
		if (length <= 0) return;

		for (var i = 0; i < length; i++)
		{
			builder.WriteByte(PADDING_VALUE);
		}
	}

	private static void WriteFileHeader(MemoryStream builder, string filename, uint timestamp, uint size, string filemode = DEFAULT_FILEMODE)
	{
		// Write the filename
		var bytes = Encoding.UTF8.GetBytes(filename);
		builder.Write(bytes, 0, Math.Min(FILENAME_LENGTH, bytes.Length));
		WritePadding(builder, FILENAME_LENGTH - bytes.Length);
		
		// Write the timestamp
		bytes = Encoding.UTF8.GetBytes(timestamp.ToString());
		builder.Write(bytes, 0, Math.Min(TIMESTAMP_LENGTH, bytes.Length));
		WritePadding(builder, TIMESTAMP_LENGTH - bytes.Length);
		
		// Identities are not supported
		builder.WriteByte((byte)'0');
		WritePadding(builder, IDENTITY_LENGTH - 1);
		builder.WriteByte((byte)'0');
		WritePadding(builder, IDENTITY_LENGTH - 1);

		// Write the file mode
		bytes = Encoding.UTF8.GetBytes(filemode);
		builder.Write(bytes, 0, Math.Min(FILEMODE_LENGTH, bytes.Length));
		WritePadding(builder, FILEMODE_LENGTH - bytes.Length);

		// Write the size of the file
		bytes = Encoding.UTF8.GetBytes(size.ToString());
		builder.Write(bytes, 0, Math.Min(SIZE_LENGTH, bytes.Length));
		WritePadding(builder, SIZE_LENGTH - bytes.Length);

		// End the header
		bytes = Encoding.UTF8.GetBytes(END_COMMAND);
		builder.Write(bytes, 0, bytes.Length);
	}

	private static MemoryStream WriteSymbols(StaticLibraryFormatFile[] files)
	{
		var builder = new MemoryStream();
		WriteSymbols(builder, files.SelectMany(i => i.Symbols).ToArray());
		return builder;
	}

	private static int[] WriteSymbols(MemoryStream builder, string[] symbols)
	{
		var indices = new int[symbols.Length];

		for (var i = 0; i < symbols.Length; i++)
		{
			indices[i] = (int)builder.Length;

			builder.Write(Encoding.UTF8.GetBytes(symbols[i]));
			builder.WriteByte(0);
		}

		// Align to 2 bytes if necessary
		if (builder.Length % 2 != 0) builder.WriteByte(0);

		return indices;
	}

	private static MemoryStream CreateFilenameTable(StaticLibraryFormatFile[] files, uint timestamp)
	{
		using var filenames = new MemoryStream();
		var indices = WriteSymbols(filenames, files.Select(i => i.Name).ToArray());

		files.Zip(indices).ForEach(i => i.First.Name = '/' + i.Second.ToString());

		var builder = new MemoryStream();
		WriteFileHeader(builder, FILENAME_TABLE_NAME, timestamp, (uint)filenames.Length);

		filenames.WriteTo(builder);

		return builder;
	}

	private static uint SwapEndianness(uint value)
	{
		var a = (value >> 0) & 0xFF;
		var b = (value >> 8) & 0xFF;
		var c = (value >> 16) & 0xFF;
		var d = (value >> 24) & 0xFF;

		return a << 24 | b << 16 | c << 8 | d << 0;
	} 

	public static void Export(StaticLibraryFormatFile[] files, string output)
	{
		files.ForEach(i => i.Load());

		using var contents = new MemoryStream();

		var timestamp = (uint)(DateTime.UtcNow.Subtract(TIMESTAMP_START)).TotalSeconds;
		var position = (uint)0;

		using var filename_table = CreateFilenameTable(files, timestamp);

		foreach (var file in files)
		{
			file.Position = position;

			var bytes = file.GetBytes();

			WriteFileHeader(contents, file.Name, timestamp, (uint)bytes.Length);

			contents.Write(bytes);

			position += FILE_HEADER_LENGTH + (uint)bytes.Length;

			// Align the position to 2 bytes
			if (position % 2 == 0) continue;

			contents.WriteByte(0);
			position++;
		}

		using var builder = new MemoryStream();
		builder.Write(Encoding.UTF8.GetBytes(SIGNATURE));

		using var symbol_buffer = WriteSymbols(files);

		var symbol_count = (uint)files.Sum(i => i.Symbols.Length);
		var export_table_size = sizeof(int) + symbol_count * sizeof(int) + symbol_buffer.Length;

		// Compute the offset which must be applied to all the file positions
		var offset = SIGNATURE.Length + FILE_HEADER_LENGTH + export_table_size + filename_table.Length;
		files.ForEach(i => i.Position += (uint)offset);

		// Write the export table file header
		WriteFileHeader(builder, EXPORT_TABLE_FILENAME, timestamp, (uint)export_table_size, EXPORT_TABLE_FILEMODE);

		builder.Write(BitConverter.GetBytes(SwapEndianness(symbol_count)));

		// Write all the file pointers
		foreach (var file in files)
		{
			var bytes = BitConverter.GetBytes(SwapEndianness(file.Position));

			for (var i = 0; i < file.Symbols.Length; i++)
			{
				builder.Write(bytes);
			}
		}

		// Append the symbol buffer
		symbol_buffer.WriteTo(builder);

		// Append the filename table
		filename_table.WriteTo(builder);

		// Finally append the contents
		contents.WriteTo(builder);

		File.WriteAllBytes(output + AssemblyPhase.StaticLibraryExtension, builder.GetBuffer());
	}

	public static Status Export(Context context, string output_name, Dictionary<SourceFile, List<string>> exports)
	{
		try
		{
			var exported_source_files = new List<StaticLibraryFormatFile>();

			exported_source_files.Add(new StaticLibraryFormatFile(
				output_name + ".exports.v",
				Array.Empty<string>(),
				Encoding.UTF8.GetBytes(ObjectExporter.ExportContext(context))
			));

			exported_source_files.Add(new StaticLibraryFormatFile(
				output_name + ".types.templates",
				Array.Empty<string>(),
				Encoding.UTF8.GetBytes(ObjectExporter.ExportTemplateTypeVariants(context))
			));

			exported_source_files.Add(new StaticLibraryFormatFile(
				output_name + ".functions.templates",
				Array.Empty<string>(),
				Encoding.UTF8.GetBytes(ObjectExporter.ExportTemplateFunctionVariants(context))
			));

			var exported_symbols = ObjectExporter
				.GetExportedSymbols(context)
				.Merge(exports)
				.Select(i => new StaticLibraryFormatFile(
					AssemblyPhase.GetObjectFileName(i.Key, output_name),
					i.Value.ToArray()
				));

			StaticLibraryFormat.Export(exported_symbols.Concat(exported_source_files).ToArray(), output_name);
		}
		catch (Exception e)
		{
			return Status.Error(e.Message);
		}

		return Status.OK;
	}

	public static Status Export(Context context, Dictionary<SourceFile, List<string>> exports, Dictionary<SourceFile, BinaryObjectFile> object_files, string output_name)
	{
		try
		{
			var exported_source_files = new List<StaticLibraryFormatFile>();

			exported_source_files.Add(new StaticLibraryFormatFile(
				output_name + ".exports.v",
				Array.Empty<string>(),
				Encoding.UTF8.GetBytes(ObjectExporter.ExportContext(context))
			));

			exported_source_files.Add(new StaticLibraryFormatFile(
				output_name + ".types.templates",
				Array.Empty<string>(),
				Encoding.UTF8.GetBytes(ObjectExporter.ExportTemplateTypeVariants(context))
			));

			exported_source_files.Add(new StaticLibraryFormatFile(
				output_name + ".functions.templates",
				Array.Empty<string>(),
				Encoding.UTF8.GetBytes(ObjectExporter.ExportTemplateFunctionVariants(context))
			));

			var exported_symbols = new List<StaticLibraryFormatFile>();

			foreach (var iterator in object_files)
			{
				var source_file = iterator.Key;
				var object_file = iterator.Value;

				var object_file_name = AssemblyPhase.GetObjectFileName(source_file, output_name);
				var object_file_symbols = object_file.Exports.ToArray();

				var bytes = Settings.IsTargetWindows
					? PeFormat.Build(object_file.Sections, object_file.Exports)
					: ElfFormat.Build(object_file.Sections, object_file.Exports);

				exported_symbols.Add(new StaticLibraryFormatFile(object_file_name, object_file_symbols, bytes));
			}

			StaticLibraryFormat.Export(exported_symbols.Concat(exported_source_files).ToArray(), output_name);
		}
		catch (Exception e)
		{
			return Status.Error(e.Message);
		}

		return Status.OK;
	}
}