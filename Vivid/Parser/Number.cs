using System;

public class Number : Type
{
	public Format Type { get; private set; }
	public bool IsUnsigned { get; private set; }
	public int Bits { get; private set; }

	public int Bytes => Bits / 8;

	public Number(Format type, int bits, bool unsigned, string name) : base(name, Modifier.DEFAULT | Modifier.PRIMITIVE)
	{
		Type = type;
		Bits = bits;
		IsUnsigned = unsigned;
	}

	public override Format GetFormat()
	{
		return Type;
	}

	public override int GetReferenceSize()
	{
		return Bytes;
	}

	public override int GetContentSize()
	{
		return Bytes;
	}

	public override bool Equals(object? other)
	{
		return other is Number number && number.IsPrimitive && Identifier == number.Identifier && Bytes == number.Bytes && Type == number.Type;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Identifier, Bytes, Type);
	}
}