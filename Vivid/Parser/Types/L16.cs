public class L16 : Number
{
	private const int STRIDE = 2;

	public L16() : base(Parser.Format, Parser.Size.Bits, true, "l16")
	{
		Identifier = "Ps";
	}

	public override Type GetOffsetType()
	{
		return global::Types.SMALL;
	}

	public override int GetContentSize()
	{
		return STRIDE;
	}
}