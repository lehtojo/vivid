public class Ulong : Number
{
	private const int BYTES = 8;

	public Ulong() : base(NumberType.UINT64, 64, "ulong") { }

	public override int GetSize()
	{
		return BYTES;
	}
}
