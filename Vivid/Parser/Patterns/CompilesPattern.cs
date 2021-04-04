using System.Collections.Generic;

public class CompilesPattern : Pattern
{
	public const int PRIORITY = 5;

	private const int COMPILES = 0;
	private const int CONDITION = 2;

	// Pattern: compiles [\n] {...}
	public CompilesPattern() : base
	(
		TokenType.KEYWORD,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.CONTENT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return tokens[COMPILES].Is(Keywords.COMPILES) && tokens[CONDITION].Is(ParenthesisType.CURLY_BRACKETS);
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		var conditions = Singleton.Parse(context, tokens[CONDITION].To<ContentToken>());
		var result = new CompilesNode(tokens[COMPILES].Position);

		conditions.ForEach(i => result.Add(i));

		return result;
	}
}