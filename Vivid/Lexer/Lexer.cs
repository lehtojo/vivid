using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Linq;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1032", Justification = "Lexer exception should not be constructed without a position")]
public class LexerException : Exception
{
	public Position Position { get; private set; }
	public string Description { get; private set; }

	public LexerException(Position position, string description)
	{
		Position = position;
		Description = description;
	}
}

public enum AreaType
{
	UNSPECIFIED,
	TEXT,
	COMMENT,
	NUMBER,
	CONTENT,
	OPERATOR,
	STRING,
	CHARACTER,
	HEXADECIMAL,
	END
}

public class Area
{
	public AreaType Type { get; set; }

	public string Text { get; set; } = string.Empty;

	public Position Start { get; set; } = new Position();
	public Position End { get; set; } = new Position();
}

public static class Lexer
{
	public static Size Size { get; set; } = Size.QWORD;

	public const string POSITIVE_INFINITY_CONSTANT = "POSITIVE_INFINITY";
	public const string NEGATIVE_INFINITY_CONSTANT = "NEGATIVE_INFINITY";

	public const char LINE_ENDING = '\n';
	public const char COMMENT = '#';
	public const string MULTILINE_COMMENT = "###";
	public const char STRING = '\'';
	public const char CHARACTER = '`';
	public const char DECIMAL_SEPARATOR = '.';
	public const char EXPONENT_SEPARATOR = 'e';
	public const char SIGNED_TYPE_SEPARATOR = 'i';
	public const char UNSIGNED_TYPE_SEPARATOR = 'u';

	/// <summary>
	/// Returns whether the character is a operator
	/// </summary>
	/// <returns>True if the character is a operator, otherwise false</returns>
	private static bool IsOperator(char c)
	{
		return c >= 33 && c <= 47 && c != COMMENT && c != STRING || c >= 58 && c <= 63 || c == 94 || c == 124 || c == 126 || c == '¤';
	}

	/// <summary>
	/// Returns all the characters which can mix with the specified character.
	/// If this function returns null, it means the specified character can mix with any character.
	/// </summary>
	private static string? GetMixingCharacters(char c)
	{
		return c switch
		{
			'.' => ".0123456789",
			',' => string.Empty,
			'<' => "|=",
			'>' => "|=-:",
			_ => null
		};
	}

	/// <summary>
	/// Returns whether the two specified characters can mix
	/// </summary>
	private static bool Mixes(char a, char b)
	{
		var x = GetMixingCharacters(a);
		if (x != null) return x.Contains(b);

		var y = GetMixingCharacters(b);
		if (y != null) return y.Contains(a);

		return true;
	}

	/// <summary>
	/// Returns whether the characters represent a start of a hexadecimal number
	/// </summary>
	private static bool IsStartOfHexadecimal(char current, char next)
	{
		return current == '0' && next == 'x';
	}

	/// <summary>
	/// Returns whether the character is a digit
	/// </summary>
	/// <returns>True if the character is a digit, otherwise false</returns>
	private static bool IsDigit(char c)
	{
		return c >= 48 && c <= 57;
	}

	/// <summary>
	/// Returns whether the character is a text
	/// </summary>
	/// <returns>True if the character is a text, otherwise false</returns>
	private static bool IsText(char c)
	{
		return c >= 65 && c <= 90 || c >= 97 && c <= 122 || c == 95;
	}

	/// <summary>
	/// Returns whether the character is start of a parenthesis
	/// </summary>
	/// <returns>True if the character is start of a parenthesis, otherwise false</returns>
	private static bool IsContent(char c)
	{
		return ParenthesisType.Has(c);
	}

	/// <summary>
	/// Returns whether the character is start of a comment
	/// </summary>
	/// <returns>True if the character is start of a comment, otherwise false</returns>
	private static bool IsComment(char c)
	{
		return c == COMMENT;
	}

	/// <summary>
	/// Returns whether the character start of a string
	/// </summary>
	/// <returns>True if the character is start of a string, otherwise false</returns>
	private static bool IsString(char c)
	{
		return c == STRING;
	}

