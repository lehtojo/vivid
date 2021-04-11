using System.Collections.Generic;

class FunctionPattern : Pattern
{
	public const int PRIORITY = 20;

	public const int HEADER = 0;
	public const int BODY = 2;

	// Pattern: $name (...) [\n] {...}
	public FunctionPattern() : base
	(
		TokenType.FUNCTION,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.CONTENT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return tokens[BODY].Is(ParenthesisType.CURLY_BRACKETS);
	}

	public override Node Build(Context context, PatternState state, List<Token> tokens)
	{
		var header = tokens[HEADER].To<FunctionToken>();
		var body = tokens[BODY].To<ContentToken>();

		var function = new Function(context, Modifier.DEFAULT, header.Name, body.Tokens, header.Position, body.End);
		function.Parameters.AddRange(header.GetParameters(function));

		context.Declare(function);

		return new FunctionDefinitionNode(function, header.Position);
	}
}
