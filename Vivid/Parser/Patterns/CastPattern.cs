using System.Collections.Generic;
using System.Linq;

public class CastPattern : Pattern
{
	public const int PRIORITY = 19;

	private const int OBJECT = 0;
	private const int CAST = 1;
	private const int TYPE = 2;

	// Pattern: ... as Type [<$1, $2, ..., $n>]
	public CastPattern() : base
	(
		TokenType.OBJECT,
		TokenType.KEYWORD,
		TokenType.IDENTIFIER
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		if (!tokens[CAST].Is(Keywords.AS))
		{
			return false;
		}

		Try(Common.ConsumeTemplateArguments, state);

		return true;
	}

	public override Node Build(Context context, PatternState state, List<Token> tokens)
	{
		var source = Singleton.Parse(context, tokens[OBJECT]);
		var type = Common.ReadTypeArgument(context, new Queue<Token>(tokens.Skip(TYPE)));

		if (type == null)
		{
			throw Errors.Get(tokens[TYPE].Position, "Could not resolve the cast type");
		}

		return new CastNode(source, new TypeNode(type, tokens[TYPE].Position));
	}
}