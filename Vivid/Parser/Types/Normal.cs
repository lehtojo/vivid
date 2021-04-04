public class Normal : Number
{
	private const int BYTES = 4;

	public Normal() : base(Format.INT32, 32, false, "normal")
	{
		Identifier = "i";
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
