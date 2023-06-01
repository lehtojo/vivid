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

public enum TextType
{
	UNSPECIFIED,
	TEXT,
	NUMBER,
	PARENTHESIS,
	OPERATOR,
	COMMENT,
	STRING,
	CHARACTER,
	NUMBER_WITH_BASE,
	END
}

public class TextArea
{
	public Position Start { get; set; } = new Position();
	public Position End { get; set; } = new Position();
	public TextType Type { get; set; }
	public string Text { get; set; } = string.Empty;
}

public static class Lexer
{
	public const char LINE_ENDING = '\n';
	public const char COMMENT = '#';
	public const string MULTILINE_COMMENT = "###";
	public const char STRING = '\'';
	public const char STRING_OBJECT = '\"';
	public const char CHARACTER = '`';
	public const char ESCAPER = '\\';
	public const char DECIMAL_SEPARATOR = '.';
	public const char EXPONENT_SEPARATOR = 'e';
	public const char SIGNED_TYPE_SEPARATOR = 'i';
	public const char UNSIGNED_TYPE_SEPARATOR = 'u';
	public const char BINARY_BASE_IDENTIFIER = 'b';
	public const char HEXADECIMAL_BASE_IDENTIFIER = 'x';

	/// <summary>
	/// Returns whether the specified character is an operator
	/// </summary>
	private static bool IsOperator(char i)
	{
		return (i >= '*' && i <= '/') || (i >= ':' && i <= '?') || i == '&' || i == '%' || i == '!' || i == '^' || i == '|' || i == '¤';
	}

	/// <summary>
	/// Returns all the characters which can mix with the specified character.
	/// If this function returns null, it means the specified character can mix with any character.
	/// </summary>
	private static string? GetMixingCharacters(char i)
	{
		return i switch
		{
			'.' => ".0123456789",
			',' => string.Empty,
			'<' => "|=+",
			'>' => "|=+-:",
			'*' => "=",
			_ => null
		};
	}

	/// <summary>
	/// Returns whether the two specified characters can mix
	/// </summary>
	private static bool Mixes(char a, char b)
	{
		var allowed = GetMixingCharacters(a);
		if (allowed != null) return allowed.Contains(b);

		allowed = GetMixingCharacters(b);
		if (allowed != null) return allowed.Contains(a);

		return true;
	}

	/// <summary>
	/// Returns whether the characters represent a start of a number with base
	/// </summary>
	private static bool IsNumberWithBase(char current, char next)
	{
		return current == '0' && (next == BINARY_BASE_IDENTIFIER || next == HEXADECIMAL_BASE_IDENTIFIER);
	}

	/// <summary>
	/// Returns whether the character is a digit
	/// </summary>
	private static bool IsDigit(char i)
	{
		return i >= '0' && i <= '9';
	}

	/// <summary>
	/// Returns whether the character is a text
	/// </summary>
	private static bool IsText(char i)
	{
		return (i >= 'a' && i <= 'z') || (i >= 'A' && i <= 'Z') || (i == '_');
	}

	/// <summary>
	/// Returns whether the character is start of a parenthesis
	/// </summary>
	private static bool IsParenthesis(char i)
	{
		return i == '(' || i == '[' || i == '{';
	}

	/// <summary>
	/// Returns whether the character is start of a comment
	/// </summary>
	private static bool IsComment(char i)
	{
		return i == COMMENT;
	}

	/// <summary>
	/// Returns whether the character start of a string
	/// </summary>
	private static bool IsString(char i)
	{
		return i == STRING || i == STRING_OBJECT;
	}

	/// <summary>
	/// Returns whether the character start of a character value
	/// </summary>
	private static bool IsCharacterValue(char i)
	{
		return i == CHARACTER;
	}

	/// <summary>
	/// Returns the type of the character
	/// </summary>
	private static TextType GetTextType(char current, char next)
	{
		if (IsText(current)) return TextType.TEXT;
		if (IsNumberWithBase(current, next)) return TextType.NUMBER_WITH_BASE;
		if (IsDigit(current)) return TextType.NUMBER;
		if (IsParenthesis(current)) return TextType.PARENTHESIS;
		if (IsOperator(current)) return TextType.OPERATOR;
		if (IsComment(current)) return TextType.COMMENT;
		if (IsString(current)) return TextType.STRING;
		if (IsCharacterValue(current)) return TextType.CHARACTER;
		if (current == LINE_ENDING) return TextType.END;
		return TextType.UNSPECIFIED;
	}

