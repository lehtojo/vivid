public class Link : Number
{
	private const int STRIDE = 1;

	public Link() : base(Parser.Format, Parser.Size.Bits, true, "link")
	{
		Identifier = "Ph";
	}

	public override void AddDefinition(Mangle mangle)
	{
		mangle.Value += "Ph";
	}

	public override int GetContentSize()
	{
		return STRIDE;
	}
}