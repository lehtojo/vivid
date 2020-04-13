public class Ushort : Number
{
	private const int BYTES = 2;

	public Ushort() : base(NumberType.UINT16, 16, "ushort") { }

	public override int GetReferenceSize()
	{
		return BYTES;
	}

	public override int GetContentSize()
	{
		return BYTES;
	}
}
