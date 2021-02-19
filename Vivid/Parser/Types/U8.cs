public class U8 : Number
{
	private const int BYTES = 1;

	public U8() : base(Format.UINT8, 8, true, "u8")
	{
		Identifier = "h";
	}

	public override void AddDefinition(Mangle mangle)
	{
		mangle.Value += "h";
	}

	public override int GetReferenceSize()
	{
		return BYTES;
	}

	public override int GetContentSize()
	{
		return BYTES;
	}
}