	/// <summary>
	/// Returns whether the character start of a character value
	/// </summary>
	/// <returns>True if the character is start of a character value, otherwise false</returns>
	private static bool IsCharacterValue(char c)
	{
		return c == CHARACTER;
	}

	/// <summary>
	/// Returns the type of the character
	/// </summary>
	/// <returns>Type of the character</returns>
	private static AreaType GetType(char current, char next)
	{
		if (IsText(current)) return AreaType.TEXT;
		if (IsStartOfHexadecimal(current, next)) return AreaType.HEXADECIMAL;
		if (IsDigit(current)) return AreaType.NUMBER;
		if (IsContent(current)) return AreaType.CONTENT;
		if (IsOperator(current)) return AreaType.OPERATOR;
		if (IsComment(current)) return AreaType.COMMENT;
		if (IsString(current)) return AreaType.STRING;
		if (IsCharacterValue(current)) return AreaType.CHARACTER;
		if (current == LINE_ENDING) return AreaType.END;

		return AreaType.UNSPECIFIED;
	}

	/// <summary>
	/// Returns whether the character is part of the progressing token
	/// </summary>
	/// <returns>True if the character is part of the progressing token</returns>
	private static bool IsPartOf(AreaType previous_type, AreaType current_type, char previous, char current, char next)
	{
		if (!Mixes(previous, current)) return false;

		if (current_type == previous_type || previous_type == AreaType.UNSPECIFIED) return true;

		switch (previous_type)
		{
			case AreaType.TEXT: return current_type == AreaType.NUMBER;

			case AreaType.HEXADECIMAL: return current_type == AreaType.NUMBER || 
				(previous == '0' && current == 'x') ||
				(current >= 'a' && current <= 'f') || (current >= 'A' && current <= 'F');

			case AreaType.NUMBER:
			{
				return (current == DECIMAL_SEPARATOR && char.IsDigit(next)) || // Example: 7.0
					current == EXPONENT_SEPARATOR || // Example: 100e0
					current == SIGNED_TYPE_SEPARATOR || // Example 0i8
					current == UNSIGNED_TYPE_SEPARATOR || // Example 0u32
					(previous == EXPONENT_SEPARATOR && (current == '+' || current == '-')); // Examples: 3.14159e+10, 10e-10
			}

			default: return false;
		}
	}

	/// <summary>
	/// Skips all the spaces starting from the given position
	/// </summary>
	/// <returns>Returns the position after the spaces</returns>
	private static Position SkipSpaces(string text, Position position)
	{
		while (position.Local < text.Length)
		{
			var current = text[position.Local];
			if (current != ' ') break;

			position.NextCharacter();
		}

		return position;
	}

	/// <summary>
	/// Finds the corresponding end parenthesis and returns its position
	/// </summary>
	/// <returns>Position of the closing parenthesis</returns>
	private static Position SkipContent(string text, Position start)
	{
		var position = start.Clone();

		var opening = text[position.Local];
		var closing = ParenthesisType.Get(opening).Closing;

		var count = 0;

		while (position.Local < text.Length)
		{
			var current = text[position.Local];

			if (current == LINE_ENDING)
			{
				position.NextLine();
			}
			else if (current == COMMENT)
			{
				position = SkipComment(text, position);
			}
			else if (current == STRING)
			{
				position = SkipString(text, position);
			}
			else if (current == CHARACTER)
			{
				position = SkipCharacterValue(text, position);
			}
			else
			{
				if (current == opening) { count++; }
				else if (current == closing) { count--; }

				position.NextCharacter();
			}

			if (count == 0) return position;
		}

		throw new LexerException(start, "Can not find closing parenthesis");
	}

	/// <summary>
	/// Returns whether a multiline comment begins at the specified position
	/// </summary>
	private static bool IsMultilineComment(string text, Position start)
	{
		return start.Local + MULTILINE_COMMENT.Length * 2 + 1 <= text.Length && text[start.Local..(start.Local + MULTILINE_COMMENT.Length)] == MULTILINE_COMMENT && text[start.Local + MULTILINE_COMMENT.Length] != COMMENT;
	}