	/// <summary>
	/// Returns whether the character is part of the progressing token
	/// </summary>
	private static bool IsPartOf(TextType previous_type, TextType current_type, char previous, char current, char next)
	{
		if (!Mixes(previous, current)) return false;

		if (current_type == previous_type || previous_type == TextType.UNSPECIFIED) return true;

		switch (previous_type)
		{
			case TextType.TEXT: return current_type == TextType.NUMBER || current_type == TextType.NUMBER_WITH_BASE;

			case TextType.NUMBER_WITH_BASE: return current_type == TextType.NUMBER || 
				(previous == '0' && (current == BINARY_BASE_IDENTIFIER || current == HEXADECIMAL_BASE_IDENTIFIER)) ||
				(current >= 'a' && current <= 'f') || (current >= 'A' && current <= 'F');

			case TextType.NUMBER:
			{
				return (current == DECIMAL_SEPARATOR && char.IsDigit(next)) ||
					current == EXPONENT_SEPARATOR ||
					current == SIGNED_TYPE_SEPARATOR ||
					current == UNSIGNED_TYPE_SEPARATOR ||
					(previous == EXPONENT_SEPARATOR && (current == '+' || current == '-'));
			}

			default: return false;
		}
	}

	/// <summary>
	/// Skips all the spaces starting from the specified position
	/// </summary>
	private static Position SkipSpaces(string text, Position position)
	{
		while (position.Local < text.Length)
		{
			if (text[position.Local] != ' ') break;
			position.NextCharacter();
		}

		return position;
	}

