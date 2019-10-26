public class Tiny : Number
{
	private const int BYTES = 1;

	public Tiny() : base(NumberType.INT8, 8, "tiny") { }

	public override int GetSize()
	{
		return BYTES;
	}
}
