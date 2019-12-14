using System;
using System.Collections.Generic;
using System.Text;

class FunctionPattern : Pattern
{
	public const int PRIORITY = 20;

	public const int HEADER = 0;
	public const int BODY = 2;

	// ... () [\n] ()
	public FunctionPattern() : base
	(
		TokenType.FUNCTION, TokenType.END | TokenType.OPTIONAL, TokenType.CONTENT
	) {}

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		return true;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		var header = tokens[HEADER] as FunctionToken;
		var body = tokens[BODY] as ContentToken;

		var function = new Function(context, AccessModifier.PUBLIC, header.Name, header.GetParameterNames(), body.GetTokens());
		context.Declare(function);

		return new Node();
	}
}
