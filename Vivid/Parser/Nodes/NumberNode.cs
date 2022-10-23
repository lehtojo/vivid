using System;
using System.Collections.Generic;
using System.Globalization;

public class NumberNode : Node, ICloneable
{
	public Format Format { get; private set; }
	public object Value { get; set; }
	public int Bits => Common.GetBits(Value);

	public NumberNode(Format format, object value)
	{
		Format = format;
		Value = value;
		Instance = NodeType.NUMBER;

		if (Value is not long && Value is not double)
		{
			throw new ArgumentException("Number node received a number which was not in the correct format");
		}
	}

	public NumberNode(Format format, object value, Position? position)
	{
		Format = format;
		Value = value;
		Position = position;
		Instance = NodeType.NUMBER;

		if (Value is not long && Value is not double)
		{
			throw new ArgumentException("Number node received a number which was not in the correct format");
		}
	}

	public void Convert(Format format)
	{
		if (format == Format.DECIMAL)
		{
			Value = System.Convert.ToDouble(Value);
			Format = format;
		}
		else
		{
			Value = System.Convert.ToInt64(Value);
			Format = format;
		}
	}

	public NumberNode Negate()
	{
		if (Format == Format.DECIMAL)
		{
			Value = -(double)Value;
		}
		else
		{
			Value = -(long)Value;
		}

		return this;
	}

	public override Type? TryGetType()
	{
		return Numbers.Get(Format);
	}

	public new object Clone()
	{
		return new NumberNode(Format, Value, Position?.Clone());
	}

	public override bool Equals(object? other)
	{
		return other is NumberNode node &&
				base.Equals(other) &&
				Format == node.Format &&
				EqualityComparer<object>.Default.Equals(Value, node.Value);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Format, Value);
	}

	public override string ToString() => $"Number {Value}";
}
