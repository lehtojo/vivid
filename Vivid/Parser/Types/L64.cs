public class L64 : Number
{
	private const int STRIDE = 8;

	public L64() : base(Parser.Format, Parser.Size.Bits, true, "l64")
	{
		Identifier = "Px";
	}

	public override Type GetOffsetType()
	{
		return global::Types.LARGE;
	}

	public override void AddDefinition(Mangle mangle)
	{
		mangle.Value += "Px";
	}

	public override int GetContentSize()
	{
		return STRIDE;
	}
}