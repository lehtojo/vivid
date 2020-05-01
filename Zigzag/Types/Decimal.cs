public class Decimal : Number
{
	public Decimal() : base(Format.DECIMAL, 32, false, "decimal") { }

	public override int GetReferenceSize()
	{
		return Parser.Size.Bytes;
	}
	
	public override int GetContentSize()
	{
		return Parser.Size.Bytes;
	}
}
