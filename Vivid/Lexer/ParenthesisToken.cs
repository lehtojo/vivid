using System;
using System.Collections.Generic;
using System.Linq;

public class ParenthesisToken : Token
{
	public ParenthesisType Opening { get; set; }
	public List<Token> Tokens { get; set; } = new List<Token>();
	public Position? End { get; private set; }

	public bool IsEmpty => Tokens.Count == 0;

	public ParenthesisToken(string text, Position start, Position end) : base(TokenType.PARENTHESIS)
	{
		Position = start;
		End = end;
		Opening = ParenthesisType.Get(text.First());

		if (text.Length == 2) return;

		Tokens = Lexer.GetTokens(text[1..^1], start.Clone().NextCharacter());
	}

	public ParenthesisToken() : base(TokenType.PARENTHESIS)
	{
		Opening = ParenthesisType.PARENTHESIS;
		Tokens = new List<Token>();
	}

	public ParenthesisToken(List<Token> tokens) : base(TokenType.PARENTHESIS)
	{
		Opening = ParenthesisType.PARENTHESIS;
		Tokens = new List<Token>(tokens);
	}

	public ParenthesisToken(params Token[] tokens) : base(TokenType.PARENTHESIS)
	{
		Opening = ParenthesisType.PARENTHESIS;
		Tokens = new List<Token>(tokens);
	}

	public ParenthesisToken(ParenthesisType type, params Token[] tokens) : base(TokenType.PARENTHESIS)
	{
		Opening = type;
		Tokens = new List<Token>(tokens);
	}

	public ParenthesisToken(ParenthesisType type, Token[] tokens, Position position) : base(TokenType.PARENTHESIS)
	{
		Opening = type;
		Tokens = new List<Token>(tokens);
		Position = position;
	}

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

	public override bool Equals(object? other)
	{
		return other is ParenthesisToken token &&
			   base.Equals(other) &&
			   Tokens.SequenceEqual(token.Tokens) &&
			   EqualityComparer<ParenthesisType>.Default.Equals(Opening, token.Opening) &&
			   IsEmpty == token.IsEmpty;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Tokens, Opening, IsEmpty);
	}

	public override object Clone()
	{
		var clone = (ParenthesisToken)MemberwiseClone();
		clone.Tokens = Tokens.Select(i => (Token)i.Clone()).ToList();

		return clone;
	}

	public override string ToString()
	{
		return Opening.Opening + string.Join(' ', Tokens) + Opening.Closing;
	}
}
