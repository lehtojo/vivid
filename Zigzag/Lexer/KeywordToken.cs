using System;
using System.Collections.Generic;

public class KeywordToken : Token
{
	public Keyword Keyword { get; private set; }

	public KeywordToken(string text) : base(TokenType.KEYWORD)
	{
		Keyword = Keywords.Get(text);
	}

	public KeywordToken(Keyword keyword) : base(TokenType.KEYWORD)
	{
		Keyword = keyword;
	}

	public override bool Equals(object? obj)
	{
		return obj is KeywordToken token &&
			   base.Equals(obj) &&
			   EqualityComparer<Keyword>.Default.Equals(Keyword, token.Keyword);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Keyword);
	}
}
