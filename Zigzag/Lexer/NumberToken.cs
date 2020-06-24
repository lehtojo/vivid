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

		if (int.TryParse(text.Skip(index + 1).TakeWhile(c => char.IsDigit(c)).ToString(), out int result))
		{
			bits = result;
		}
		else
		{
			bits = Lexer.Size.Bits;
		}
	}

	private static int GetExponent(string text)
	{
		var index = text.IndexOf(Lexer.EXPONENT_SEPARATOR);

		if (index == -1)
		{
			return 0;
		}
		else if (int.TryParse(text.Skip(index + 1).TakeWhile(c => char.IsDigit(c)).ToString(), out int result))
		{
			return result;
		}
		else
		{
			throw new ApplicationException($"Invalid number exponent: '{text}'");
		}
	}

	public NumberToken(string text) : base(TokenType.NUMBER)
	{
		var exponent = GetExponent(text);

		if (IsDecimal(text))
		{
			// Calculate the value
			var number_part = GetNumberPart(text).Replace(".", ",");
			var value = double.Parse(number_part, CultureInfo.InvariantCulture);

			/// TODO: Detect too large exponent
			value *= Math.Pow(10, exponent);

<<<<<<< HEAD
         /// TODO: Think about overriding the decimal type
         Value = value;
=======
			/// TODO: Think about overriding the decimal type
			Value = value;
>>>>>>> ec8e325... Improved code quality and implemented basic support for operator overloading
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
			NumberType = Size.TryGetFromBytes(bits / 8)?.ToFormat(unsigned) ?? throw new ApplicationException($"Invalid number format: '{text}'");
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

	public override object Clone()
	{
		return MemberwiseClone();
	}
}
