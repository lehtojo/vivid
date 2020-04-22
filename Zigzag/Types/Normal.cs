public class Normal : Number
{
	private const int BYTES = 4;

	public Normal() : base(NumberType.INT32, 32, "normal") { }

	public override int GetReferenceSize()
	{
		return BYTES;
	}

	public override int GetContentSize()
	{
		return BYTES;
	}
}
