public class Uint : Number
{
	private const int BYTES = 4;

	public Uint() : base(NumberType.UINT32, 32, "uint") { }

	public override int GetReferenceSize()
	{
		return BYTES;
	}

	public override int GetContentSize()
	{
		return BYTES;
	}
}
