public class NumberReference : Reference
{
	public object Value { get; private set; }

	public NumberReference(object value, Size size) : base(size)
	{
		Value = value;
	}

	public override string Use(Size size)
	{
		return string.Format("{0} {1}", size, Value.ToString());
	}

	public override LocationType GetType()
	{
		return LocationType.NUMBER;
	}
}