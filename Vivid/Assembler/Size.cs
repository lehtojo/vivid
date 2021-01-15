using System;

public class Size
{
	public static readonly Size NONE = new Size("?", "?", 0);
	public static readonly Size BYTE = new Size("byte", ".byte", 1);
	public static readonly Size WORD = new Size("word", ".short", 2);
	public static readonly Size DWORD = new Size("dword", ".long", 4);
	public static readonly Size QWORD = new Size("qword", ".quad", 8);
	public static readonly Size XMMWORD = new Size("xmmword", ".xword", 16);
	public static readonly Size YMMWORD = new Size("ymmword", ".yword", 32);

	public string Identifier { get; private set; }
	public string Allocator { get; private set; }	
	public int Bytes { get; private set; }
	public int Bits => Bytes * 8;

	private Size(string identifier, string allocator, int bytes)
	{
		Identifier = identifier;
		Allocator = allocator;
		Bytes = bytes;
	}

	public static Size FromBytes(int bytes)
	{
		return bytes switch
		{
			1 => BYTE,
			2 => WORD,
			4 => DWORD,
			8 => QWORD,
			16 => XMMWORD,
			32 => YMMWORD,
			_ => throw new ApplicationException("Invalid instruction parameter size given"),
		};
	}

	public static Size? TryGetFromBytes(int bytes)
	{
		return bytes switch
		{
			1 => BYTE,
			2 => WORD,
			4 => DWORD,
			8 => QWORD,
			16 => XMMWORD,
			32 => YMMWORD,
			_ => null,
		};
	}

	public static Size FromFormat(Format type)
	{
		switch (type)
		{
			case Format.INT8:
			case Format.UINT8: return BYTE;

			case Format.INT16:
			case Format.UINT16: return WORD;

			case Format.INT32:
			case Format.UINT32: return DWORD;

			case Format.INT64:
			case Format.UINT64: return QWORD;

			case Format.INT128:
			case Format.UINT128: return XMMWORD;

			case Format.INT256:
			case Format.UINT256: return YMMWORD;

			case Format.DECIMAL: return Assembler.Size;

			default: throw new ArgumentException("Unknown number type given to convert");
		}
	}

	public Format ToFormat(bool unsigned = true)
	{
		return Bytes switch
		{
			1 => unsigned ? Format.UINT8 : Format.INT8,
			2 => unsigned ? Format.UINT16 : Format.INT16,
			4 => unsigned ? Format.UINT32 : Format.INT32,
			8 => unsigned ? Format.UINT64 : Format.INT64,
			16 => unsigned ? Format.UINT128 : Format.INT128,
			32 => unsigned ? Format.UINT256 : Format.INT256,
			_ => throw new ApplicationException("Could not convert size to number type"),
		};
	}

	public static bool operator ==(Size size, int bits)
	{
		return size.Bits == bits;
	}

	public static bool operator !=(Size size, int bits)
	{
		return size.Bits != bits;
	}

	public override bool Equals(object? other)
	{
		return other is Size size && Bytes == size.Bytes;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Identifier, Allocator, Bytes, Bits);
	}

	public override string ToString()
	{
		return Identifier;
	}
}