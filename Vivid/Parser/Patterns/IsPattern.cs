using System.Collections.Generic;
using System.Linq;

public class IsPattern : Pattern
{
	private const int KEYWORD = 1;
	private const int TYPE = 2;

	// Pattern: $object is [not] $type [$name]
	public IsPattern() : base
	(
		TokenType.DYNAMIC | TokenType.IDENTIFIER | TokenType.FUNCTION,
		TokenType.KEYWORD
	)
	{ Priority = 16; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		if (!tokens[KEYWORD].Is(Keywords.IS) && !tokens[KEYWORD].Is(Keywords.IS_NOT)) return false;

		// Consume the type
		if (!Common.ConsumeType(state)) return false;

		// Try consuming the result variable name
		state.Consume(TokenType.IDENTIFIER);
		return true;
	}

	public override Node? Build(Context context, ParserState state, List<Token> formatted)
	{
		var negate = formatted[KEYWORD].Is(Keywords.IS_NOT);

		var source = Singleton.Parse(context, formatted.First());
		var tokens = formatted.GetRange(TYPE, formatted.Count - TYPE);
		var type = Common.ReadType(context, tokens);

		if (type == null)
		{
			throw Errors.Get(formatted[TYPE].Position, "Can not understand the type");
		}

		var result = (Node?)null;

		// If there is a token left in the queue, it must be the result variable name
		if (tokens.Any())
		{
			var name = tokens.Pop()!.To<IdentifierToken>().Value;
			var variable = new Variable(context, type, VariableCategory.LOCAL, name, Modifier.DEFAULT);

			result = new IsNode(source, type, variable, formatted[KEYWORD].Position);
		}
		else
		{
			result = new IsNode(source, type, null, formatted[KEYWORD].Position);
		}

		return negate ? new NotNode(result, false, result.Position) : result;
	}
}
