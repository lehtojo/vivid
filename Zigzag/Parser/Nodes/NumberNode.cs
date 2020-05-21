using System;
using System.Collections.Generic;

public class NumberNode : Node, IType, ICloneable
{
	public Format Type { get; private set; }
	public object Value { get; set; }

	public NumberNode(Format type, object value)
	{
		Type = type;
		Value = value;
	}

	public void Negate()
	{
		if (Type == Format.DECIMAL)
		{
			Value = -(double)Value;
		}
		else
		{
			Value = -(long)Value;
		}
	}

	public new Type GetType()
	{
		return Numbers.Get(Type);
	}

	public override NodeType GetNodeType()
	{
		return NodeType.NUMBER_NODE;
	}

	public object Clone()
	{
		return new NumberNode(Type, Value);
	}

	public override bool Equals(object? obj)
	{
		return obj is NumberNode node &&
				base.Equals(obj) &&
				Type == node.Type &&
				EqualityComparer<object>.Default.Equals(Value, node.Value);
	}

	public override int GetHashCode()
	{
		HashCode hash = new HashCode();
		hash.Add(base.GetHashCode());
		hash.Add(Type);
		hash.Add(Value);
		return hash.ToHashCode();
	}
}
