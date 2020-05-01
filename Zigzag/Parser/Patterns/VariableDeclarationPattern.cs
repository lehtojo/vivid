using System.Collections.Generic;

public class VariableDeclarationPattern : Pattern
{
	public const int PRIORITY = 2;

	public const int NAME = 0;
	public const int OPERATOR = 1;
	public const int TYPE = 2;

	// Example: apples: num[]
	public VariableDeclarationPattern() : base
	(
        TokenType.IDENTIFIER, TokenType.OPERATOR, TokenType.IDENTIFIER
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		return tokens[OPERATOR].To<OperatorToken>().Operator == Operators.COLON;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		var name = tokens[NAME].To<IdentifierToken>();

		if (context.IsVariableDeclared(name.Value))
		{
			throw Errors.Get(name.Position, $"Variable '{name.Value}' already exists in this context");
		}

		if (name.Value == Function.THIS_POINTER_IDENTIFIER)
		{
			throw Errors.Get(name.Position, $"Cannot declare variable called '{Function.THIS_POINTER_IDENTIFIER}' since the name is reserved");
		}

		var type_name = tokens[TYPE].To<IdentifierToken>().Value;
		var type = context.GetType(type_name) ?? throw Errors.Get(tokens[TYPE].Position, $"Couldn't resolve variable type '{type_name}'");

		var category = context.IsType ? VariableCategory.MEMBER : VariableCategory.LOCAL;

		var variable = new Variable
		(
			context,
			type,
			category,
			name.Value,
			AccessModifier.PUBLIC
		);

		return new VariableNode(variable);
	}
}
