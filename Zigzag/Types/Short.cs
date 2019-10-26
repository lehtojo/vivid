public class Short : Number
{
	private const int BYTES = 2;

	public Short() : base(NumberType.INT16, 16, "short") { }

	public override int GetSize()
	{
		return BYTES;
	}
}
