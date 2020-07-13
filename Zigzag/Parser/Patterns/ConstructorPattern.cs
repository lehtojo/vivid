using System.Collections.Generic;

public class ConstructorPattern : Pattern
{
	public const int PRIORITY = 21;

	private const int HEAD = 0;
	private const int BODY = 2;

	// init (...) [\n] {...}
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
		var head = tokens[HEAD].To<FunctionToken>();
		var type = context.GetTypeParent();

		return type != null && head.Name == Keywords.INIT.Identifier;
	}

	public override Node? Build(Context context, List<Token> tokens)
	{
		var head = tokens[HEAD].To<FunctionToken>();
		var body = tokens[BODY].To<ContentToken>();
		var type = (Type)context;

		if (head.Name == Keywords.INIT.Identifier)
		{
			var constructor = new Constructor(context, AccessModifier.PUBLIC, body.GetTokens());
			constructor.Parameters = head.GetParameterNames(constructor);
		
			type.AddConstructor(constructor);
		}
		else
		{
			var constructor = new Constructor(context, AccessModifier.PUBLIC, body.GetTokens());
			constructor.Parameters = head.GetParameterNames(constructor);
		
			type.AddDestructor(constructor);
		}
		

		return null;
	}
}