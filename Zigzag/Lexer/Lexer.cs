using System;
using System.Collections.Generic;

public class Lexer
{
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

		public string Text { get; set; }

		public Position Start { get; set; }
		public Position End { get; set; }
	}

	private static bool IsOperator(char c)
	{
		return c >= 33 && c <= 47 && c != COMMENT && c != STRING || c >= 58 && c <= 63 || c == 94 || c == 124 || c == 126;
	}

	private static bool IsDigit(char c)
	{
		return c >= 48 && c <= 57;
	}

	private static bool IsText(char c)
	{
		return c >= 65 && c <= 90 || c >= 97 && c <= 122 || c == 95;
	}

	private static bool IsContent(char c)
	{
		return ParenthesisType.Has(c);
	}

	private static bool ÍsComment(char c)
	{
		return c == COMMENT;
	}

	private static bool IsString(char c)
	{
		return c == STRING;
	}

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
		else if (ÍsComment(c))
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

	private static bool IsPartOf(Type @base, Type current, char c)
	{
		if (current == @base || @base == Type.UNSPECIFIED)
		{
			return true;
		}

		switch (@base)
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

	private static Position SkipSpaces(string text, Position position)
	{
		while (position.Absolute < text.Length)
		{
			char c = text[position.Absolute];

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

	private static Position SkipContent(string text, Position start)
	{
		Position position = start.Clone();

		char opening = text[position.Absolute];
		char closing = ParenthesisType.Get(opening).Closing;

		int count = 0;

		while (position.Absolute < text.Length)
		{
			char c = text[position.Absolute];

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

	private static Position SkipComment(string text, Position start)
	{
		int i = text.IndexOf('\n', start.Absolute);

		if (i != -1)
		{
			int length = i - start.Absolute;
			return new Position(start.Line, start.Character + length, i).NextLine();
		}
		else
		{
			int length = text.Length - start.Absolute;
			return new Position(start.Line, start.Character + length, text.Length);
		}
	}

	private static Position SkipString(string text, Position start)
	{
		int i = text.IndexOf(STRING, start.Absolute + 1);
		int j = text.IndexOf('\n', start.Absolute + 1);

		if (i == -1 || j != -1 && j < i)
		{
			throw Errors.Get(start, "Couldn't find the end of the string");
		}

		int length = i - start.Absolute;

		return new Position(start.Line, start.Character + length, i + 1);
	}

	public static Area GetNextToken(string text, Position start)
	{
		// Firsly the spaces must be skipped to find the next token
		Position position = SkipSpaces(text, start);

		// Verify there's text to iterate
		if (position.Absolute == text.Length)
		{
			return null;
		}

		Area area = new Area
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
			char c = text[position.Absolute];

			if (IsContent(c))
			{

				// There cannot be number and content tokens side by side
				if (area.Type == Type.NUMBER)
				{
					throw Errors.Get(position, "Missing operator between number and parenthesis");
				}

				break;
			}

			Type type = GetType(c);

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

	private static void Functions(List<Token> tokens)
	{
		if (tokens.Count < FUNCTION_LENGTH)
		{
			return;
		}

		for (int i = tokens.Count - 2; i >= 0;)
		{
			Token current = tokens[i];

			if (current.Type == TokenType.IDENTIFIER)
			{
				Token next = tokens[i + 1];

				if (next.Type == TokenType.CONTENT)
				{
					ContentToken parameters = (ContentToken)next;

					if (parameters.Type == ParenthesisType.PARENTHESIS)
					{
						IdentifierToken name = (IdentifierToken)current;
						FunctionToken function = new FunctionToken(name, parameters);

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

	public static List<Token> GetTokens(string raw)
	{
		return GetTokens(raw, new Position());
	}

	public static List<Token> GetTokens(string raw, Position anchor)
	{
		List<Token> tokens = new List<Token>();
		Position position = new Position();

		while (position.Absolute < raw.Length)
		{
			Area area = GetNextToken(raw, position);

			if (area == null)
			{
				break;
			}

			if (area.Type != Type.COMMENT)
			{
				Token token = ParseToken(area, anchor);
				token.Position = (anchor += area.Start);
				tokens.Add(token);
			}

			position = area.End;
		}

		Functions(tokens);

		return tokens;
	}
}
