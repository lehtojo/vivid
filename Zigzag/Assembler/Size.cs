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

    public static Size? TryGetFromBytes(int bytes)
    {
        switch (bytes)
        {
            case 1: return BYTE;
            case 2: return WORD;
            case 4: return DWORD;
            case 8: return QWORD;
            case 16: return OWORD;
            case 32: return YWORD;

            default: return null;
        }
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

            case Format.DECIMAL: return Assembler.Size;

            default: throw new ArgumentException("Unknown number type given to convert");
        }
    }

    public Format ToFormat(bool unsigned = true)
    {
        switch (Bytes)
        {
            case 1: return unsigned ? Format.UINT8 : Format.INT8;
            case 2: return unsigned ? Format.UINT16 : Format.INT16;
            case 4: return unsigned ? Format.UINT32 : Format.INT32;
            case 8: return unsigned ? Format.UINT64 : Format.INT64;

            default: throw new ApplicationException("Couldn't convert size to number type");
        }
    }

    public override string ToString()
    {
        return Identifier;
    }
}