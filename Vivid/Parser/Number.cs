using System;

public class Number : Type
{
	public bool IsUnsigned { get; private set; }
	public int Bits { get; private set; }

	public int Bytes => Bits / 8;

	public Number(Format format, int bits, bool unsigned, string name) : base(name, Modifier.DEFAULT | Modifier.PRIMITIVE)
	{
		DefaultAllocationSize = bits / 8;
		Format = format;
		Bits = bits;
		IsUnsigned = unsigned;
	}

	public override bool Equals(object? other)
	{
		return other is Number number && number.IsPrimitive && Identifier == number.Identifier && Bytes == number.Bytes && Format == number.Format;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Identifier, Bytes, Format);
	}
}