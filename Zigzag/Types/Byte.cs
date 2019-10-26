public class Byte : Number
{
	private const int BYTES = 1;

	public Byte() : base(NumberType.UINT8, 8, "byte") { }

	public override int GetSize()
	{
		return BYTES;
	}
}
