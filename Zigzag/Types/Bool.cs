public class Bool : Type {
    private const int BYTES = 1;

    public Bool() : base("bool", AccessModifier.PUBLIC) {}

	public override int GetSize()
	{
		return BYTES;
	}
}