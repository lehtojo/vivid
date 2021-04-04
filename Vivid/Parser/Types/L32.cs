public class L32 : Number
{
	private const int STRIDE = 4;

	public L32() : base(Parser.Format, Parser.Size.Bits, true, "l32")
	{
		Identifier = "Pi";
	}

	public override Type GetOffsetType()
	{
		return global::Types.NORMAL;
	}

	public override int GetContentSize()
	{
		return STRIDE;
	}
}