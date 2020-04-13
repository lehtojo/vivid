public class Decimal : Number
{
	private const int BYTES = 4;

	public Decimal() : base(NumberType.DECIMAL32, 32, "decimal") { }

	public override int GetReferenceSize()
	{
		return BYTES;
	}
	public override int GetContentSize()
	{
		return BYTES;
	}
}
