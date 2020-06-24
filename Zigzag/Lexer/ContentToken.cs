using System;
using System.Collections.Generic;
using System.Linq;

public class ContentToken : Token
{
	private const int OPENING = 0;
	private const int EMPTY = 2;

	private List<ContentToken> Sections = new List<ContentToken>();
	private List<Token> Tokens = new List<Token>();

	public new ParenthesisType Type { get; private set; }

	private bool IsTable => Sections.Count > 0;
	public bool IsEmpty => Tokens.Count == 0 && Sections.Count == 0;
	public int SectionCount => Math.Max(1, Sections.Count);

	private bool IsComma(Token token)
	{
		return token.Type == TokenType.OPERATOR && ((OperatorToken)token).Operator == Operators.COMMA;
	}

	private Stack<int> FindSections(List<Token> tokens)
	{
		var indices = new Stack<int>();
		indices.Push(tokens.Count);

		for (var i = tokens.Count - 1; i >= 0; i--)
		{
			var token = tokens[i];

			if (IsComma(token))
			{
				indices.Push(i);
			}
		}

		return indices;
	}

	public ContentToken(string raw, Position position) : base(TokenType.CONTENT)
	{
		Type = ParenthesisType.Get(raw[OPENING]);

		if (raw.Length != EMPTY)
		{
			var content = raw[1..^1];

			var tokens = Lexer.GetTokens(content, position.Clone().NextCharacter());
			var sections = FindSections(tokens);

			if (sections.Count == 1)
			{
				Tokens = tokens;
				return;
			}

			int start = 0;
			int end;

			while (sections.Count > 0)
			{
				end = sections.Pop();

				var section = tokens.GetRange(start, end - start);

				if (section.Count == 0)
				{
					var comma = tokens[start].Position;
					throw Errors.Get(comma.NextCharacter(), "Parameter cannot be empty");
				}

				Sections.Add(new ContentToken(section));

				start = end + 1;
			}
		}
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
		Sections = new List<ContentToken>(sections);
	}

	public List<Token> GetTokens(int section = 0)
	{
		return IsTable ? Sections[section].GetTokens() : Tokens;
	}

	public override bool Equals(object? obj)
	{
		return obj is ContentToken token &&
			   base.Equals(obj) &&
			   Sections.SequenceEqual(token.Sections) &&
			   Tokens.SequenceEqual(token.Tokens) &&
			   EqualityComparer<ParenthesisType>.Default.Equals(Type, token.Type) &&
			   IsTable == token.IsTable &&
			   IsEmpty == token.IsEmpty &&
			   SectionCount == token.SectionCount;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Sections, Tokens, Type, IsTable, IsEmpty, SectionCount);
	}

	public override object Clone()
	{
		var clone = (ContentToken)MemberwiseClone();
		clone.Sections = Sections.Select(s => (ContentToken)s.Clone()).ToList();
		clone.Tokens = Tokens.Select(t => (Token)t.Clone()).ToList();

		return clone;
	}
}
