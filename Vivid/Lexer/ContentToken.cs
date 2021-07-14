using System;
using System.Collections.Generic;
using System.Linq;

public class ContentToken : Token
{
	private const int OPENING = 0;
	private const int EMPTY = 2;

	public List<Token> Tokens { get; private set; } = new List<Token>();
	public Position? End { get; private set; }
	public bool IsEmpty => Tokens.Count == 0;

	public new ParenthesisType Type { get; set; }

	public List<List<Token>> GetSections()
	{
		var sections = new List<List<Token>>();
		if (Tokens.Count == 0) return sections;

		var section = new List<Token>();

		foreach (var token in Tokens)
		{
			if (token.Is(Operators.COMMA))
			{
				sections.Add(section);
				section = new List<Token>();
				continue;
			}

			section.Add(token);
		}

		sections.Add(section);

		return sections;
	}

	public ContentToken(string raw, Position start, Position end) : base(TokenType.CONTENT)
	{
		Position = start;
		End = end;
		Type = ParenthesisType.Get(raw[OPENING]);

		if (raw.Length == EMPTY)
		{
			return;
		}

		Tokens = Lexer.GetTokens(raw[1..^1], start.Clone().NextCharacter());
	}

	public ContentToken() : base(TokenType.CONTENT)
	{
		Type = ParenthesisType.PARENTHESIS;
		Tokens = new List<Token>();
	}

	public ContentToken(List<Token> tokens) : base(TokenType.CONTENT)
	{
		Type = ParenthesisType.PARENTHESIS;
		Tokens = new List<Token>(tokens);
	}

	public ContentToken(params Token[] tokens) : base(TokenType.CONTENT)
	{
		Type = ParenthesisType.PARENTHESIS;
		Tokens = new List<Token>(tokens);
	}

	public ContentToken(ParenthesisType type, params Token[] tokens) : base(TokenType.CONTENT)
	{
		Type = type;
		Tokens = new List<Token>(tokens);
	}

	public ContentToken(params ContentToken[] sections) : base(TokenType.CONTENT)
	{
		Type = ParenthesisType.PARENTHESIS;
		Tokens = sections.SelectMany(i => i.Tokens).ToList();
	}

	public override bool Equals(object? other)
	{
		return other is ContentToken token &&
			   base.Equals(other) &&
			   Tokens.SequenceEqual(token.Tokens) &&
			   EqualityComparer<ParenthesisType>.Default.Equals(Type, token.Type) &&
			   IsEmpty == token.IsEmpty;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Tokens, Type, IsEmpty);
	}

	public override object Clone()
	{
		var clone = (ContentToken)MemberwiseClone();
		clone.Tokens = Tokens.Select(t => (Token)t.Clone()).ToList();

		return clone;
	}

	public override string ToString()
	{
		return Type.Opening + string.Join(' ', Tokens) + Type.Closing;
	}
}
