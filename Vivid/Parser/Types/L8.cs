public class L8 : Number
{
	private const int STRIDE = 1;

	public L8() : base(Parser.Format, Parser.Size.Bits, true, "l8")
	{
		Identifier = "Ph";
	}

	public override Type GetOffsetType()
	{
		return global::Types.TINY;
	}

	public override int GetContentSize()
	{
		return STRIDE;
	}
}