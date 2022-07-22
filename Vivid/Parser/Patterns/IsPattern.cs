using System.Collections.Generic;
using System.Linq;

public class IsPattern : Pattern
{
	public const int PRIORITY = 16;

	private const int KEYWORD = 1;
	private const int TYPE = 2;

	// Pattern: $object is [not] $type [<$1, $2, ..., $n>] [$name]
	public IsPattern() : base
	(
		TokenType.DYNAMIC | TokenType.IDENTIFIER | TokenType.FUNCTION,
		TokenType.KEYWORD
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		if (!tokens[KEYWORD].Is(Keywords.IS) && !tokens[KEYWORD].Is(Keywords.IS_NOT))
		{
			return false;
		}

		// Consume the type
		if (!Common.ConsumeType(state))
		{
			return false;
		}

		// Try consuming the result variable name
		Consume(state, TokenType.IDENTIFIER);

		return true;
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		var negate = tokens[KEYWORD].Is(Keywords.IS_NOT);

		var source = Singleton.Parse(context, tokens.First());
		var queue = new Queue<Token>(tokens.Skip(TYPE));
		var type = Common.ReadType(context, queue);

		if (type == null)
		{
			throw Errors.Get(tokens[TYPE].Position, "Can not understand the type");
		}

		var result = (Node?)null;

		// If there is a token left in the queue, it must be the result variable name
		if (queue.Any())
		{
			var name = queue.Dequeue().To<IdentifierToken>().Value;
			var variable = new Variable(context, type, VariableCategory.LOCAL, name, Modifier.DEFAULT);

			result = new IsNode(source, type, variable, tokens[KEYWORD].Position);
		}
		else
		{
			result = new IsNode(source, type, null, tokens[KEYWORD].Position);
		}

		return negate ? new NotNode(result, result.Position) : result;
	}
}
