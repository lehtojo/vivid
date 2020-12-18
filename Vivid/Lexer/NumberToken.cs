using System;
using System.Globalization;
using System.Linq;

public class NumberToken : Token
{
	public object Value { get; private set; }
	public Format NumberType { get; private set; }
	public int Bits { get; private set; }
	public int Bytes => Bits / 8;

	private static bool IsDecimal(string text)
	{
		return text.Contains('.');
	}

	private static string GetNumberPart(string text)
	{
		return new string(text.TakeWhile(c => char.IsDigit(c) || c == Lexer.DECIMAL_SEPARATOR).ToArray());
	}

	private static void GetType(string text, out int bits, out bool unsigned)
	{
		var index = text.IndexOf(Lexer.SIGNED_TYPE_SEPARATOR);

		if (index != -1)
		{
			unsigned = false;
		}
		else
		{
			index = text.IndexOf(Lexer.UNSIGNED_TYPE_SEPARATOR);

			if (index != -1)
			{
				unsigned = true;
			}
			else
			{
				unsigned = false;
				bits = Lexer.Size.Bits;
				return;
			}
		}

		var size = new string(text.Skip(index + 1).TakeWhile(c => char.IsDigit(c)).ToArray());

		if (int.TryParse(size, out int result))
		{
			bits = result;
		}
		else
		{
			bits = Lexer.Size.Bits;
		}
	}

	private int GetExponent(string text)
	{
		var index = text.IndexOf(Lexer.EXPONENT_SEPARATOR);

		if (index == -1)
		{
			return 0;
		}

		var exponent = new string(text.Skip(index + 1).TakeWhile(c => char.IsDigit(c)).ToArray());

		if (int.TryParse(exponent, out int result))
		{
			return result;
		}
		else
		{
			throw new LexerException(Position, $"Invalid number exponent: '{text}'");
		}
	}

	public NumberToken(string text, Position position) : base(TokenType.NUMBER)
	{
		Position = position;

		var exponent = GetExponent(text);

		if (IsDecimal(text))
		{
			// Calculate the value
			var number_part = GetNumberPart(text);
			var value = double.Parse(number_part, CultureInfo.InvariantCulture);

			/// TODO: Detect too large exponent
			value *= Math.Pow(10, exponent);

			Value = value;
			NumberType = Format.DECIMAL;
			Bits = Lexer.Size.Bytes * 8;
		}
		else
		{
			// Calculate the value
			var value = long.Parse(GetNumberPart(text), CultureInfo.InvariantCulture);

			/// TODO: Detect too large exponent
			value *= (long)Math.Pow(10, exponent);

			// Get the format of the number
			GetType(text, out int bits, out bool unsigned);

			Value = value;
			NumberType = Size.TryGetFromBytes(bits / 8)?.ToFormat(unsigned) ?? throw new LexerException(Position, $"Invalid number format: '{text}'");
			Bits = bits;
		}
	}

	public NumberToken(int number) : base(TokenType.NUMBER)
	{
		Value = (long)number;
		NumberType = Lexer.Size.ToFormat(false);
		Bits = Lexer.Size.Bytes * 8;
	}

	public NumberToken(double number) : base(TokenType.NUMBER)
	{
		Value = number;
		NumberType = Format.DECIMAL;
		Bits = Lexer.Size.Bytes * 8;
	}

	public override bool Equals(object? other)
	{
		return other is NumberToken token &&
			   base.Equals(other) &&
			   (long)Value == (long)token.Value &&
			   NumberType == token.NumberType &&
			   Bits == token.Bits &&
			   Bytes == token.Bytes;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Value, NumberType, Bits, Bytes);
	}

	public override object Clone()
	{
		return MemberwiseClone();
	}
}
