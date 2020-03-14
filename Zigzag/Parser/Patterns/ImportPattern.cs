using System.Collections.Generic;

public class ImportPattern : Pattern
{
	private const int PRIORITY = 20;

	private const int IMPORT = 0;
	private const int TYPE = 1;
	private const int HEAD = 2;

	// import a-z (...)
	public ImportPattern() : base
	(
		TokenType.KEYWORD,
		TokenType.FUNCTION
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		var keyword = (KeywordToken)tokens[IMPORT];

		if (keyword.Keyword != Keywords.IMPORT)
		{
			return false;
		}

		return false;
	}

	public override Node? Build(Context context, List<Token> tokens)
	{
		return null;
	}
}