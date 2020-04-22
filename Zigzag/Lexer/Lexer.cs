using System;
using System.Collections.Generic;

public class Lexer
{
	public static Size Size { get; private set; } = Size.QWORD;

	private const char COMMENT = '#';
	private const char STRING = '\'';

	public enum Type
	{
		UNSPECIFIED,
		TEXT,
		COMMENT,
		NUMBER,
		CONTENT,
		OPERATOR,
		STRING,
		END
	}

	public class Area
	{
		public Type Type { get; set; }

		public string Text { get; set; } = string.Empty;

		public Position Start { get; set; } = new Position();
		public Position End { get; set; } = new Position();
	}

	/// <summary>
	/// Returns whether the character is a operator
	/// </summary>
	/// <param name="c">Character to scan</param>
	/// <returns>True if the character is a operator, otherwise false</returns>
	private static bool IsOperator(char c)
	{
		return c >= 33 && c <= 47 && c != COMMENT && c != STRING || c >= 58 && c <= 63 || c == 94 || c == 124 || c == 126;
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
	/// Returns the type of the character
	/// </summary>
	/// <param name="c">Character to scan</param>
	/// <returns>Type of the character</returns>
	private static Type GetType(char c)
	{
		if (IsText(c))
		{
			return Type.TEXT;
		}
		else if (IsDigit(c))
		{
			return Type.NUMBER;
		}
		else if (IsContent(c))
		{
			return Type.CONTENT;
		}
		else if (IsOperator(c))
		{
			return Type.OPERATOR;
		}
		else if (IsComment(c))
		{
			return Type.COMMENT;
		}
		else if (IsString(c))
		{
			return Type.STRING;
		}
		else if (c == '\n')
		{
			return Type.END;
		}

		return Type.UNSPECIFIED;
	}

	/// <summary>
	/// Returns whether the character is part of the progressing token
	/// </summary>
	/// <param name="previous">Type of the progressing token</param>
	/// <param name="current">Type of the current character</param>
	/// <param name="c">Current character</param>
	/// <returns>True if the character is part of the progressing token</returns>
	private static bool IsPartOf(Type previous, Type current, char c)
	{
		if (current == previous || previous == Type.UNSPECIFIED)
		{
			return true;
		}

		switch (previous)
		{
			case Type.TEXT:
			{
				return current == Type.NUMBER;
			}

			case Type.NUMBER:
			{
				return c == '.';
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
		while (position.Absolute < text.Length)
		{
			var c = text[position.Absolute];

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

		var opening = text[position.Absolute];
		var closing = ParenthesisType.Get(opening).Closing;

		var count = 0;

		while (position.Absolute < text.Length)
		{
			var c = text[position.Absolute];

			if (c == '\n')
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

		throw Errors.Get(start, "Couldn't find closing parenthesis");
	}

	/// <summary>
	/// Skips the current comment and returns the position
	/// </summary>
	/// <param name="text">Current text</param>
	/// <param name="start">Start of the comment</param>
	/// <returns>Position after the comment</returns>
	private static Position SkipComment(string text, Position start)
	{
		var i = text.IndexOf('\n', start.Absolute);

		if (i != -1)
		{
			var length = i - start.Absolute;
			return new Position(start.Line, start.Character + length, i).NextLine();
		}
		else
		{
			var length = text.Length - start.Absolute;
			return new Position(start.Line, start.Character + length, text.Length);
		}
	}

	/// <summary>
	/// Skips the current string and returns the position
	/// </summary>
	/// <param name="text">Current text</param>
	/// <param name="start">Start of the comment</param>
	/// <returns>Position after the string</returns>
	private static Position SkipString(string text, Position start)
	{
		var i = text.IndexOf(STRING, start.Absolute + 1);
		var j = text.IndexOf('\n', start.Absolute + 1);

		if (i == -1 || j != -1 && j < i)
		{
			throw Errors.Get(start, "Couldn't find the end of the string");
		}

		var length = i - start.Absolute;

		return new Position(start.Line, start.Character + length, i + 1);
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
		if (position.Absolute == text.Length)
		{
			return null;
		}

		var area = new Area
		{
			Start = position.Clone(),
			Type = GetType(text[position.Absolute])
		};

		switch (area.Type)
		{

			case Type.COMMENT:
			{
				area.End = SkipComment(text, area.Start);
				area.Text = text.Substring(area.Start.Absolute, area.End.Absolute - area.Start.Absolute);
				return area;
			}

			case Type.CONTENT:
			{
				area.End = SkipContent(text, area.Start);
				area.Text = text.Substring(area.Start.Absolute, area.End.Absolute - area.Start.Absolute);
				return area;
			}

			case Type.END:
			{
				area.End = position.Clone().NextLine();
				area.Text = "\n";
				return area;
			}

			case Type.STRING:
			{
				area.End = SkipString(text, area.Start);
				area.Text = text.Substring(area.Start.Absolute, area.End.Absolute - area.Start.Absolute);
				return area;
			}

			default: break;
		}

		// Possible types are now: TEXT, NUMBER, OPERATOR
		while (position.Absolute < text.Length)
		{
			var c = text[position.Absolute];

			if (IsContent(c))
			{

				// There cannot be number and content tokens side by side
				if (area.Type == Type.NUMBER)
				{
					throw Errors.Get(position, "Missing operator between number and parenthesis");
				}

				break;
			}

			var type = GetType(c);

			if (!IsPartOf(area.Type, type, c))
			{
				break;
			}

			position.NextCharacter();
		}

		area.End = position;
		area.Text = text.Substring(area.Start.Absolute, area.End.Absolute - area.Start.Absolute);

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
	/// <param name="anchor">Current position in text</param>
	/// <returns>Area as a token</returns>
	private static Token ParseToken(Area area, Position anchor)
	{
		switch (area.Type)
		{
			case Type.TEXT: return ParseTextToken(area.Text);
			case Type.NUMBER: return new NumberToken(area.Text);
			case Type.OPERATOR: return new OperatorToken(area.Text);
			case Type.CONTENT: return new ContentToken(area.Text, anchor += area.Start);
			case Type.END: return new Token(TokenType.END);
			case Type.STRING: return new StringToken(area.Text);

			default: throw Errors.Get(anchor += area.Start, new Exception(string.Format("Unrecognized token '{0}'", area.Text)));
		}
	}

	private const int FUNCTION_LENGTH = 2;

	/// <summary>
	/// Forms function tokens from the given tokens
	/// </summary>
	/// <param name="tokens">Tokens to iterate</param>
	private static void CreateFunctionCalls(List<Token> tokens)
	{
		if (tokens.Count < FUNCTION_LENGTH)
		{
			return;
		}

		for (int i = tokens.Count - 2; i >= 0;)
		{
			var current = tokens[i];

			if (current.Type == TokenType.IDENTIFIER)
			{
				var next = tokens[i + 1];

				if (next.Type == TokenType.CONTENT)
				{
					var parameters = (ContentToken)next;

					if (parameters.Type == ParenthesisType.PARENTHESIS)
					{
						var name = (IdentifierToken)current;
						var function = new FunctionToken(name, parameters);
						function.Position = name.Position;

						tokens[i] = function;
						tokens.RemoveAt(i + 1);

						i -= FUNCTION_LENGTH;
						continue;
					}
				}
			}

			i--;
		}
	}

	/// <summary>
	/// Join all sequential modifier keywords into one token
	/// </summary>
	public static void JoinModifiers(List<Token> tokens)
	{
		if (tokens.Count == 1) return;

		for (var i = tokens.Count - 2; i >= 0; i--)
		{
			var current = tokens[i];
			var next = tokens[i + 1];

			if (current is KeywordToken current_keyword && next is KeywordToken next_keyword &&
				current_keyword.Keyword is AccessModifierKeyword current_modifier && 
				next_keyword.Keyword is AccessModifierKeyword next_modifier)
			{
				var identifier = current_modifier.Identifier + ' ' + next_modifier.Identifier;
				var combined = new AccessModifierKeyword(identifier, current_modifier.Modifier | next_modifier.Modifier);

				tokens.RemoveAt(i); tokens.RemoveAt(i);
				tokens.Insert(i, new KeywordToken(combined));
			}
		}
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
		var tokens = new List<Token>();
		var position = new Position();

		while (position.Absolute < text.Length)
		{
			var area = GetNextToken(text, position);

			if (area == null)
			{
				break;
			}

			if (area.Type != Type.COMMENT)
			{
				var token = ParseToken(area, anchor);
				token.Position = (anchor += area.Start);
				tokens.Add(token);
			}

			position = area.End;
		}

		CreateFunctionCalls(tokens);
		JoinModifiers(tokens);

		return tokens;
	}
}
