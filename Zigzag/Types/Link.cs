public class Link : Type
{
	public const int STRIDE = 1;

	public Link() : base("link", AccessModifier.PUBLIC) { }

	public override int GetContentSize()
	{
		return STRIDE;
	}
}