public class Tiny : Number
{
	private const int BYTES = 1;

	public Tiny() : base(Format.INT8, 8, false, "tiny") { }

	public override int GetReferenceSize()
	{
		return BYTES;
	}

	public override int GetContentSize()
	{
		return BYTES;
	}
}
