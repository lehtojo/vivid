public class Bool : Type
{
	private const int BYTES = 1;

	public Bool() : base("bool", AccessModifier.PUBLIC)
	{
		Identifier = "b";
	}

	public override void AddDefinition(Mangle mangle)
	{
		mangle.Value += "b";
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