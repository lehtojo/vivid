public class U16 : Number
{
	private const int BYTES = 2;

	public U16() : base(Format.UINT16, 16, true, "u16")
	{
		Identifier = "t";
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
