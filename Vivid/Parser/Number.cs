using System;

public class Number : Type
{
	private new Format Format { get; set; }
	public bool IsUnsigned { get; private set; }
	public int Bits { get; private set; }

	public int Bytes => Bits / 8;

	public Number(Format format, int bits, bool unsigned, string name) : base(name, Modifier.DEFAULT | Modifier.PRIMITIVE)
	{
		Format = format;
		Bits = bits;
		IsUnsigned = unsigned;
	}

	public override Format GetFormat()
	{
		return Format;
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
		return other is Number number && number.IsPrimitive && Identifier == number.Identifier && Bytes == number.Bytes && Format == number.Format;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Identifier, Bytes, Format);
	}
}