using System.Collections.Generic;

public class ModifierSectionPattern : Pattern
{
	public const int MODIFIER = 0;
	public const int COLON = 1;

	public const int PRIORITY = 20;
	
	// Example: public static:
	public ModifierSectionPattern() : base
	(
		TokenType.KEYWORD,
		TokenType.OPERATOR
	) {}

	public override bool Passes(Context context, List<Token> tokens)
	{
      return tokens[MODIFIER].To<KeywordToken>().Keyword.Type == KeywordType.ACCESS_MODIFIER &&
               tokens[COLON].To<OperatorToken>().Operator == Operators.COLON;
	}

	public override Node? Build(Context context, List<Token> tokens)
	{
      return new SectionNode(tokens[MODIFIER].To<KeywordToken>().Keyword.To<AccessModifierKeyword>().Modifier);
	}

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}
}