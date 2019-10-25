using System.Collections.Generic;
public class ReturnPattern : Pattern
{
	public const int PRIORITY = 1;

	private const int RETURN = 0;
	private const int OBJECT = 1;

	// Pattern:
	// return ...
	public ReturnPattern() : base(TokenType.KEYWORD, /* return */
								  TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.NUMBER | TokenType.CONTENT | TokenType.DYNAMIC) /* ... */ {}


	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}


	public override bool Passes(List<Token> tokens)
	{
		KeywordToken keyword = (KeywordToken)tokens[RETURN];
		return keyword.Keyword == Keywords.RETURN;
	}


	public override Node Build(Context context, List<Token> tokens)
	{
		return new ReturnNode(Singleton.Parse(context, tokens[OBJECT]));
	}
}
