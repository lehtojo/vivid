using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System;

public static class BinaryUtility
{
	/// <summary>
	/// Goes through all the relocations from the specified sections and connects them to the local symbols if possible
	/// </summary>
	public static void UpdateRelocations(List<BinaryRelocation> relocations, Dictionary<string, BinarySymbol> symbols)
	{
		foreach (var relocation in relocations)
		{
			var symbol = relocation.Symbol;

			// If the relocation is not external, the symbol is already resolved
			if (!symbol.External) continue;

			// Try to find the actual symbol
			if (!symbols.TryGetValue(symbol.Name, out var definition)) continue; // throw new ApplicationException($"Symbol '{symbol.Name}' is not defined");

			relocation.Symbol = definition;
		}
	}

	/// <summary>
	/// Goes through all the relocations from the specified sections and connects them to the local symbols if possible
	/// </summary>
	public static void UpdateRelocations(List<BinarySection> sections, Dictionary<string, BinarySymbol> symbols)
	{
		foreach (var section in sections)
		{
			UpdateRelocations(section.Relocations, symbols);
		}
	}

	/// <summary>
	/// Exports the specified symbols
	/// </summary>
	public static void ApplyExports(Dictionary<string, BinarySymbol> symbols, HashSet<string> exports)
	{
		foreach (var export in exports)
		{
			if (symbols.TryGetValue(export, out var symbol))
			{
				symbol.Export = true;
				continue;
			}

			throw new ApplicationException($"Exporting of symbol {export} is requested, but it does not exist");
		}
	}

	/// <summary>
	/// Returns a list of all symbols in the specified sections
	/// </summary>
	public static Dictionary<string, BinarySymbol> GetAllSymbolsFromSections(List<BinarySection> sections)
	{
		var symbols = new Dictionary<string, BinarySymbol>();

		foreach (var symbol in sections.SelectMany(i => i.Symbols.Values))
		{
			// 1. Just continue, if the symbol can be added
			// 2. If this is executed, it means that some version of the current symbol is already added.
			// However, if the current symbol is external, it does not matter.
			if (symbols.TryAdd(symbol.Name, symbol) || symbol.External) continue;

			// If the version of the current symbol in the dictionary is not external, the current symbol is defined at least twice
			var conflict = symbols[symbol.Name];
			if (!conflict.External) throw new ApplicationException($"Symbol {symbol.Name} is created at least twice");

			// Since the version of the current symbol in the dictionary is external, it can be replaced with the actual definition (current symbol)
			symbols[symbol.Name] = symbol;
		}

		return symbols;
	}

	/// <summary>
	/// Computes all offsets in the specified sections. If any of the offsets can not computed, this function throws an exception.
	/// </summary>
	public static void ComputeOffsets(List<BinarySection> sections, Dictionary<string, BinarySymbol> symbols)
	{
		foreach (var section in sections)
		{
			foreach (var offset in section.Offsets)
			{
				// Try to find the 'from'-symbol
				var symbol = offset.Offset.From.Name;
				if (!symbols.TryGetValue(symbol, out var from)) throw new ApplicationException($"Can not compute an offset, because symbol {symbol} can not be found");

				// Try to find the 'to'-symbol
				symbol = offset.Offset.To.Name;
				if (!symbols.TryGetValue(symbol, out var to)) throw new ApplicationException($"Can not compute an offset, because symbol {symbol} can not be found");

				// Ensure both symbols are defined locally
				if (from.Section == null || to.Section == null) throw new ApplicationException("Both symbols in offsets must be local");

				// Compute the offset between the symbols
				var value = (to.Section!.VirtualAddress + to.Offset) - (from.Section!.VirtualAddress + from.Offset);

				switch (offset.Bytes)
				{
					case 8: { WriteInt64(section.Data, offset.Position, value); break; }
					case 4: { WriteInt32(section.Data, offset.Position, value); break; }
					case 2: { WriteInt16(section.Data, offset.Position, value); break; }
					case 1: { Write(section.Data, offset.Position, value); break; }
					default: throw new ApplicationException("Unsupported offset size");
				}
			}
		}
	}

	public static int Write<T>(byte[] destination, int offset, T source)
	{
		var size = Marshal.SizeOf(source);
		var buffer = Marshal.AllocHGlobal(size);
		Marshal.StructureToPtr(source!, buffer, false);
		Marshal.Copy(buffer, destination, offset, size);
		Marshal.FreeHGlobal(buffer);
		return offset + size;
	}

	public static void Write<T>(byte[] destination, int offset, List<T> source)
	{
		foreach (var element in source)
		{
			offset = Write(destination, offset, element);
		}
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
	/// Writes the specified value to the specified position
	/// </summary>
	public static void Write(byte[] data, int offset, long value)
	{
		data[offset] = (byte)(value & 0xFF);
	}

	/// <summary>
	/// Writes the specified value to the specified position
	/// </summary>
	public static void WriteInt16(byte[] data, int offset, long value)
	{
		data[offset++] = (byte)(value & 0xFF);
		data[offset++] = (byte)((value & 0xFF00) >> 8);
	}

	/// <summary>
	/// Writes the specified value to the specified position
	/// </summary>
	public static void WriteInt32(byte[] data, int position, long value)
	{
		data[position++] = (byte)(value & 0xFF);
		data[position++] = (byte)((value & 0xFF00) >> 8);
		data[position++] = (byte)((value & 0xFF0000) >> 16);
		data[position++] = (byte)((value & 0xFF000000) >> 24);
	}

	/// <summary>
	/// Writes the specified value to the specified position
	/// </summary>
	public static void WriteInt64(byte[] data, int offset, long value)
	{
		data[offset++] = (byte)(value & 0xFF);
		data[offset++] = (byte)((value & 0xFF00) >> 8);
		data[offset++] = (byte)((value & 0xFF0000) >> 16);
		data[offset++] = (byte)((value & 0xFF000000) >> 24);
		data[offset++] = (byte)((value & 0xFF00000000) >> 32);
		data[offset++] = (byte)((value & 0xFF0000000000) >> 40);
		data[offset++] = (byte)((value & 0xFF000000000000) >> 48);
		data[offset++] = (byte)(((ulong)value & 0xFF00000000000000) >> 56);
	}
}