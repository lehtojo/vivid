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
}