	/// <summary>
	/// Skips the current comment and returns the position
	/// </summary>
	/// <returns>Position after the comment</returns>
	private static Position SkipComment(string text, Position start)
	{
		if (IsMultilineComment(text, start))
		{
			var end = text.IndexOf(MULTILINE_COMMENT, start.Local + MULTILINE_COMMENT.Length);
			if (end == -1) throw new LexerException(start, $"Multiline comment did not have a closing '{MULTILINE_COMMENT}'");

			// Skip to the end of the multiline comment
			end += MULTILINE_COMMENT.Length;

			// Count how many line endings there are inside the comment
			var comment = text[start.Local..end];
			var lines = comment.Count(i => i == LINE_ENDING);

			// Determine the index of the last line ending inside the comment
			var last_line_ending = comment.LastIndexOf(LINE_ENDING);

			// Check if the comment is a multiline comment
			if (last_line_ending != -1)
			{
				last_line_ending += start.Local; // The index must be relative to the whole text
				last_line_ending++; // Skip the line ending

				return new Position(start.Line + lines, end - last_line_ending, end, start.Absolute + comment.Length);
			}

			/// NOTE: The comment is a single-line comment
			return new Position(start.Line + lines, start.Character + comment.Length, end, start.Absolute + comment.Length);
		}

		var i = text.IndexOf(LINE_ENDING, start.Local);

		if (i != -1)
		{
			var length = i - start.Local;
			return new Position(start.Line, start.Character + length, start.Local + length, start.Absolute + length);
		}
		else
		{
			var length = text.Length - start.Local;
			return new Position(start.Line, start.Character + length, start.Local + length, start.Absolute + length);
		}
	}

	/// <summary>
	/// Skips closures which has the same character in both ends
	/// </summary>
	/// <returns>Position after the closures</returns>
	private static Position SkipClosures(char closure, string text, Position start, string error)
	{
		var i = text.IndexOf(closure, start.Local + 1);
		var j = text.IndexOf(LINE_ENDING, start.Local + 1);

		if (i == -1 || j != -1 && j < i)
		{
			throw new LexerException(start, error);
		}

		var length = i + 1 - start.Local;

		return new Position(start.Line, start.Character + length, start.Local + length, start.Absolute + length);
	}

	/// <summary>
	/// Skips the current string and returns the position
	/// </summary>
	/// <returns>Position after the string</returns>
	private static Position SkipString(string text, Position start)
	{
		return SkipClosures(STRING, text, start, "Can not find the end of the string");
	}

	/// <summary>
	/// Skips the current character value and returns the position
	/// </summary>
	/// <returns>Position after the character value</returns>
	private static Position SkipCharacterValue(string text, Position start)
	{
		return SkipClosures(CHARACTER, text, start, "Can not find the end of the character value");
	}

	/// <summary>
	/// Returns the integer value of the special character
	/// </summary>
	private static ulong GetSpecialCharacterValue(Position position, string text)
	{
		var command = text[1];
		var length = 0;
		var error = string.Empty;

		if (command == 'x')
		{
			length = 2;
			error = "Can not understand hexadecimal value";
		}
		else if (command == 'u')
		{
			length = 4;
			error = "Can not understand Unicode character";
		}
		else if (command == 'U')
		{
			length = 8;
			error = "Can not understand Unicode character";
		}
		else
		{
			throw new LexerException(position, $"Can not understand string command '{command}'");
		}

		var hexadecimal = text[2..];

		if (hexadecimal.Length != length) throw new LexerException(position, "Invalid character");
		if (!ulong.TryParse(hexadecimal, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong value)) throw new LexerException(position, error);

		return value;
	}

	/// <summary>
	/// Returns the integer value of the character value
	/// </summary>
	private static ulong GetCharacterValue(Position position, string text)
	{
		// Remove the closures
		text = text[1..^1];

		if (text.Length == 0) throw new LexerException(position, "Character value is empty");

		if (text.First() != '\\')
		{
			if (text.Length != 1) throw new LexerException(position, "Character value allows only one character");

			return text.First();
		}

		if (text.Length == 2 && text[1] == '\\') return (ulong)'\\';
		if (text.Length <= 2) throw new LexerException(position, "Invalid character");

		return GetSpecialCharacterValue(position, text);
	}

