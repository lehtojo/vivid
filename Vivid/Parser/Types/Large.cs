public class Large : Number
{
	private const int BYTES = 8;

	public Large() : base(Format.INT64, 64, false, "large")
	{
		Identifier = "x";
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
