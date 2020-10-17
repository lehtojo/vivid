using System;

public class Size
{
	public static readonly Size NONE = new Size("?", 0);
	public static readonly Size BYTE = new Size("byte", 1);
	public static readonly Size WORD = new Size("word", 2);
	public static readonly Size DWORD = new Size("dword", 4);
	public static readonly Size QWORD = new Size("qword", 8);
	public static readonly Size OWORD = new Size("oword", 16);
	public static readonly Size YWORD = new Size("yword", 32);

	public string Identifier { get; private set; }
	public string Allocator => "d" + Identifier[0];
	public int Bytes { get; private set; }
	public int Bits => Bytes * 8;

	private Size(string identifier, int bytes)
	{
		Identifier = identifier;
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
			16 => OWORD,
			32 => YWORD,
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
			16 => OWORD,
			32 => YWORD,
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
			case Format.UINT128: return OWORD;

			case Format.INT256:
			case Format.UINT256: return YWORD;

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

	public override string ToString()
	{
		return Identifier;
	}
}