	/// <summary>
	/// Returns how many bits the specified number requires
	/// </summary>
	public static int GetBits(object value)
	{
		if (value is double) return Lexer.Size.Bits;

		var x = (long)value;

		if (x < 0)
		{
			if (x < int.MinValue) return 64;
			else if (x < short.MinValue) return 32;
			else if (x < byte.MinValue) return 16;
		}
		else
		{
			if (x > int.MaxValue) return 64;
			else if (x > short.MaxValue) return 32;
			else if (x > byte.MaxValue) return 16;
		}

		return 8;
	}

	/// <summary>
	/// Returns the next token area in the text
	/// </summary>
	/// <returns>The next token in the text</returns>
	public static Area? GetNextToken(string text, Position start)
	{
		// Firstly the spaces must be skipped to find the next token
		var position = SkipSpaces(text, start);

		// Verify there is text to iterate
		if (position.Local == text.Length) return null;

		var current = text[position.Local];
		var next = position.Local + 1 < text.Length ? text[position.Local + 1] : (char)0;

		var area = new Area
		{
			Start = position.Clone(),
			Type = GetType(current, next)
		};

		switch (area.Type)
		{
			case AreaType.COMMENT:
			{
				area.End = SkipComment(text, area.Start);
				area.Text = text[area.Start.Local..area.End.Local];
				return area;
			}

			case AreaType.CONTENT:
			{
				area.End = SkipContent(text, area.Start);
				area.Text = text[area.Start.Local..area.End.Local];
				return area;
			}

			case AreaType.END:
			{
				area.End = position.Clone().NextLine();
				area.Text = LINE_ENDING.ToString();
				return area;
			}

			case AreaType.STRING:
			{
				area.End = SkipString(text, area.Start);
				area.Text = text[area.Start.Local..area.End.Local];
				return area;
			}

			case AreaType.CHARACTER:
			{
				area.End = SkipCharacterValue(text, area.Start);
				area.Type = AreaType.NUMBER;

				var value = (long)GetCharacterValue(area.Start, text[area.Start.Local..area.End.Local]);
				var bits = GetBits(value);
				
				area.Text = value.ToString() + SIGNED_TYPE_SEPARATOR + bits.ToString();
				return area;
			}
		}

		position.NextCharacter();

		// Possible types are now: TEXT, NUMBER, OPERATOR
		while (position.Local < text.Length)
		{
			var previous = current;
			current = next;
			next = position.Local + 1 < text.Length ? text[position.Local + 1] : (char)0;

			if (IsContent(current)) break;

			// Determine what area type the current character represents
			var type = GetType(current, next);

			if (!IsPartOf(area.Type, type, previous, current, next)) break;

			position.NextCharacter();
		}

		area.End = position;
		area.Text = text[area.Start.Local..area.End.Local];

		return area;
	}

	/// <summary>
	/// Parses a token from text
	/// </summary>
	/// <returns>Text as a token</returns>
	private static Token ParseTextToken(string text)
	{
		if (Operators.Exists(text))
		{
			return new OperatorToken(text);
		}
		else if (Keywords.Exists(text))
		{
			return new KeywordToken(text);
		}
		else
		{
			return new IdentifierToken(text);
		}
	}

	/// <summary>
	/// Parses the specified hexadecimal
	/// </summary>
	private static long ParseHexadecimal(Area area)
	{
		if (long.TryParse(area.Text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value)) return value;

		throw new LexerException(area.Start, $"Can not understand the hexadecimal '{area.Text}'");
	}

	/// <summary>
	/// Parses a token from a text area
	/// </summary>
	/// <returns>Area as a token</returns>
	private static Token ParseToken(Area area)
	{
		return area.Type switch
		{
			AreaType.TEXT => ParseTextToken(area.Text),
			AreaType.NUMBER => new NumberToken(area.Text, area.Start),
			AreaType.OPERATOR => new OperatorToken(Operators.Exists(area.Text) ? area.Text : throw new LexerException(area.Start, $"Unknown operator '{area.Text}'")),
			AreaType.CONTENT => new ContentToken(area.Text, area.Start, area.End),
			AreaType.END => new Token(TokenType.END),
			AreaType.STRING => new StringToken(area.Text),
			AreaType.HEXADECIMAL => new NumberToken(ParseHexadecimal(area)),
			_ => throw new LexerException(area.Start, $"Unknown token '{area.Text}'"),
		};
	}

