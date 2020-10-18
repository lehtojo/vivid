using System;
using System.Collections.Generic;
using System.Linq;

public class ContentToken : Token
{
	private const int OPENING = 0;
	private const int EMPTY = 2;

	public List<Token> Tokens { get; private set; }  = new List<Token>();
	public bool IsEmpty => Tokens.Count == 0;

	public new ParenthesisType Type { get; private set; }

	public int GetSectionCount()
	{
		return Math.Max(1, Tokens.Count(i => i.Type == TokenType.OPERATOR && i.To<OperatorToken>().Operator == Operators.COMMA));
	}

	public List<List<Token>> GetSections()
	{
		var sections = new List<List<Token>>();

		if (IsEmpty)
		{
			return sections;
		}

		var section = new List<Token>();
		
		foreach (var token in Tokens)
		{
			if (token.Type == TokenType.OPERATOR && token.To<OperatorToken>().Operator == Operators.COMMA)
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

	public ContentToken(string raw, Position position) : base(TokenType.CONTENT)
	{
		Type = ParenthesisType.Get(raw[OPENING]);

		if (raw.Length == EMPTY)
		{
			return;
		}

		Tokens = Lexer.GetTokens(raw[1..^1], position.Clone().NextCharacter());
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
}
