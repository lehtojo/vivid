public class Small : Number
{
	private const int BYTES = 2;

	public Small() : base(Format.INT16, 16, false, "small")
	{
		Identifier = "s";
	}

	public override void AddDefinition(Mangle mangle)
	{
		mangle.Value += "s";
	}

	public override int GetReferenceSize()
	{
		return BYTES;
	}

	public override int GetContentSize()
	{
		return BYTES;
	}
}
