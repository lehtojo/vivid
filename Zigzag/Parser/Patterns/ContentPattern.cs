using System.Collections.Generic;

public class ContentPattern : Pattern
{
	public const int PRIORITY = 16;

	private const int CONTENT = 0;

	public ContentPattern() : base(TokenType.CONTENT) {}

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(List<Token> tokens)
	{
		// Only content with parenthesis type of '()' or '[]' can be automatically parsed
		ContentToken content = (ContentToken)tokens[CONTENT];
		return content.Type == ParenthesisType.PARENTHESIS;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		return Singleton.GetContent(context, (ContentToken)tokens[CONTENT]);
	}
}
