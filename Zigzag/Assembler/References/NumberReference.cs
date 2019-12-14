using System.Collections.Generic;

public class NumberReference : Reference
{
	public object Value { get; private set; }

	public NumberReference(object value, Size size) : base(size)
	{
		Value = value;
	}

	public override string Use(Size size)
	{
		return string.Format("{0} {1}", size, Value.ToString().Replace(',', '.'));
	}

	public override string Use()
	{
		return Value.ToString().Replace(',', '.');
	}

	public override bool IsComplex()
	{
		return false;
	}

	public override LocationType GetType()
	{
		return LocationType.NUMBER;
	}

	public override bool Equals(object? obj)
	{
		return obj is NumberReference reference &&
			   EqualityComparer<object>.Default.Equals(Value, reference.Value);
	}
}