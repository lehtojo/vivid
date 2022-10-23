using System;
using System.Globalization;
using System.Linq;

public class NumberToken : Token
{
	public object Value { get; private set; }
	public Format Format { get; private set; }
	public int Bits { get; private set; }
	public Position? End { get; private set; }
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

		if (text.Length == ++index)
		{
			throw new LexerException(Position, "Invalid number exponent");
		}

		var sign = 1;

		if (text[index] == '-')
		{
			sign = -1;
			index++;
		}
		else if (text[index] == '+')
		{
			index++;
		}

		var exponent = new string(text.Skip(index).TakeWhile(i => char.IsDigit(i)).ToArray());

		if (int.TryParse(exponent, out int result))
		{
			return sign * result;
		}

		throw new LexerException(Position, "Invalid number exponent");
	}

	public NumberToken(string text, Position position) : base(TokenType.NUMBER)
	{
		Position = position;
		End = position.Translate(text.Length);

		var exponent = GetExponent(text);

		if (IsDecimal(text))
		{
			// Calculate the value
			if (!double.TryParse(GetNumberPart(text), NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
			{
				throw new LexerException(position, "Can not resolve the number");
			}

			// Apply the exponent to the value
			value *= Math.Pow(10, exponent);

			if (double.IsInfinity(value))
			{
				throw new LexerException(position, $"Decimal number approaches infinity. Use the constants {Settings.POSITIVE_INFINITY_CONSTANT} or {Settings.NEGATIVE_INFINITY_CONSTANT} instead.");
			}

			Value = value;
			Format = Format.DECIMAL;
			Bits = Lexer.Size.Bytes * 8;
		}
		else
		{
			// Calculate the value
			if (!long.TryParse(GetNumberPart(text), NumberStyles.Float, CultureInfo.InvariantCulture, out long value))
			{
				throw new LexerException(position, "Can not resolve the number");
			}

			// Apply the exponent to the value
			for (var i = 0; i < exponent; i++)
			{
				var previous = value;
				value *= 10;

				if (value <= previous) throw new LexerException(position, "Too large or too small integer");
			}

			// Get the format of the number
			GetType(text, out int bits, out bool unsigned);

			Value = value;
			Format = Size.TryGetFromBytes(bits / 8)?.ToFormat(unsigned) ?? throw new LexerException(Position, $"Invalid number format");
			Bits = bits;
		}
	}

	public NumberToken(long number) : base(TokenType.NUMBER)
	{
		Value = number;
		Format = Lexer.Size.ToFormat(false);
		Bits = Lexer.Size.Bytes * 8;
	}

	public NumberToken(int number) : base(TokenType.NUMBER)
	{
		Value = (long)number;
		Format = Lexer.Size.ToFormat(false);
		Bits = Lexer.Size.Bytes * 8;
	}

	public override bool Equals(object? other)
	{
		return other is NumberToken token &&
			   base.Equals(other) &&
			   (long)Value == (long)token.Value &&
			   Format == token.Format &&
			   Bits == token.Bits &&
			   Bytes == token.Bytes;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Value, Format, Bits, Bytes);
	}

	public override object Clone()
	{
		return MemberwiseClone();
	}

	public override string ToString()
	{
		return Value.ToString() ?? string.Empty;
	}
}
