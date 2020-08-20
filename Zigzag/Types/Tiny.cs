public class Tiny : Number
{
	private const int BYTES = 1;

	public Tiny() : base(Format.INT8, 8, false, "tiny") 
	{ 
		Identifier = "c";
	}

	public override void AddDefinition(Mangle mangle)
	{
		mangle.Value += "c";
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
