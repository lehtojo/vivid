using System.Collections.Generic;

public class CompilesPattern : Pattern
{
	private const int COMPILES = 0;
	private const int CONDITION = 2;

	// Pattern: compiles [\n] {...}
	public CompilesPattern() : base
	(
		TokenType.KEYWORD,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.PARENTHESIS
	)
	{ Priority = 5; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return tokens[COMPILES].Is(Keywords.COMPILES) && tokens[CONDITION].Is(ParenthesisType.CURLY_BRACKETS);
	}

	public override Node? Build(Context context, ParserState state, List<Token> tokens)
	{
		var conditions = Singleton.Parse(context, tokens[CONDITION].To<ParenthesisToken>());
		var result = new CompilesNode(tokens[COMPILES].Position);

		conditions.ForEach(i => result.Add(i));

		return result;
	}
}