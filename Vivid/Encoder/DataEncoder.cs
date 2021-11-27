using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System;

public class DataEncoderModule
{
	public const string DATA_SECTION_NAME = ".data";
	public const int DEFAULT_OUTPUT_SIZE = 100;
	
	public string Name { get; set; } = DATA_SECTION_NAME;
	public byte[] Output = new byte[DEFAULT_OUTPUT_SIZE];
	public int Position { get; set; } = 0;
	public Dictionary<string, BinarySymbol> Symbols { get; set; } = new Dictionary<string, BinarySymbol>();
	public List<BinaryRelocation> Relocations { get; set; } = new List<BinaryRelocation>();
	public List<BinaryOffset> Offsets { get; set; } = new List<BinaryOffset>();
	public int Alignment { get; set; } = 8;

	/// <summary>
	/// Ensures the internal buffer has the specified amount of bytes available
	/// </summary>
	public void Reserve(int bytes)
	{
		if (Output.Length - Position >= bytes) return;
		Array.Resize(ref Output, (Output.Length + bytes) * 2);
	}

	/// <summary>
	/// Writes the specified value to the current position and advances to the next position
	/// </summary>
	public void Write(long value)
	{
		Reserve(1);
		Output[Position++] = (byte)value;
	}

	/// <summary>
	/// Writes the specified value to the specified position
	/// </summary>
	public void Write(int position, long value)
	{
		Output[position] = (byte)value;
	}

	/// <summary>
	/// Writes the specified value to the current position and advances to the next position
	/// </summary>
	public void WriteInt16(long value)
	{
		Reserve(2);
		Output[Position++] = (byte)(value & 0xFF);
		Output[Position++] = (byte)((value & 0xFF00) >> 8);
	}

	/// <summary>
	/// Writes the specified value to the current position and advances to the next position
	/// </summary>
	public void WriteInt32(long value)
	{
		Reserve(4);
		Output[Position++] = (byte)(value & 0xFF);
		Output[Position++] = (byte)((value & 0xFF00) >> 8);
		Output[Position++] = (byte)((value & 0xFF0000) >> 16);
		Output[Position++] = (byte)((value & 0xFF000000) >> 24);
	}

	/// <summary>
	/// Writes the specified value to the specified position
	/// </summary>
	public void WriteInt32(int position, long value)
	{
		Output[position++] = (byte)(value & 0xFF);
		Output[position++] = (byte)((value & 0xFF00) >> 8);
		Output[position++] = (byte)((value & 0xFF0000) >> 16);
		Output[position++] = (byte)((value & 0xFF000000) >> 24);
	}

	/// <summary>
	/// Writes the specified value to the current position and advances to the next position
	/// </summary>
	public void WriteInt64(long value)
	{
		Reserve(8);
		Output[Position++] = (byte)(value & 0xFF);
		Output[Position++] = (byte)((value & 0xFF00) >> 8);
		Output[Position++] = (byte)((value & 0xFF0000) >> 16);
		Output[Position++] = (byte)((value & 0xFF000000) >> 24);
		Output[Position++] = (byte)((value & 0xFF00000000) >> 32);
		Output[Position++] = (byte)((value & 0xFF0000000000) >> 40);
		Output[Position++] = (byte)((value & 0xFF000000000000) >> 48);
		Output[Position++] = (byte)(((ulong)value & 0xFF00000000000000) >> 56);
	}

	/// <summary>
	/// Writes the specified value to the current position and advances to the next position
	/// </summary>
	public void WriteInt64(int position, long value)
	{
		Reserve(8);
		Output[position++] = (byte)(value & 0xFF);
		Output[position++] = (byte)((value & 0xFF00) >> 8);
		Output[position++] = (byte)((value & 0xFF0000) >> 16);
		Output[position++] = (byte)((value & 0xFF000000) >> 24);
		Output[position++] = (byte)((value & 0xFF00000000) >> 32);
		Output[position++] = (byte)((value & 0xFF0000000000) >> 40);
		Output[position++] = (byte)((value & 0xFF000000000000) >> 48);
		Output[position++] = (byte)(((ulong)value & 0xFF00000000000000) >> 56);
	}

	/// <summary>
	/// Expresses the specified value as a signed LEB128.
	/// </summary>
	private static byte[] ToULEB128(int value)
	{
		var bytes = new List<byte>();

		do
		{
			var x = value & 0x7F;
			value >>= 7;

			if (value != 0)
			{
				x |= (1 << 7);
			}
	
			bytes.Add((byte)x);

		} while (value != 0);

		return bytes.ToArray();
	}

