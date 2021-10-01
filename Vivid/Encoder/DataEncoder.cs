using System;
using System.Collections.Generic;
using System.Text;

public class DataEncoderModule
{
	public const int DefaultOutputSize = 100;

	public byte[] Output = new byte[DefaultOutputSize];
	public int Position { get; set; } = 0;
	public Dictionary<string, BinarySymbol> Symbols { get; set; } = new Dictionary<string, BinarySymbol>();
	public List<BinaryRelocation> Relocations { get; set; } = new List<BinaryRelocation>();

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
	public void String(string value)
	{
		#warning Support text commands
		Write(Encoding.ASCII.GetBytes(value));
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
	public BinarySymbol CreateLocalSymbol(string name, int offset)
	{
		if (Symbols.TryGetValue(name, out var symbol))
		{
			// Ensure the properties are correct
			symbol.External = false;
			symbol.Offset = offset;
		}
		else
		{
			// Create a new local symbol with the specified properties
			symbol = new BinarySymbol(name, offset, false);
			Symbols.Add(name, symbol);
		}

		return symbol;
	}

	public BinarySection Export()
	{
		// Shrink the output buffer to only fit the current size
		Array.Resize(ref Output, Position);

		var section = new BinarySection(ElfFormat.DATA_SECTION, BinarySectionType.DATA, Output);
		section.Relocations = Relocations;
		section.Symbols = Symbols;

		foreach (var symbol in Symbols.Values) { symbol.Section = section; }
		foreach (var relocation in Relocations) { relocation.Section = section; }

		return section;
	}
}

public static class DataEncoder
{
	public const int SystemAddressSize = 8;

	/// <summary>
	/// Ensures the specified module is aligned as requested
	/// </summary>
	public static void Align(DataEncoderModule module, int alignment)
	{
		module.Zero(alignment - module.Position % alignment);
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

		if (label.IsSectionRelative)
		{
			module.Relocations.Add(new BinaryRelocation(module.GetLocalOrCreateExternalSymbol(label.Name), module.Position, 0, BinaryRelocationType.SECTION_RELATIVE, label.Size.Bytes));
			return;
		}

		module.Relocations.Add(new BinaryRelocation(module.GetLocalOrCreateExternalSymbol(label.Name), module.Position, 0, BinaryRelocationType.ABSOLUTE, label.Size.Bytes));
	}

	/// <summary>
	/// Adds the specified table into the specified module
	/// </summary>
	public static void AddTable(DataEncoderModule module, Table table)
	{
		//if (table.IsBuilt) return;
		//table.IsBuilt = true;

		if (table.IsSection) throw new NotSupportedException("Tables must not be sections here");

		// Define the table as a symbol
		module.CreateLocalSymbol(table.Name, module.Position);

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
					module.Relocations.Add(new BinaryRelocation(module.GetLocalOrCreateExternalSymbol(f.Name), module.Position, 0, BinaryRelocationType.ABSOLUTE, SystemAddressSize));
					module.WriteInt64(0);

					if (!f.IsBuilt) subtables.Add(f);
					break;
				}

				case Label g:
				{
					module.Relocations.Add(new BinaryRelocation(module.GetLocalOrCreateExternalSymbol(g.GetName()), module.Position, 0, BinaryRelocationType.ABSOLUTE, SystemAddressSize));
					module.WriteInt64(0);
					break;
				}

				case Offset h:
				{
					throw new NotImplementedException("Data section offsets are not implemented yet");
				}

				case TableLabel i:
				{
					AddTableLable(module, i);
					break;
				}

				default: break;
			}
		}

		subtables.ForEach(i => AddTable(module, i));
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