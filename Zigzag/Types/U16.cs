public class U16 : Number
{
	private const int BYTES = 2;

	public U16() : base(NumberType.UINT16, 16, true, "u16") { }

	public override int GetReferenceSize()
	{
		return BYTES;
	}

	public override int GetContentSize()
	{
		return BYTES;
	}
}
