using System;
using System.Collections.Generic;

public class NumberNode : Node, IType, ICloneable
{
	public Format Type { get; }
	public object Value { get; set; }

	public NumberNode(Format type, object value)
	{
		Type = type;
		Value = value;

		if (!(Value is long || Value is double))
		{
			throw new ArgumentException("Number node received a number which was not in the correct format");
		}
	}

	public NumberNode(Format type, object value, Position? position)
	{
		Type = type;
		Value = value;
		Position = position;

		if (!(Value is long || Value is double))
		{
			throw new ArgumentException("Number node received a number which was not in the correct format");
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

	public new Type GetType()
	{
		return Numbers.Get(Type);
	}

	public override NodeType GetNodeType()
	{
		return NodeType.NUMBER;
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
		var hash = new HashCode();
		hash.Add(base.GetHashCode());
		hash.Add(Type);
		hash.Add(Value);
		return hash.ToHashCode();
	}
}
