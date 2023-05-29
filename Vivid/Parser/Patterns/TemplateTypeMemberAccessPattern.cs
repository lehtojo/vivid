using System.Collections.Generic;

public class TemplateTypeMemberAccessPattern : Pattern
{
	// Pattern: $name <$1, $2, ... $n> .
	public TemplateTypeMemberAccessPattern() : base(TokenType.IDENTIFIER)
	{
		Priority = 19;
	}

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		if (!Common.ConsumeTemplateArguments(state)) return false;

		var next = state.Peek();
		return next != null && next.Is(Operators.DOT);
	}

	public override Node Build(Context context, ParserState state, List<Token> tokens)
	{
		var position = tokens[0].Position;
		var type = Common.ReadType(context, tokens) ?? throw Errors.Get(position, "Can not resolve the accessed type");

		return new TypeNode(type, position);
	}
}