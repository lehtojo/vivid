public abstract class Number : Type
{
	public NumberType Type { get; private set; }
	public int Bits { get; private set; }

	public int Bytes => Bits / 8;

	public Number(NumberType type, int bits, string name) : base(name, AccessModifier.PUBLIC)
	{
		Type = type;
		Bits = bits;
	}
}