	/// <summary>
	/// Join all sequential modifier keywords into one token
	/// </summary>
	public static void Join(List<Token> tokens)
	{
		if (tokens.Count == 1) return;

		for (var i = tokens.Count - 2; i >= 0; i--)
		{
			var current = tokens[i];
			var next = tokens[i + 1];

			if (current is KeywordToken a && next is KeywordToken b)
			{
				if (a.Keyword is ModifierKeyword x && b.Keyword is ModifierKeyword y)
				{
					var identifier = x.Identifier + ' ' + y.Identifier;
					var combined = new ModifierKeyword(identifier, x.Modifier | y.Modifier);

					tokens.RemoveAt(i);
					tokens.RemoveAt(i);
					tokens.Insert(i, new KeywordToken(combined) { Position = a.Position });
					continue;
				}
				
				if (a.Keyword != Keywords.IS || b.Keyword != Keywords.NOT)
				{
					continue;
				}

				tokens.RemoveAt(i);
				tokens.RemoveAt(i);
				tokens.Insert(i, new KeywordToken(Keywords.IS_NOT) { Position = a.Position });
			}
		}
	}

	/// <summary>
	/// Converts all special characters in the text to use the hexadecimal character format
	/// </summary>
	private static string PreprocessSpecialCharacters(string text)
	{
		var builder = new StringBuilder(text);

		// Start from the second last character and look for special characters
		for (var i = builder.Length - 2; i >= 0; i--)
		{
			// Special characters start with '\\'
			if (builder[i] != '\\') continue;

			// Skip occurrences where there are two sequential '\\'
			// Example: '\\\\n' = \n != '\\\x0A'
			if (i - 1 >= 0 && builder[i - 1] == '\\') continue;

			var value = builder[i + 1] switch
			{
				'a' => "\\x07",
				'b' => "\\x08",
				't' => "\\x09",
				'n' => "\\x0A",
				'v' => "\\x0B",
				'f' => "\\x0C",
				'r' => "\\x0D",
				'e' => "\\x1B",
				'\"' => "\\x22",
				'\'' => "\\x27",
				_ => string.Empty
			};

			if (string.IsNullOrEmpty(value)) continue;

			builder.Remove(i, 2);
			builder.Insert(i, value);
		}

		return builder.ToString();
	}

	/// <summary>
	/// Returns the text as a token list
	/// </summary>
	/// <returns>Text as a token list</returns>
	public static List<Token> GetTokens(string text, bool join = true)
	{
		return GetTokens(text, new Position(), join);
	}

	/// <summary>
	/// Returns the text as a token list
	/// </summary>
	/// <returns>Text as a token list</returns>
	public static List<Token> GetTokens(string text, Position anchor, bool join = true)
	{
		text = PreprocessSpecialCharacters(text);

		var tokens = new List<Token>();
		var position = new Position(anchor.Line, anchor.Character, 0, anchor.Absolute);

		while (position.Local < text.Length)
		{
			var area = GetNextToken(text, position.Clone());
			if (area == null) break;

			if (area.Type != AreaType.COMMENT)
			{
				var token = ParseToken(area);
				token.Position = area.Start;
				tokens.Add(token);
			}

			position = area.End;
		}

		if (join) Join(tokens);
		
		return tokens;
	}

	/// <summary>
	/// Ensures all the tokens have a reference to the specified file
	/// </summary>
	public static void RegisterFile(List<Token> tokens, SourceFile file)
	{
		foreach (var token in tokens)
		{
			token.Position.File = file;

			if (token.Is(TokenType.CONTENT))
			{
				RegisterFile(token.To<ContentToken>().Tokens, file);
			}
			else if (token.Is(TokenType.FUNCTION))
			{
				var function = token.To<FunctionToken>();
				function.Identifier.Position.File = file;
				function.Parameters.Position.File = file;

				RegisterFile(function.Parameters.Tokens, file);
			}
		}
	}
}
