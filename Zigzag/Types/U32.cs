public class U32 : Number
{
	private const int BYTES = 4;

	public U32() : base(Format.UINT32, 32, true, "u32") { }

	public override int GetReferenceSize()
	{
		return BYTES;
	}

	public override int GetContentSize()
	{
		return BYTES;
	}
}
