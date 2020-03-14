using System.Collections.Generic;

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
		var header = (FunctionToken)tokens[HEADER];
		var body = (ContentToken)tokens[BODY];

		var function = new Function(context, AccessModifier.PUBLIC, header.Name, header.GetParameterNames(), body.GetTokens());
		context.Declare(function);

		return new Node();
	}
}
