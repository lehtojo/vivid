using System;

public class NumberToken : Token
{
	public object Value { get; private set; }
	public NumberType NumberType { get; private set; }
	public int Bits { get; private set; }
	public int Bytes => Bits / 8;

	private bool IsDecimal(string text)
	{
		return text.Contains('.');
	}

	public NumberToken(string text) : base(TokenType.NUMBER)
	{
		if (IsDecimal(text))
		{
			Value = double.Parse(text.Replace('.', ','));
			NumberType = NumberType.DECIMAL32;
		}
		else
		{
			Value = long.Parse(text);
			NumberType = NumberType.INT32;
		}
		
		Bits = 32;
	}

	public NumberToken(int number) : base(TokenType.NUMBER)
	{
		Value = (long)number;
		NumberType = NumberType.INT32;
		Bits = 32;
	}

	public NumberToken(double number) : base(TokenType.NUMBER)
	{
		Value = number;
		NumberType = NumberType.DECIMAL32;
		Bits = 32;
	}

	public override bool Equals(object? obj)
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
