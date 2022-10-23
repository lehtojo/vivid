using System.Collections.Generic;
using System.Linq;

public class TemplateFunctionCallPattern : Pattern
{
	// Pattern: $name <$1, $2, ... $n> (...)
	public TemplateFunctionCallPattern() : base(TokenType.IDENTIFIER)
	{
		Priority = 19;
	}

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return Common.ConsumeTemplateFunctionCall(state);
	}

	public override Node Build(Context context, ParserState state, List<Token> tokens)
	{
		var name = tokens.First().To<IdentifierToken>();
		var descriptor = new FunctionToken(name, tokens.Last().To<ParenthesisToken>()) { Position = name.Position };
		var template_arguments = Common.ReadTemplateArguments(context, tokens, 1);

		return Singleton.GetFunction(context, context, descriptor, template_arguments, false);
	}
}