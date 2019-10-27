using System;
using System.Collections.Generic;

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
		Value = (long)number;
		NumberType = NumberType.INT8;
		Bits = 8;
	}

	public NumberToken(short number) : base(TokenType.NUMBER)
	{
		Value = (long)number;
		NumberType = NumberType.INT16;
		Bits = 16;
	}

	public NumberToken(int number) : base(TokenType.NUMBER)
	{
		Value = (long)number;
		NumberType = NumberType.INT32;
		Bits = 32;
	}

	public NumberToken(long number) : base(TokenType.NUMBER)
	{
		Value = (long)number;
		NumberType = NumberType.INT64;
		Bits = 64;
	}
	public override bool Equals(object obj)
	{
		return obj is NumberToken token &&
			   base.Equals(obj) &&
			   (long)Value == (long)token.Value &&
			   NumberType == token.NumberType &&
			   Bits == token.Bits &&
			   Bytes == token.Bytes;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Value, NumberType, Bits, Bytes);
	}
}
