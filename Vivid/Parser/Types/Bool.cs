public class Bool : Type
{
	public const string IDENTIFIER = "bool";
	private const int BYTES = 1;

	public Bool() : base(IDENTIFIER, Modifier.PUBLIC)
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