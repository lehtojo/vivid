public class Normal : Number
{
	private const int BYTES = 4;

	public Normal() : base(NumberType.INT32, 32, "num") { }

	public override int GetSize()
	{
		return BYTES;
	}
}
