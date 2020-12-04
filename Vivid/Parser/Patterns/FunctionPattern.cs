using System.Collections.Generic;
using System.Linq;

class FunctionPattern : Pattern
{
	public const int PRIORITY = 20;

	public const int MODIFIERS = 0;
	public const int HEADER = 1;
	public const int BODY = 3;

	// Pattern: [modifiers] a-z (...) [\n] {...}
	public FunctionPattern() : base
	(
		TokenType.KEYWORD | TokenType.OPTIONAL,
		TokenType.FUNCTION,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.CONTENT
	)
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return tokens[MODIFIERS].Type == TokenType.NONE || tokens[MODIFIERS]?.To<KeywordToken>().Keyword is AccessModifierKeyword;
	}

	private static int GetModifiers(List<Token> tokens)
	{
		return tokens[MODIFIERS].Type == TokenType.NONE ? AccessModifier.PUBLIC : tokens[MODIFIERS].To<KeywordToken>().Keyword.To<AccessModifierKeyword>().Modifier;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		var modifiers = GetModifiers(tokens);
		var header = tokens[HEADER].To<FunctionToken>();
		var body = tokens[BODY].To<ContentToken>();

		var function = new Function(context, modifiers, header.Name, body.Tokens);
		function.Position = header.Position;
		function.Parameters.AddRange(header.GetParameters(function));

		context.Declare(function);

		return new FunctionDefinitionNode(function, header.Position);
	}
}
