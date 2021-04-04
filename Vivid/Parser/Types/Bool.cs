public class Bool : Type
{
	private const int BYTES = 1;

	public Bool() : base("bool", Modifier.PUBLIC)
	{
		Identifier = "b";
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