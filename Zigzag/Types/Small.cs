public class Small : Number
{
	private const int BYTES = 2;

	public Small() : base(Format.INT16, 16, false, "small") { }

	public override int GetReferenceSize()
	{
		return BYTES;
	}

	public override int GetContentSize()
	{
		return BYTES;
	}
}
