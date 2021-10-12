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
	SetPrologueLength,
	SetEpilogueBegin,
	SetISA,
	Count
}

/// <summary>
/// Encodes the DWARF debug line section (.debug_line)
/// </summary>
public class DebugLineEncoderModule : DataEncoderModule
{
	public Position? Location { get; set; }
	public int Offset { get; set; } = 0;

	public DebugLineEncoderModule()
	{
		WriteInt64(0); // Set the length of this unit to zero initially
		WriteInt16(4); // Dwarf version 4
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
		Write(0); // DebugLineOperation.SetPrologueLength
		Write(0); // DebugLineOperation.SetEpilogueBegin
		Write(1); // DebugLineOperation.SetISA

		Write(0); // Indicate that now begins the last (only the compilation folder is added) included folder

		/// TODO: Add folders...
	}

	private void WriteOperation(DebugLineOperation operation)
	{
		Write((long)operation);
	}

	public void Move(Position location, int offset)
	{
		if (Location != null)
		{
			// Move to the specified binary offset
			WriteOperation(DebugLineOperation.AdvanceProgramCounter);
			WriteSLEB128(offset - Offset);

			// Move to the specified line
			WriteOperation(DebugLineOperation.AdvanceLine);
			WriteSLEB128(location.Line - Location.Line);

			// Move to the specified column
			WriteOperation(DebugLineOperation.SetColumn);
			WriteSLEB128(location.Character);

			Location = location;
			Offset = offset;
		}
	}
}