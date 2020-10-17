using System.Collections.Generic;

public class ConstructorPattern : Pattern
{
	private const string DESTRUCTOR_IDENTIFIER = "deinit";

	public const int PRIORITY = 21;

	private const int HEAD = 0;
	private const int BODY = 2;

	// init/deinit (...) [\n] {...}
	public ConstructorPattern() : base
	(
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
		// Constructors and destructors must be inside a type
		if (context.GetTypeParent() == null)
		{
			return false;
		}

		var head = tokens[HEAD].To<FunctionToken>();
		var type = context.GetTypeParent();

		return type != null && (head.Name == Keywords.INIT.Identifier || head.Name == DESTRUCTOR_IDENTIFIER);
	}

	public override Node? Build(Context context, List<Token> tokens)
	{
		var head = tokens[HEAD].To<FunctionToken>();
		var body = tokens[BODY].To<ContentToken>();
		var type = (Type)context;

		if (head.Name == Keywords.INIT.Identifier)
		{
			var constructor = new Constructor(context, AccessModifier.PUBLIC, body.Tokens);
			constructor.Parameters = head.GetParameters(constructor);

			type.AddConstructor(constructor);

			return new FunctionDefinitionNode(constructor);
		}
		else
		{
			var destructor = new Constructor(context, AccessModifier.PUBLIC, body.Tokens);
			destructor.Parameters = head.GetParameters(destructor);

			type.AddDestructor(destructor);

			return new FunctionDefinitionNode(destructor);
		}
	}
}