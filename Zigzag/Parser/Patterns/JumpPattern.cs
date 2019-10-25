using System.Collections.Generic;
public class JumpPattern : Pattern
{
	public const int PRIORITY = 1;

	private const int GOTO = 0;
	private const int LABEL = 1;

	public JumpPattern() : base(TokenType.KEYWORD, TokenType.IDENTIFIER) {}

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(List<Token> tokens)
	{
		KeywordToken keyword = (KeywordToken)tokens[GOTO];
		return keyword.Keyword == Keywords.GOTO;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		IdentifierToken name = (IdentifierToken)tokens[LABEL];

		if (!context.IsLabelDeclared(name.Value))
		{
			throw Errors.Get(name.Position, $"Label '{name.Value}' doesn't exist in the current context");
		}

		Label label = context.GetLabel(name.Value);
		return new JumpNode(label);
	}
}