using System;
using System.Collections.Generic;
using System.Linq;

public class TemplateFunctionCallPattern : Pattern
{
	public const int PRIORITY = 19;

	// Pattern:
	// $name <$1, $2, ... $n> (...)
	public TemplateFunctionCallPattern() : base(TokenType.IDENTIFIER) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return Common.ConsumeTemplateFunctionCall(state);
	}

	public override Node Build(Context context, PatternState state, List<Token> tokens)
	{
		var name = tokens.First().To<IdentifierToken>();
		var descriptor = new FunctionToken(name, tokens.Last()?.To<ContentToken>() ?? throw new ApplicationException("Tried to create a template function call but the syntax was invalid"));
		var template_arguments = Common.ReadTemplateArguments(context, new Queue<Token>(tokens.Skip(1)));

		return Singleton.GetFunction(context, context, descriptor, template_arguments);
	}
}