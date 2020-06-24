public class Link : Type
{
	private const int STRIDE = 1;

	public Link() : base("link", AccessModifier.PUBLIC) { }

	public override int GetContentSize()
	{
		return STRIDE;
	}
}