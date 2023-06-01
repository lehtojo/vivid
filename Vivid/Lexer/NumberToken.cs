using System;
using System.Globalization;
using System.Linq;

public class NumberToken : Token
{
	public object Value { get; private set; }
	public Format Format { get; private set; }
	public Position? End { get; private set; }

	private static string GetNumberPart(string text)
	{
		return new string(text.TakeWhile(i => char.IsDigit(i) || i == Lexer.DECIMAL_SEPARATOR).ToArray());
	}

	private static Format GetNumberFormat(string text)
	{
		var index = text.IndexOf(Lexer.SIGNED_TYPE_SEPARATOR);
		var unsigned = false;

		if (index == -1)
		{
			index = text.IndexOf(Lexer.UNSIGNED_TYPE_SEPARATOR);

			if (index != -1)
			{
				unsigned = true;
			}
			else
			{
				return Format.INT64;
			}
		}

		// Take all the digits, which represent the bit size
		var size = new string(text.Skip(index + 1).TakeWhile(i => char.IsDigit(i)).ToArray());

		// If digits were captured and the number can be parsed, return a format, which matches it
		if (int.TryParse(size, out int bits)) return Size.FromBytes(bits / 8).ToFormat(unsigned);

		// Return the default format
		return Size.FromBytes(Settings.Bytes).ToFormat(unsigned);
	}

	private int GetExponent(string text)
	{
		var index = text.IndexOf(Lexer.EXPONENT_SEPARATOR);
		if (index == -1) return 0;

		if (text.Length == ++index) throw new LexerException(Position, "Invalid number exponent");

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

		if (int.TryParse(exponent, out int result)) return sign * result;

		throw new LexerException(Position, "Invalid number exponent");
	}

	public NumberToken(string text, Position position) : base(TokenType.NUMBER)
	{
		Position = position;
		End = position.Translate(text.Length);

		var exponent = GetExponent(text);

		if (text.Contains(Lexer.DECIMAL_SEPARATOR))
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

			Value = value;
			Format = GetNumberFormat(text);
		}
	}

	public NumberToken(long value) : base(TokenType.NUMBER)
	{
		Value = value;
		Format = Settings.Signed;
	}

	public NumberToken(int value) : base(TokenType.NUMBER)
	{
		Value = (long)value;
		Format = Settings.Signed;
	}

	public override bool Equals(object? other)
	{
		return other is NumberToken token && base.Equals(other) && (long)Value == (long)token.Value && Format == token.Format;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Value, Format);
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