	/// <summary>
	/// Expresses the specified value as an unsigned LEB128.
	/// </summary>
	private static byte[] ToSLEB128(int value)
	{
		var bytes = new List<byte>();

		var more = true;
		var negative = value < 0;

		while (more) 
		{
			var x = value & 0x7F;
			value >>= 7;

			// The following is only necessary if the implementation of >>= uses a logical shift rather than an arithmetic shift for a signed left operand
			if (negative)
			{
				value |= (~0 << (sizeof(int) - 7)); // Sign extend
			}

			// Sign bit of byte is second high order bit (0x40)
			if ((value == 0 && ((x & 0x40) == 0)) || (value == -1 && ((x & 0x40) == 0x40)))
			{
				more = false;
			}
			else
			{
				x |= (1 << 7);
			}

			bytes.Add((byte)x);
		}

		return bytes.ToArray();
	}

	/// <summary>
	/// Writes the specified integer as a SLEB128
	/// </summary>
	public void WriteSLEB128(int value)
	{
		Write(ToSLEB128(value));
	}

	/// <summary>
	/// Writes the specified integer as a ULEB128
	/// </summary>
	public void WriteULEB128(int value)
	{
		Write(ToULEB128(value));
	}

	/// <summary>
	/// Writes the specified bytes into this module
	/// </summary>
	public void Write(byte[] bytes)
	{
		Reserve(bytes.Length);
		Array.Copy(bytes, 0, Output, Position, bytes.Length);
		Position += bytes.Length;
	}

	/// <summary>
	/// Writes the specified amount of zeroes into this module
	/// </summary>
	public void Zero(int amount)
	{
		Reserve(amount);
		Position += amount;
	}

	/// <summary>
	/// Writes the specified string into this module
	/// </summary>
	public void String(string text, bool terminate = true)
	{
		var position = 0;

		while (position < text.Length)
		{
			var slice = new string(text.Skip(position).TakeWhile(i => i != '\\').ToArray());
			position += slice.Length;

			if (slice.Length > 0) Write(Encoding.ASCII.GetBytes(slice));

			if (position >= text.Length) break;

			position++; // Skip character '\'

			var command = text[position++];
			var length = 0;
			var error = string.Empty;

			if (command == 'x')
			{
				length = 2;
				error = "Can not understand hexadecimal value in a string";
			}
			else if (command == 'u')
			{
				length = 4;
				error = "Can not understand Unicode character in a string";
			}
			else if (command == 'U')
			{
				length = 8;
				error = "Can not understand Unicode character in a string";
			}
			else if (command == '\\')
			{
				Write('\\');
				continue;
			}
			else
			{
				throw new ApplicationException($"Can not understand string command '{command}'");
			}

			var hexadecimal = text.Substring(position, length);
			if (!ulong.TryParse(hexadecimal, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong value)) throw new ApplicationException(error);

			Write(BitConverter.GetBytes(value).Take(length / 2).ToArray());
			position += length;
		}

		if (terminate) Write(0);
	}

	/// <summary>
	/// Returns a local symbol with the specified name if such symbol exists, otherwise an external symbol with the specified name is created.
	/// </summary>
	public BinarySymbol GetLocalOrCreateExternalSymbol(string name)
	{
		if (Symbols.TryGetValue(name, out var symbol)) return symbol;

		// Create an external version of the specified symbol
		symbol = new BinarySymbol(name, 0, true);
		Symbols.Add(name, symbol);
		return symbol;
	}

	/// <summary>
	/// Creates a local symbol with the specified name at the specified offset.
	/// This function converts an existing external version of the symbol to a local symbol if such symbol exists.
	/// </summary>
	public BinarySymbol CreateLocalSymbol(string name, int offset, bool export = false)
	{
		if (Symbols.TryGetValue(name, out var symbol))
		{
			// Ensure the properties are correct
			symbol.External = false;
			symbol.Offset = offset;
			symbol.Export = export;
		}
		else
		{
			// Create a new local symbol with the specified properties
			symbol = new BinarySymbol(name, offset, false);
			Symbols.Add(name, symbol);
			symbol.Export = export;
		}

		return symbol;
	}

	public BinarySection Export()
	{
		// Shrink the output buffer to only fit the current size
		Array.Resize(ref Output, Position);

		// Add a hidden symbol that has the name of this section without dot. It represents the start of this section.
		var name = Name.Length > 0 && Name[0] == '.' ? Name.Substring(1) : Name;
		Symbols.Add(name, new BinarySymbol(name, 0, false));

		var section = new BinarySection(Name, BinarySectionType.DATA, Output);
		section.Alignment = Alignment;
		section.Relocations = Relocations;
		section.Symbols = Symbols;
		section.Offsets = Offsets;

		// If this section represents the primary data section, add the default flags
		if (Name == DATA_SECTION_NAME) { section.Flags = BinarySectionFlag.WRITE | BinarySectionFlag.ALLOCATE; }

		foreach (var symbol in Symbols.Values) { symbol.Section = section; }
		foreach (var relocation in Relocations) { relocation.Section = section; }

		return section;
	}

