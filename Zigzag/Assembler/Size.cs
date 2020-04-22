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

    private Size(string identifier, int bytes)
    {
        Identifier = identifier;
        Bytes = bytes;
    }

    public static Size FromBytes(int bytes)
    {
        switch (bytes)
        {
            case 1: return BYTE;
            case 2: return WORD;
            case 4: return DWORD;
            case 8: return QWORD;
            case 16: return OWORD;
            case 32: return YWORD;

            default: throw new ApplicationException("Invalid instruction parameter size given");
        }
    }

    public NumberType ToNumberType(bool unsigned)
    {
        switch (Bytes)
        {
            case 1: return unsigned ? NumberType.UINT8 : NumberType.INT8;
            case 2: return unsigned ? NumberType.UINT16 : NumberType.INT16;
            case 4: return unsigned ? NumberType.UINT32 : NumberType.INT32;
            case 8: return unsigned ? NumberType.UINT64 : NumberType.INT64;

            default: throw new ApplicationException("Couldn't convert size to number type");
        }
    }

    public override string ToString()
    {
        return Identifier;
    }
}