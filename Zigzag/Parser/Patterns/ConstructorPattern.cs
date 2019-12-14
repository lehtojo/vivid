using System.Collections.Generic;

public class ConstructorPattern : Pattern
{
	public const int PRIORITY = 20;

	private const int HEAD = 0;
	private const int BODY = 2;

	// a-z (...) [\n] (...)
	public ConstructorPattern() : base
	(
	   TokenType.FUNCTION,
	   TokenType.END | TokenType.OPTIONAL,
	   TokenType.CONTENT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		var head = tokens[HEAD] as FunctionToken;
		var type = context.GetTypeParent();

		return type != null && head.Name == type.Name;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		var head = tokens[HEAD] as FunctionToken;
		var body = tokens[BODY] as ContentToken;

		var type = context as Type;

		var constructor = new Constructor(context, AccessModifier.PUBLIC, head.GetParameterNames(), body.GetTokens());
		type.AddConstructor(constructor);

		return null;
	}
}