using System;
using System.Collections.Generic;
using System.Globalization;
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
	/// <param name="c">Character to scan</param>
	/// <returns>True if the character is a operator, otherwise false</returns>
	private static bool IsOperator(char c)
	{
		return c >= 33 && c <= 47 && c != COMMENT && c != STRING || c >= 58 && c <= 63 || c == 94 || c == 124 || c == 126 || c == '¤';
	}

	/// <summary>
	/// Returns whether the character is an independent operator
	/// </summary>
	/// <param name="c">Character to scan</param>
	/// <returns>True if the character is an independent operator, otherwise false</returns>
	private static bool IsIndependentOperator(char c)
	{
		return c == '<' || c == '>' || c == ',';
	}

	/// <summary>
	/// Returns whether the character is a digit
	/// </summary>
	/// <param name="c">Character to scan</param>
	/// <returns>True if the character is a digit, otherwise false</returns>
	private static bool IsDigit(char c)
	{
		return c >= 48 && c <= 57;
	}

	/// <summary>
	/// Returns whether the character is a text
	/// </summary>
	/// <param name="c">Character to scan</param>
	/// <returns>True if the character is a text, otherwise false</returns>
	private static bool IsText(char c)
	{
		return c >= 65 && c <= 90 || c >= 97 && c <= 122 || c == 95;
	}

	/// <summary>
	/// Returns whether the character is start of a parenthesis
	/// </summary>
	/// <param name="c">Character to scan</param>
	/// <returns>True if the character is start of a parenthesis, otherwise false</returns>
	private static bool IsContent(char c)
	{
		return ParenthesisType.Has(c);
	}

	/// <summary>
	/// Returns whether the character is start of a comment
	/// </summary>
	/// <param name="c">Character to scan</param>
	/// <returns>True if the character is start of a comment, otherwise false</returns>
	private static bool IsComment(char c)
	{
		return c == COMMENT;
	}

	/// <summary>
	/// Returns whether the character start of a string
	/// </summary>
	/// <param name="c">Character to scan</param>
	/// <returns>True if the character is start of a string, otherwise false</returns>
	private static bool IsString(char c)
	{
		return c == STRING;
	}

	/// <summary>
	/// Returns whether the character start of a character value
	/// </summary>
	/// <param name="c">Character to scan</param>
	/// <returns>True if the character is start of a character value, otherwise false</returns>
	private static bool IsCharacterValue(char c)
	{
		return c == CHARACTER;
	}

	/// <summary>
	/// Returns the type of the character
	/// </summary>
	/// <param name="c">Character to scan</param>
	/// <returns>Type of the character</returns>
	private static AreaType GetType(char c)
	{
		if (IsText(c))
		{
			return AreaType.TEXT;
		}
		else if (IsDigit(c))
		{
			return AreaType.NUMBER;
		}
		else if (IsContent(c))
		{
			return AreaType.CONTENT;
		}
		else if (IsOperator(c))
		{
			return AreaType.OPERATOR;
		}
		else if (IsComment(c))
		{
			return AreaType.COMMENT;
		}
		else if (IsString(c))
		{
			return AreaType.STRING;
		}
		else if (IsCharacterValue(c))
		{
			return AreaType.CHARACTER;
		}
		else if (c == LINE_ENDING)
		{
			return AreaType.END;
		}

		return AreaType.UNSPECIFIED;
	}

	/// <summary>
	/// Returns whether the character is part of the progressing token
	/// </summary>
	/// <param name="previous">Type of the progressing token</param>
	/// <param name="current">Type of the current character</param>
	/// <param name="previous_symbol">Previous character</param>
	/// <param name="current_symbol">Current character</param>
	/// <returns>True if the character is part of the progressing token</returns>
	private static bool IsPartOf(AreaType previous, AreaType current, char previous_symbol, char current_symbol, char next_symbol)
	{
		if (IsIndependentOperator(previous_symbol) && IsIndependentOperator(current_symbol))
		{
			return false;
		}

		if (current == previous || previous == AreaType.UNSPECIFIED)
		{
			return true;
		}

		switch (previous)
		{
			case AreaType.TEXT:
			{
				return current == AreaType.NUMBER;
			}

			case AreaType.NUMBER:
			{
				return (current_symbol == DECIMAL_SEPARATOR && char.IsDigit(next_symbol)) || // Example: 7.0
					current_symbol == EXPONENT_SEPARATOR || // Example: 100e0
					current_symbol == SIGNED_TYPE_SEPARATOR || // Example 0i8
					current_symbol == UNSIGNED_TYPE_SEPARATOR || // Example 0u32
					(previous_symbol == EXPONENT_SEPARATOR && (current_symbol == '+' || current_symbol == '-')); // Examples: 3.14159e+10, 10e-10
			}

			default: return false;
		}
	}

	/// <summary>
	/// Skips all the spaces starting from the given position
	/// </summary>
	/// <param name="text">Current text</param>
	/// <param name="position">Start of the spaces</param>
	/// <returns>Returns the position after the spaces</returns>
	private static Position SkipSpaces(string text, Position position)
	{
		while (position.Local < text.Length)
		{
			var c = text[position.Local];

			if (c != ' ')
			{
				break;
			}
			else
			{
				position.NextCharacter();
			}
		}

		return position;
	}

	/// <summary>
	/// Finds the corresponding end parenthesis and returns its position
	/// </summary>
	/// <param name="text">Current text</param>
	/// <param name="start">Start of the opening parenthesis</param>
	/// <returns>Position of the closing parenthesis</returns>
	private static Position SkipContent(string text, Position start)
	{
		var position = start.Clone();

		var opening = text[position.Local];
		var closing = ParenthesisType.Get(opening).Closing;

		var count = 0;

		while (position.Local < text.Length)
		{
			var c = text[position.Local];

			if (c == LINE_ENDING)
			{
				position.NextLine();
			}
			else
			{
				if (c == opening)
				{
					count++;
				}
				else if (c == closing)
				{
					count--;
				}

				position.NextCharacter();
			}

			if (count == 0)
			{
				return position;
			}
		}

		throw new LexerException(start, "Could not find closing parenthesis");
	}

	/// <summary>
	/// Returns whether a multiline comment begins at the specified position
	/// </summary>
	private static bool IsMultilineComment(string text, Position start)
	{
		return start.Local + (MULTILINE_COMMENT.Length * 2 + 1) <= text.Length && text[start.Local..(start.Local + MULTILINE_COMMENT.Length)] == MULTILINE_COMMENT && text[start.Local + MULTILINE_COMMENT.Length] != COMMENT;
	}

	/// <summary>
	/// Skips the current comment and returns the position
	/// </summary>
	/// <param name="text">Current text</param>
	/// <param name="start">Start of the comment</param>
	/// <returns>Position after the comment</returns>
	private static Position SkipComment(string text, Position start)
	{
		if (IsMultilineComment(text, start))
		{
			var j = text.IndexOf(MULTILINE_COMMENT, start.Local + MULTILINE_COMMENT.Length);

			if (j == -1)
			{
				throw new LexerException(start, $"Multiline comment did not have a closing '{MULTILINE_COMMENT}'");
			}

			// Skip to the end of the multiline comment
			j += MULTILINE_COMMENT.Length;

			// Count how many line ending are there inside the comment
			var lines = text[start.Local..j].Count(i => i == LINE_ENDING);

			// Try to resolve the character position from the last line ending inside the multiline comment
			var k = text[start.Local..j].LastIndexOf(LINE_ENDING);

			// If there is no line ending inside the multiline comment, it means the comment uses only one line and its length can be added to the current character position
			var c = k != -1 ? (j - k - 1) : (start.Character + j - start.Local);

			return new Position(start.Line + lines, c, j, j);
		}

		var i = text.IndexOf(LINE_ENDING, start.Local);

		if (i != -1)
		{
			var length = i - start.Local;
			return new Position(start.Line, start.Character + length, start.Local + length, i);
		}
		else
		{
			var length = text.Length - start.Local;
			return new Position(start.Line, start.Character + length, start.Local + length, text.Length);
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

		var length = (i + 1) - start.Local;

		return new Position(start.Line, start.Character + length, start.Local + length, i + 1);
	}

	/// <summary>
	/// Skips the current string and returns the position
	/// </summary>
	/// <param name="text">Current text</param>
	/// <param name="start">Start of the string</param>
	/// <returns>Position after the string</returns>
	private static Position SkipString(string text, Position start)
	{
		return SkipClosures(STRING, text, start, "Could not find the end of the string");
	}

	/// <summary>
	/// Skips the current character value and returns the position
	/// </summary>
	/// <param name="text">Current text</param>
	/// <param name="start">Start of the character value</param>
	/// <returns>Position after the character value</returns>
	private static Position SkipCharacterValue(string text, Position start)
	{
		return SkipClosures(CHARACTER, text, start, "Could not find the end of the character value");
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
			error = "Could not understand hexadecimal value";
		}
		else if (command == 'u')
		{
			length = 4;
			error = "Could not understand unicode character";
		}
		else if (command == 'U')
		{
			length = 8;
			error = "Could not understand unicode character";
		}
		else
		{
			throw new LexerException(position, $"Could not understand string command '{command}'");
		}

		var hexadecimal = text[2..];

		if (hexadecimal.Length != length)
		{
			throw new LexerException(position, "Invalid character");
		}

		if (!ulong.TryParse(hexadecimal, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong value))
		{
			throw new LexerException(position, error);
		}

		return value;
	}

	/// <summary>
	/// Returns the integer value of the character value
	/// </summary>
	private static ulong GetCharacterValue(Position position, string text)
	{
		// Remove the closures
		text = text[1..^1];

		if (text.Length == 0)
		{
			throw new LexerException(position, "Character value was empty");
		}

		if (text.First() != '\\')
		{
			if (text.Length != 1)
			{
				throw new LexerException(position, "Character value allows only one character");
			}

			return text.First();
		}

		if (text.Length <= 2)
		{
			throw new LexerException(position, "Invalid character");
		}

		return GetSpecialCharacterValue(position, text);
	}

	/// <summary>
	/// Returns the next token area in the text
	/// </summary>
	/// <param name="text">Current text</param>
	/// <param name="start">Position from which to start looking for the next token</param>
	/// <returns>The next token in the text</returns>
	public static Area? GetNextToken(string text, Position start)
	{
		// Firsly the spaces must be skipped to find the next token
		var position = SkipSpaces(text, start);

		// Verify there's text to iterate
		if (position.Local == text.Length)
		{
			return null;
		}

		var area = new Area
		{
			Start = position.Clone(),
			Type = GetType(text[position.Local])
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
				area.Text = GetCharacterValue(area.Start, text[area.Start.Local..area.End.Local]).ToString(CultureInfo.InvariantCulture);
				area.Type = AreaType.NUMBER;
				return area;
			}

			default: break;
		}

		position.NextCharacter();

		// Possible types are now: TEXT, NUMBER, OPERATOR
		while (position.Local < text.Length)
		{
			var current_symbol = text[position.Local];

			if (IsContent(current_symbol))
			{
				// There cannot be number and content tokens side by side
				if (area.Type == AreaType.NUMBER)
				{
					throw new LexerException(position, "Missing operator between number and parenthesis");
				}

				break;
			}

			var type = GetType(current_symbol);

			var previous_symbol = position.Local == 0 ? (char)0 : text[position.Local - 1];
			var next_symbol = position.Local + 1 >= text.Length ? (char)0 : text[position.Local + 1];

			if (!IsPartOf(area.Type, type, previous_symbol, current_symbol, next_symbol))
			{
				break;
			}

			position.NextCharacter();
		}

		area.End = position;
		area.Text = text[area.Start.Local..area.End.Local];

		return area;
	}

	/// <summary>
	/// Parses a token from text
	/// </summary>
	/// <param name="text">Text to parse into a token</param>
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
	/// Parses a token from a text area
	/// </summary>
	/// <param name="area">Current text area</param>
	/// <returns>Area as a token</returns>
	private static Token ParseToken(Area area)
	{
		return area.Type switch
		{
			AreaType.TEXT => ParseTextToken(area.Text),
			AreaType.NUMBER => new NumberToken(area.Text, area.Start),
			AreaType.OPERATOR => new OperatorToken(Operators.Exists(area.Text) ? area.Text : throw new LexerException(area.Start, $"Unknown operator '{area.Text}'")),
			AreaType.CONTENT => new ContentToken(area.Text, area.Start),
			AreaType.END => new Token(TokenType.END),
			AreaType.STRING => new StringToken(area.Text),
			_ => throw new LexerException(area.Start, $"Unknown token '{area.Text}'"),
		};
	}

	/// <summary>
	/// Join all sequential modifier keywords into one token
	/// </summary>
	public static void JoinModifiers(List<Token> tokens)
	{
		if (tokens.Count == 1)
		{
			return;
		}

		for (var i = tokens.Count - 2; i >= 0; i--)
		{
			var current = tokens[i];
			var next = tokens[i + 1];

			if (current is KeywordToken current_keyword && next is KeywordToken next_keyword &&
				current_keyword.Keyword is ModifierKeyword current_modifier &&
				next_keyword.Keyword is ModifierKeyword next_modifier)
			{
				var identifier = current_modifier.Identifier + ' ' + next_modifier.Identifier;
				var combined = new ModifierKeyword(identifier, current_modifier.Modifier | next_modifier.Modifier);

				tokens.RemoveAt(i); tokens.RemoveAt(i);
				tokens.Insert(i, new KeywordToken(combined));
			}
		}
	}

	/// <summary>
	/// Converts all special characters in the text to use the hexadecimal character format
	/// </summary>
	private static string PreprocessSpecialCharacters(string text)
	{
		text = text.Replace("\\a", "\\x07");
		text = text.Replace("\\b", "\\x08");
		text = text.Replace("\\t", "\\x09");
		text = text.Replace("\\n", "\\x0A");
		text = text.Replace("\\v", "\\x0B");
		text = text.Replace("\\f", "\\x0C");
		text = text.Replace("\\r", "\\x0D");
		text = text.Replace("\\e", "\\x1B");
		text = text.Replace("\\\"", "\\x22");
		text = text.Replace("\\\'", "\\x27");
		text = text.Replace("\\\\", "\\x5C");

		return text;
	}

	/// <summary>
	/// Returns the text as a token list
	/// </summary>
	/// <param name="text">Text to scan</param>
	/// <returns>Text as a token list</returns>
	public static List<Token> GetTokens(string text)
	{
		return GetTokens(text, new Position());
	}

	/// <summary>
	/// Returns the text as a token list
	/// </summary>
	/// <param name="text">Text to scan</param>
	/// <param name="anchor">Current position</param>
	/// <returns>Text as a token list</returns>
	public static List<Token> GetTokens(string text, Position anchor)
	{
		text = PreprocessSpecialCharacters(text);

		var tokens = new List<Token>();
		var position = new Position(anchor.Line, anchor.Character, 0, anchor.Absolute);

		while (position.Local < text.Length)
		{
			var area = GetNextToken(text, position.Clone());

			if (area == null)
			{
				break;
			}

			if (area.Type != AreaType.COMMENT)
			{
				var token = ParseToken(area);
				token.Position = area.Start;
				tokens.Add(token);
			}

			position = area.End;
		}

		JoinModifiers(tokens);

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
