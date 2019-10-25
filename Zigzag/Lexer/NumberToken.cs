using System;

public class NumberToken : Token
{
	public object Value { get; private set; }
	public NumberType NumberType { get; private set; }
	public int Bits { get; private set; }
	public int Bytes => Bits / 8;

	public NumberToken(string text) : base(TokenType.NUMBER)
	{
		Value = long.Parse(text);
		NumberType = NumberType.INT32;
		Bits = 32;
	}

	public NumberToken(byte number) : base(TokenType.NUMBER)
	{
		Value = number;
		NumberType = NumberType.INT8;
	}

	public NumberToken(short number) : base(TokenType.NUMBER)
	{
		Value = number;
		NumberType = NumberType.INT16;
	}

	public NumberToken(int number) : base(TokenType.NUMBER)
	{
		Value = number;
		NumberType = NumberType.INT32;
	}

	public NumberToken(long number) : base(TokenType.NUMBER)
	{
		Value = number;
		NumberType = NumberType.INT64;
	}
}
