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
	public int Line { get; set; } = 0;
	public int Character { get; set; } = 0;
	public int Offset { get; set; } = 0;

	private void AddFile(string name, int folder)
	{
		String(name); // Terminated file name
		WriteULEB128(folder); // Folder index
		WriteULEB128(0); // Time
		WriteULEB128(0); // Size
	}

	public DebugLineEncoderModule()
	{
		WriteInt64(0); // Set the length of this unit to zero initially
		WriteInt16(4); // Dwarf version 4
		WriteInt32(32); // Prologue length
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

		Write(0); // Indicate that now begins the last (only the compilation folder is added) included folder

		/// TODO: Add folders...

		AddFile("main.v", 0);
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

	public void Move(int line, int character, int offset)
	{
		if (Line >= 0)
		{
			// Move to the specified binary offset
			WriteOperation(DebugLineOperation.AdvanceProgramCounter);
			WriteSLEB128(offset - Offset);

			// Move to the specified line
			WriteOperation(DebugLineOperation.AdvanceLine);
			WriteSLEB128(line - Line);

			// Move to the specified column
			WriteOperation(DebugLineOperation.SetColumn);
			WriteSLEB128(character);

			Character = Character;
			Offset = offset;
			Line = Line;
			return;
		}

		WriteExtendedOperation(DebugLineExtendedOperation.SetAddress, 8);
		WriteInt64(offset);
		WriteOperation(DebugLineOperation.Copy);

		// Move to the specified column
		WriteOperation(DebugLineOperation.SetColumn);
		WriteSLEB128(character);

		WriteOperation(DebugLineOperation.SetPrologueEnd);

		Line = Line;
		Character = Character;
	}

	public void End()
	{
		WriteExtendedOperation(DebugLineExtendedOperation.EndOfSequence, 0);
		Line = -1;
		Character = -1;
		Offset = 0;
	}

	public void Export()
	{
		Write(0, Position); // Compute the length now
	}
}