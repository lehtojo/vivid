public enum DebugFrameOperation : long
{
	Advance1 = 0x02,
	Advance2 = 0x03,
	Advance4 = 0x04,
	SetFrameOffset = 0x0E
}

public class DebugFrameEncoderModule : DataEncoderModule
{
	public const string SECTION_NAME = "eh_frame";

	private int ActiveEntryStart { get; set; } = 0;
	private int MachineCodeStart { get; set; } = 0;

	public DebugFrameEncoderModule(int identity)
	{
		WriteInt32(20);
		WriteInt32(identity); // CIE ID
		Write(1); // Version
		Write('z');
		Write('R');
		Write(0); // Augmentation
		WriteULEB128(1); // Code aligment factor
		WriteSLEB128(-8); // Data alignment factor
		WriteULEB128(16); // Return address register
		Write(1);
		Write(0x1B);

		// Default rules:
		// DW_CFA_def_cfa: r7 (rsp) offset 8
		// DW_CFA_offset: r16 (rip) at cfa - 8
		// DW_CFA_nop
		// DW_CFA_nop
		Write(new byte[] { 0x0C, 0x07, 0x08, 0x90, 0x01, 0x00, 0x00 });
	}

	private void WriteOperation(DebugFrameOperation operation)
	{
		Write((long)operation);
	}

	public void Start(string name, int offset)
	{
		ActiveEntryStart = Position;
		MachineCodeStart = offset;

		WriteInt32(0); // Set the length to zero for now
		WriteInt32(Position); // Write current offset

		var symbol = new BinarySymbol(name, 0, true);
		Relocations.Add(new BinaryRelocation(symbol, Position, 0, BinaryRelocationType.PROGRAM_COUNTER_RELATIVE));

		WriteInt32(offset); // Offset to the start of the function machine code
		WriteInt32(0); // Number of bytes in the machine code
		Write(0); // Padding?
	}

	public void SetFrameOffset(int offset)
	{
		WriteOperation(DebugFrameOperation.SetFrameOffset);
		WriteULEB128(offset);
	}

	public void Move(int delta)
	{
		if (delta == 0) return;

		if (delta >= sbyte.MinValue && delta <= sbyte.MaxValue)
		{
			WriteOperation(DebugFrameOperation.Advance1);
			Write(delta);
			return;
		}

		if (delta >= short.MinValue && delta <= short.MaxValue)
		{
			WriteOperation(DebugFrameOperation.Advance2);
			WriteInt16(delta);
			return;
		}

		WriteOperation(DebugFrameOperation.Advance4);
		WriteInt32(delta);
		return;
	}

	public void End(int offset)
	{
		WriteInt32(ActiveEntryStart, Position - ActiveEntryStart - 4); // Compute the entry length now
		WriteInt32(ActiveEntryStart + 12, offset - MachineCodeStart);
	}

	public new BinarySection Export()
	{
		Name = '.' + SECTION_NAME;

		var section = base.Export();
		section.Flags = BinarySectionFlags.ALLOCATE;
		section.Alignment = 1;

		return section;
	}
}