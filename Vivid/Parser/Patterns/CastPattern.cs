using System.Collections.Generic;
using System.Linq;

public class CastPattern : Pattern
{
	private const int OBJECT = 0;
	private const int CAST = 1;
	private const int TYPE = 2;

	// Pattern: $value as $type
	public CastPattern() : base
	(
		TokenType.OBJECT,
		TokenType.KEYWORD
	)
	{ Priority = 19; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return tokens[CAST].Is(Keywords.AS) && Common.ConsumeType(state);
	}

	public override Node Build(Context context, ParserState state, List<Token> tokens)
	{
		var source = Singleton.Parse(context, tokens[OBJECT]);
		var type = Common.ReadType(context, tokens, TYPE);

		if (type == null) throw Errors.Get(tokens[TYPE].Position, "Can not resolve the cast type");

		return new CastNode(source, new TypeNode(type, tokens[TYPE].Position), tokens[CAST].Position);
	}
}