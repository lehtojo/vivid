using System.Globalization;
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
	private byte[] Bytes { get; set; } = Array.Empty<byte>();

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
		if (Bytes.Length > 0)
		{
			return;
		}

		Bytes = File.ReadAllBytes(Name);
	}

	public byte[] GetBytes()
	{
		if (Bytes.Length > 0)
		{
			return Bytes;
		}

		Bytes = File.ReadAllBytes(Name);
		return Bytes;
	}
}

public class StaticLibraryFormatFileHeader
{
	public string Filename { get; set; }
	public int Size { get; set; }
	public int Data { get; set; }

	public StaticLibraryFormatFileHeader(string filename, int size, int data)
	{
		Filename = filename;
		Size = size;
		Data = data;
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

	public const byte FILEHEADER_LENGTH = FILENAME_LENGTH + TIMESTAMP_LENGTH + IDENTITY_LENGTH * 2 + FILEMODE_LENGTH + SIZE_LENGTH + 2;

	public const int FILENAME_LENGTH = 16;
	public const int TIMESTAMP_LENGTH = 12;
	public const int IDENTITY_LENGTH = 6;
	public const byte FILEMODE_LENGTH = 8;
	public const byte SIZE_LENGTH = 10;

	public const byte PADDING_VALUE = 0x20;

	public static readonly DateTime TIMESTAMP_START = new(1970, 1, 1);

	private static void WritePadding(MemoryStream builder, int length)
	{
		if (length <= 0)
		{
			return;
		}

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

	private static MemoryStream AppendSymbols(StaticLibraryFormatFile[] files)
	{
		var builder = new MemoryStream();
		AppendSymbols(builder, files.SelectMany(i => i.Symbols).ToArray());
		return builder;
	}

	private static int[] AppendSymbols(MemoryStream builder, string[] symbols)
	{
		var indices = new int[symbols.Length];

		for (var i = 0; i < symbols.Length; i++)
		{
			indices[i] = (int)builder.Length;

			builder.Write(Encoding.UTF8.GetBytes(symbols[i]));
			builder.WriteByte(0);
		}

		builder.WriteByte(0);

		return indices;
	}

	private static MemoryStream CreateFilenameTable(StaticLibraryFormatFile[] files, uint timestamp)
	{
		using var filenames = new MemoryStream();
		var indices = AppendSymbols(filenames, files.Select(i => i.Name).ToArray());

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

			position += FILEHEADER_LENGTH + (uint)bytes.Length;

			// Align the position to 2 bytes
			if (position % 2 == 0) continue;

			contents.WriteByte(0);
			position++;
		}

		using var builder = new MemoryStream();
		builder.Write(Encoding.UTF8.GetBytes(SIGNATURE));

		using var symbol_buffer = AppendSymbols(files);

		var symbol_count = (uint)files.Sum(i => i.Symbols.Length);
		var export_table_size = sizeof(int) + symbol_count * sizeof(int) + symbol_buffer.Length;

		// Compute the offset which must be applied to all the file positions
		var offset = SIGNATURE.Length + FILEHEADER_LENGTH + export_table_size + filename_table.Length;
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
			var template_exports = ObjectExporter.GetTemplateExportFiles(context).Select(i => new StaticLibraryFormatFile(i.Key.Filename, Array.Empty<string>(), Encoding.UTF8.GetBytes(i.Value.ToString())));
			var exported_symbols = ObjectExporter.GetExportedSymbols(context).Merge(exports).Select(i => new StaticLibraryFormatFile(AssemblyPhase.GetObjectFileName(i.Key, output_name), i.Value.ToArray()));

			StaticLibraryFormat.Export(exported_symbols.Concat(template_exports).ToArray(), output_name);
		}
		catch (Exception e)
		{
			return Status.Error(e.Message);
		}

		return Status.OK;
	}
}