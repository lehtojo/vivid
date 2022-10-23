using System.Collections.Generic;

public class ModifierSectionPattern : Pattern
{
	public const int MODIFIER = 0;
	public const int COLON = 1;

	// Pattern: $modifiers :
	public ModifierSectionPattern() : base
	(
		TokenType.KEYWORD,
		TokenType.OPERATOR
	)
	{ Priority = 20; IsConsumable = false; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return tokens[MODIFIER].To<KeywordToken>().Keyword.Type == KeywordType.MODIFIER && tokens[COLON].Is(Operators.COLON);
	}

	public override Node? Build(Context context, ParserState state, List<Token> tokens)
	{
		return new SectionNode(tokens[MODIFIER].To<KeywordToken>().Keyword.To<ModifierKeyword>().Modifier, tokens[MODIFIER].Position);
	}
}