using System;
using System.Collections.Generic;

public class KeywordToken : Token
{
	public Keyword Keyword { get; set; }
	public Position End => Position.Translate(Keyword.Identifier.Length);

	public KeywordToken(string text) : base(TokenType.KEYWORD)
	{
		Keyword = Keywords.Get(text);
	}

	public KeywordToken(Keyword keyword, Position position) : base(TokenType.KEYWORD)
	{
		Keyword = keyword;
		Position = position;
	}

	public override bool Equals(object? other)
	{
		return other is KeywordToken token &&
			   base.Equals(other) &&
			   EqualityComparer<Keyword>.Default.Equals(Keyword, token.Keyword);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Keyword);
	}

	public override object Clone()
	{
		return MemberwiseClone();
	}

	public override string ToString()
	{
		return Keyword.Identifier;
	}
}
