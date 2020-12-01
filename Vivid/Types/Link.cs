public class Link : Number
{
	private const int STRIDE = 1;

	public Link() : base(Parser.Format, Parser.Size.Bits, true, "link")
	{
		Identifier = "h";
	}

	public override void AddDefinition(Mangle mangle)
	{
		mangle.Value += "h";
	}

	public override int GetContentSize()
	{
		return STRIDE;
	}
}