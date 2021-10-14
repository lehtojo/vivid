

public class DebugFrameEncoderModule : DataEncoderModule
{
	private int ActiveEntryStart { get; set; } = 0;

	public DebugFrameEncoderModule(int identity)
	{
		WriteInt64(0); // Set the length of this unit to zero initially
		WriteInt64(identity); // CIE ID
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
		Write(new byte[] { 0x0C, 0x07, 0x08, 0x90, 0x01 });
	}

	public void Start()
	{
		ActiveEntryStart = Position;

		WriteInt32(0); // Set the length to zero for now
		WriteInt32(Position); // Write current offset
		WriteInt32(0); // Offset to the start of the function machine code
		WriteInt32(0); // Number of bytes in the machine code
		Write(0); // Padding?
	}

	public void End()
	{
		WriteInt32(ActiveEntryStart, Position - ActiveEntryStart); // Compute the entry length now
		WriteInt32(ActiveEntryStart, Position - ActiveEntryStart); // Compute the entry length now
	}
}