public class Link : Type
{
	private const int STRIDE = 1;

	public Link() : base("link", AccessModifier.PUBLIC) 
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