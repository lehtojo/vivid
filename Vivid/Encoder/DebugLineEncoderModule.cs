using System.IO;

public enum DebugLineOperation : long
{
	None,
	Copy,
	AdvanceProgramCounter,
	AdvanceLine,
	SetFile,
	SetColumn,
	NegateStatement,
	SetBasicBlock,
	ConstantAddProgramCounter,
	FixedAdvanceProgramCounter,
	SetPrologueEnd,
	SetEpilogueBegin,
	SetISA,
	Count
}

public enum DebugLineExtendedOperation : long
{
	None,
	EndOfSequence,
	SetAddress,
	DefineFile,
	SetDiscriminator
}

/// <summary>
/// Encodes the DWARF debug line section (.debug_line)
/// </summary>
public class DebugLineEncoderModule : DataEncoderModule
{
	public const string SECTION_NAME = "debug_line";
	public const string DEBUG_CODE_START_SYMBOL = ".debug_code_start";
	public const int PROLOGUE_LENGTH_FIELD_OFFSET = 6;

	public int Line { get; set; } = -1;
	public int Character { get; set; } = -1;
	public int Offset { get; set; } = -1;

	private void AddFolder(string folder)
	{
		String(folder); // Terminated folder path
	}

	private void AddFile(string name, int folder)
	{
		String(name); // Terminated file name
		WriteULEB128(folder); // Folder index
		WriteULEB128(0); // Time
		WriteULEB128(0); // Size
	}

	public DebugLineEncoderModule(string file)
	{
		WriteInt32(0); // Set the length of this unit to zero initially
		WriteInt16(4); // Dwarf version 4
		WriteInt32(0); // Prologue length
		Write(1); // Minimum instruction length
		Write(1); // Maximum operations per instruction
		Write(1); // Default 'is statement' flag
		Write(1); // Line base
		Write(1); // Line range
		Write((int)DebugLineOperation.Count); // Operation code base

		// Specify the number of arguments for each standard operation
		Write(0); // DebugLineOperation.Copy
		Write(1); // DebugLineOperation.AdvanceProgramCounter
		Write(1); // DebugLineOperation.AdvanceLine
		Write(1); // DebugLineOperation.SetFile
		Write(1); // DebugLineOperation.SetColumn
		Write(0); // DebugLineOperation.NegateStatement
		Write(0); // DebugLineOperation.SetBasicBlock
		Write(0); // DebugLineOperation.ConstantAddProgramCounter
		Write(1); // DebugLineOperation.FixedAdvanceProgramCounter
		Write(0); // DebugLineOperation.SetPrologueEnd
		Write(0); // DebugLineOperation.SetEpilogueBegin
		Write(1); // DebugLineOperation.SetISA

		var folder = Path.GetDirectoryName(file);
		if (folder != null) AddFolder(folder);

		Write(0); // Indicate that now begins the last (only the compilation folder is added) included folder

		AddFile(Path.GetFileName(file), 1);

		Write(0); // End of included files

		// Compute the length of this header after the 'Prologue length'-field
		WriteInt32(PROLOGUE_LENGTH_FIELD_OFFSET, Position - (PROLOGUE_LENGTH_FIELD_OFFSET + 4));
	}

	private void WriteOperation(DebugLineOperation operation)
	{
		Write((long)operation);
	}

	private void WriteExtendedOperation(DebugLineExtendedOperation operation, int parameter_bytes)
	{
		Write(0); // Begin extended operation code
		Write(parameter_bytes + 1); // Write the number of bytes to read
		Write((long)operation);
	}

	public void Move(BinarySection section, int line, int character, int offset)
	{
		if (Line >= 0)
		{
			// Move to the specified line
			WriteOperation(DebugLineOperation.AdvanceLine);
			WriteSLEB128(line - Line);

			// Move to the specified column
			WriteOperation(DebugLineOperation.SetColumn);
			WriteSLEB128(character);

			// Move to the specified binary offset
			WriteOperation(DebugLineOperation.AdvanceProgramCounter);
			WriteSLEB128(offset - Offset);

			WriteOperation(DebugLineOperation.Copy);

			Character = character;
			Offset = offset;
			Line = line;
			return;
		}

		if (line > 1)
		{
			// Move to the specified line
			WriteOperation(DebugLineOperation.AdvanceLine);
			WriteSLEB128(line - 1);
		}

		// Move to the specified column
		WriteOperation(DebugLineOperation.SetColumn);
		WriteSLEB128(character);

		WriteExtendedOperation(DebugLineExtendedOperation.SetAddress, 8);
		
		// Add a symbol to the text section, which represents the start of the debuggable code.
		// This is done, because now the machine code offset is not correct, since after linking the code will probably be loaded to another address.
		// By inserting a symbol into the text section and adding a relocation using the symbol to this section, the offset will be corrected by the linker.
		var symbol = new BinarySymbol(DEBUG_CODE_START_SYMBOL, offset, false);
		symbol.Section = section;

		section.Symbols.Add(DEBUG_CODE_START_SYMBOL, symbol);
		Relocations.Add(new BinaryRelocation(symbol, Position, 0, BinaryRelocationType.ABSOLUTE64, 8));

		WriteInt64(offset);

		WriteOperation(DebugLineOperation.Copy);

		Character = character;
		Offset = offset;
		Line = line;
	}

	public new BinarySection Export()
	{
		WriteExtendedOperation(DebugLineExtendedOperation.EndOfSequence, 0);
		WriteInt32(0, Position - 4); // Compute the length now

		Name = '.' + SECTION_NAME;
		return base.Export();
	}
}