	public void Reset()
	{
		Array.Resize(ref Output, DEFAULT_OUTPUT_SIZE);
		Position = 0;
		Symbols.Clear();
		Relocations.Clear();
	}
}

public static class DataEncoder
{
	public const int SYSTEM_ADDRESS_SIZE = 8;

	/// <summary>
	/// Ensures the specified module is aligned as requested
	/// </summary>
	public static void Align(DataEncoderModule module, int alignment)
	{
		var padding = alignment - module.Position % alignment;
		if (padding == alignment) return;

		// By choosing the largest alignment, it is guaranteed that all the alignments are correct even after the linker relocates all sections
		module.Alignment = Math.Max(module.Alignment, alignment);
		module.Zero(padding);
	}

	/// <summary>
	/// Adds the specified table label into the specified module
	/// </summary>
	public static void AddTableLable(DataEncoderModule module, TableLabel label)
	{
		if (label.Declare)
		{
			// Define the table label as a symbol
			module.CreateLocalSymbol(label.Name, module.Position);
			return;
		}
		
		// Determine the relocation type
		var bytes = label.Size.Bytes;
		var position = module.Position;
		var type = BinaryRelocationType.ABSOLUTE64;

		if (bytes == 4) { type = label.IsSectionRelative ? BinaryRelocationType.SECTION_RELATIVE_32 : BinaryRelocationType.ABSOLUTE32; }
		else if (bytes == 8) { type = label.IsSectionRelative ? BinaryRelocationType.SECTION_RELATIVE_64 : BinaryRelocationType.ABSOLUTE64; }
		else throw new ApplicationException("Table label must be either 4-bytes or 8-bytes");

		// Allocate the table label
		module.Zero(bytes);

		module.Relocations.Add(new BinaryRelocation(module.GetLocalOrCreateExternalSymbol(label.Name), position, 0, type, bytes));
	}

	/// <summary>
	/// Adds the specified table into the specified module
	/// </summary>
	public static void AddTable(AssemblyBuilder builder, DataEncoderModule module, Table table)
	{
		if (!table.IsSection)
		{
			builder.Export(table.Name); // Export the table

			// Align the table
			if (Assembler.IsArm64) DataEncoder.Align(module, 16);

			// Define the table as a symbol
			module.CreateLocalSymbol(table.Name, module.Position);
		}

		// Align tables if the platform is ARM
		if (Assembler.IsArm64) Align(module, 8);

		var subtables = new List<Table>();

		foreach (var item in table.Items)
		{
			switch (item)
			{
				case string a:  { module.String(a);     break; }
				case long b:    { module.WriteInt64(b); break; }
				case int c:     { module.WriteInt32(c); break; }
				case short d:   { module.WriteInt16(d); break; }
				case byte e:    { module.Write(e);      break; }

				case Table f:
				{
					module.Relocations.Add(new BinaryRelocation(module.GetLocalOrCreateExternalSymbol(f.Name), module.Position, 0, BinaryRelocationType.ABSOLUTE64, SYSTEM_ADDRESS_SIZE));
					module.WriteInt64(0);

					subtables.Add(f);
					break;
				}

				case Label g:
				{
					module.Relocations.Add(new BinaryRelocation(module.GetLocalOrCreateExternalSymbol(g.GetName()), module.Position, 0, BinaryRelocationType.ABSOLUTE64, SYSTEM_ADDRESS_SIZE));
					module.WriteInt64(0);
					break;
				}

				case Offset h:
				{
					/// NOTE: All binary offsets are 4-bytes for now
					module.Offsets.Add(new BinaryOffset(module.Position, h, 4));
					module.WriteInt32(0);
					break;
				}

				case TableLabel i:
				{
					AddTableLable(module, i);
					break;
				}

				default: break;
			}
		}

		subtables.ForEach(i => AddTable(builder, module, i));
	}

	/// <summary>
	/// Defines the specified static variable
	/// </summary>
	public static void AddStaticVariable(DataEncoderModule module, Variable variable)
	{
		var name = variable.GetStaticName();
		var size = variable.Type!.AllocationSize;

		// Define the static variable as a symbol
		module.CreateLocalSymbol(name, module.Position);

		// Align tables if the platform is ARM
		if (Assembler.IsArm64) Align(module, 8);

		module.Zero(size);
	}
}