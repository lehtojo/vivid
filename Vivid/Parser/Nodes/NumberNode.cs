using System;
using System.Collections.Generic;
using System.Globalization;

public class NumberNode : Node, ICloneable
{
	public Format Type { get; private set; }
	public object Value { get; set; }
	public int Bits => Common.GetBits(Value);

	public NumberNode(Format type, object value)
	{
		Type = type;
		Value = value;
		Instance = NodeType.NUMBER;

		if (Value is not long && Value is not double)
		{
			throw new ArgumentException("Number node received a number which was not in the correct format");
		}
	}

	public NumberNode(Format type, object value, Position? position)
	{
		Type = type;
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
			Value = System.Convert.ToDouble(Value, CultureInfo.InvariantCulture);
			Type = format;
		}
		else
		{
			Value = System.Convert.ToInt64(Value, CultureInfo.InvariantCulture);
			Type = format;
		}
	}

	public NumberNode Negate()
	{
		if (Type == Format.DECIMAL)
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
		return Numbers.Get(Type);
	}

	public new object Clone()
	{
		return new NumberNode(Type, Value, Position?.Clone());
	}

	public override bool Equals(object? other)
	{
		return other is NumberNode node &&
				base.Equals(other) &&
				Type == node.Type &&
				EqualityComparer<object>.Default.Equals(Value, node.Value);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Type, Value);
	}

	public override string ToString() => $"Number {Value}";
}