	/// <summary>
	/// Finds the corresponding end parenthesis and returns its position
	/// </summary>
	private static Position SkipParenthesis(string text, Position start)
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
				position = SkipClosures(STRING, text, position, "Can not find the end of the string");
			}
			else if (current == STRING_OBJECT)
			{
				position = SkipClosures(STRING_OBJECT, text, position, "Can not find the end of the string");
			}
			else if (current == ESCAPER)
			{
				position.NextCharacter();
				position.NextCharacter();
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
		return start.Local + MULTILINE_COMMENT.Length * 2 < text.Length && text[start.Local..(start.Local + MULTILINE_COMMENT.Length)] == MULTILINE_COMMENT && text[start.Local + MULTILINE_COMMENT.Length] != COMMENT;
	}

	/// <summary>
	/// Skips the current comment and returns the position
	/// </summary>
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

			// If the 'multiline comment' is actually expressed in a single line, handle it separately
			if (last_line_ending == -1) return new Position(start.File, start.Line + lines, start.Character + comment.Length, end, start.Absolute + comment.Length);

			last_line_ending += start.Local; // The index must be relative to the whole text
			last_line_ending++; // Skip the line ending

			return new Position(start.File, start.Line + lines, end - last_line_ending, end, start.Absolute + comment.Length);
		}

		var i = text.IndexOf(LINE_ENDING, start.Local);
		var length = 0;

		if (i != -1)
		{
			length = i - start.Local;
		}
		else
		{
			length = text.Length - start.Local;
		}

		return start.Translate(length);
	}

	/// <summary>
	/// Skips closures which has the same character in both ends
	/// </summary>
	private static Position SkipClosures(char closure, string text, Position start, string error)
	{
		for (var i = start.Local + 1; i < text.Length; i++)
		{
			var current = text[i];

			if (current == closure)
			{
				var length = (i + 1) - start.Local;
				return start.Translate(length);
			}

			if (current == ESCAPER)
			{
				i++; // Skip the escaper character
				continue; /// NOTE: Escaped character will be skipped as well
			}

			if (current == LINE_ENDING) throw new LexerException(start, error);
		}

		throw new LexerException(start, error);
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
	/// Converts the specified character value text to hexadecimal character value text
	/// </summary>
	private static string? FormatSpecialCharacter(string text)
	{
		// Return if the text is empty
		if (text.Length == 0) return null;

		// Do nothing if the text does not start with an escape character
		if (text[0] != ESCAPER) return text;

		// Expect at least one character after the escape character
		if (text.Length < 2) return null;

		var command = text[1];

		// Just return the text if it is already in hexadecimal format
		if (command == 'x' || command == 'u' || command == 'U') return text;

		var value = command switch
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
			_ => null
		};

		return value != null ? value : text.Substring(1);
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
			error = "Can not understand unicode character";
		}
		else if (command == 'U')
		{
			length = 8;
			error = "Can not understand unicode character";
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
		// Remove the closures and format it so that its value can be extracted
		var formatted = FormatSpecialCharacter(text[1..^1]);

		if (formatted == null) throw new LexerException(position, "Invalid character");
		if (formatted.Length == 0) throw new LexerException(position, "Character value is empty");

		if (formatted.First() != '\\' || formatted.Length == 1)
		{
			if (formatted.Length != 1) throw new LexerException(position, "Character value allows only one character");

			return formatted.First();
		}

		if (formatted.Length <= 2) throw new LexerException(position, "Invalid character");

		return GetSpecialCharacterValue(position, formatted);
	}

	/// <summary>
	/// Returns the next token area in the text
	/// </summary>
	public static TextArea? GetNextToken(string text, Position start)
	{
		// Firstly the spaces must be skipped to find the next token
		var position = SkipSpaces(text, start);

		// Verify there is text to iterate
		if (position.Local == text.Length) return null;

		var current = text[position.Local];
		var next = position.Local + 1 < text.Length ? text[position.Local + 1] : (char)0;

		var area = new TextArea
		{
			Start = position.Clone(),
			Type = GetTextType(current, next)
		};

		switch (area.Type)
		{
			case TextType.COMMENT:
			{
				area.End = SkipComment(text, area.Start);
				area.Text = text[area.Start.Local..area.End.Local];
				return area;
			}

			case TextType.PARENTHESIS:
			{
				area.End = SkipParenthesis(text, area.Start);
				area.Text = text[area.Start.Local..area.End.Local];
				return area;
			}

			case TextType.END:
			{
				area.End = position.Clone().NextLine();
				area.Text = LINE_ENDING.ToString();
				return area;
			}

			case TextType.STRING:
			{
				area.End = SkipClosures(current, text, area.Start, "Can not find the end of the string");
				area.Text = text[area.Start.Local..area.End.Local];
				return area;
			}

			case TextType.CHARACTER:
			{
				area.End = SkipCharacterValue(text, area.Start);
				area.Type = TextType.NUMBER;

				var value = (long)GetCharacterValue(area.Start, text[area.Start.Local..area.End.Local]);
				var bits = Common.GetBits(value);

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

			// Determine what area type the current character represents
			var type = GetTextType(current, next);

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
		if (Operators.Exists(text)) return new OperatorToken(text);
		if (Keywords.Exists(text)) return new KeywordToken(text);
		return new IdentifierToken(text);
	}

	/// <summary>
	/// Parses the specified number with custom base
	/// </summary>
	private static long ParseNumberWithBase(TextArea area)
	{
		try
		{
			// Extract the base and digits from the text
			var number_base = area.Text[1];
			var digits = area.Text.Substring(2);

			// Parse the number based on its base
			if (number_base == BINARY_BASE_IDENTIFIER) return Convert.ToInt64(digits, 2);
			if (number_base == HEXADECIMAL_BASE_IDENTIFIER) return Convert.ToInt64(digits, 16);
		}
		catch {}

		throw new LexerException(area.Start, $"Can not understand the following number '{area.Text}'");
	}

	/// <summary>
	/// Parses a token from a text area
	/// </summary>
	/// <returns>Area as a token</returns>
	private static Token ParseToken(TextArea area)
	{
		return area.Type switch
		{
			TextType.TEXT => ParseTextToken(area.Text),
			TextType.NUMBER => new NumberToken(area.Text, area.Start),
			TextType.OPERATOR => new OperatorToken(Operators.Exists(area.Text) ? area.Text : throw new LexerException(area.Start, $"Unknown operator '{area.Text}'")),
			TextType.PARENTHESIS => new ParenthesisToken(area.Text, area.Start, area.End),
			TextType.END => new Token(TokenType.END),
			TextType.STRING => new StringToken(area.Text),
			TextType.NUMBER_WITH_BASE => new NumberToken(ParseNumberWithBase(area)),
			_ => throw new LexerException(area.Start, $"Unknown token '{area.Text}'"),
		};
	}

	/// <summary>
	/// Join all sequential modifier keywords into one token
	/// </summary>
	public static void JoinSequentialModifiers(List<Token> tokens)
	{
		for (var i = tokens.Count - 2; i >= 0; i--)
		{
			// Require both the current and the next tokens to be modifier keywords
			var left_token = tokens[i];
			var right_token = tokens[i + 1];

			if (left_token.Type != TokenType.KEYWORD || right_token.Type != TokenType.KEYWORD) continue;

			var left_keyword = left_token.To<KeywordToken>().Keyword;
			var right_keyword = right_token.To<KeywordToken>().Keyword;

			if (left_keyword.Type != KeywordType.MODIFIER || right_keyword.Type != KeywordType.MODIFIER) continue;

			var left_modifier = left_keyword.To<ModifierKeyword>();
			var right_modifier = right_keyword.To<ModifierKeyword>();

			// Combine the two modifiers into one token
			var identifier = left_modifier.Identifier + ' ' + right_modifier.Identifier;
			var modifiers = left_modifier.Modifier | right_modifier.Modifier;
			left_token.To<KeywordToken>().Keyword = new ModifierKeyword(identifier, modifiers);

			// Remove the right keyword, because it is combined into the left keyword
			tokens.RemoveAt(i + 1);
		}
	}

	/// <summary>
	/// Finds not-keywords and negates adjacent keywords when possible
	/// </summary>
	public static void NegateKeywords(List<Token> tokens)
	{
		for (var i = tokens.Count - 2; i >= 0; i--)
		{
			// Require the current token to be a keyword
			var token = tokens[i];
			if (token.Type != TokenType.KEYWORD) continue;

			// Require the next token to be a not-keyword
			if (!tokens[i + 1].Is(Keywords.NOT)) continue;

			var negated = (Keyword?)null;
			var keyword = token.To<KeywordToken>().Keyword;

			if (keyword == Keywords.IS)
			{
				negated = Keywords.IS_NOT;
			}
			else if (keyword == Keywords.HAS)
			{
				negated = Keywords.HAS_NOT;
			}

			if (negated != null)
			{
				token.To<KeywordToken>().Keyword = negated;
				tokens.RemoveAt(i + 1);
			}
		}
	}

	/// <summary>
	/// Postprocesses the specified tokens
	/// </summary>
	public static void Postprocess(List<Token> tokens)
	{
		JoinSequentialModifiers(tokens);
		NegateKeywords(tokens);
	}

	/// <summary>
	/// Converts all escaped characters in the specified string to escaped hexadecimal characters
	/// </summary>
	private static string PostprocessEscapedCharacters(string text)
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
				_ => null
			};

			if (value == null) continue;

			builder.Remove(i, 2);
			builder.Insert(i, value);
		}

		return builder.ToString();
	}

	/// <summary>
	/// Converts all escaped characters in string tokens to escaped hexadecimal characters
	/// </summary>
	private static void PostprocessEscapedCharacters(List<Token> tokens)
	{
		foreach (var token in tokens)
		{
			if (token.Type == TokenType.STRING)
			{
				token.To<StringToken>().Text = PostprocessEscapedCharacters(token.To<StringToken>().Text);
				continue;
			}

			if (token.Type == TokenType.PARENTHESIS)
			{
				PostprocessEscapedCharacters(token.To<ParenthesisToken>().Tokens);
			}
		}
	}

	/// <summary>
	/// Returns the text as a token list
	/// </summary>
	/// <returns>Text as a token list</returns>
	public static List<Token> GetTokens(string text, bool postprocess = true)
	{
		return GetTokens(text, new Position(), postprocess);
	}

	/// <summary>
	/// Returns the text as a token list
	/// </summary>
	public static List<Token> GetTokens(string text, Position anchor, bool postprocess = true)
	{
		var tokens = new List<Token>();
		var position = new Position(anchor.File, anchor.Line, anchor.Character, 0, anchor.Absolute);

		while (position.Local < text.Length)
		{
			var area = GetNextToken(text, position.Clone());
			if (area == null) break;

			if (area.Type != TextType.COMMENT)
			{
				var token = ParseToken(area);
				token.Position = area.Start;
				tokens.Add(token);
			}

			position = area.End;
		}

		PostprocessEscapedCharacters(tokens);

		if (postprocess) Postprocess(tokens);
		
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

			if (token.Is(TokenType.PARENTHESIS))
			{
				RegisterFile(token.To<ParenthesisToken>().Tokens, file);
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
