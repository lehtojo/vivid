using System.Collections.Generic;

class AssignPattern : Pattern
{
	public const int PRIORITY = 18;

	public const int DESTINATION = 0;
	public const int OPERATOR = 1;
	public const int SOURCE = 2;

	// (a-z) = ...
	public AssignPattern() : base
	(
		TokenType.IDENTIFIER, TokenType.OPERATOR, TokenType.OBJECT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		return tokens[OPERATOR].To<OperatorToken>().Operator == Operators.ASSIGN;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		var destination = tokens[DESTINATION].To<IdentifierToken>();

		if (!context.IsVariableDeclared(destination.Value))
		{
			if (destination.Value == Function.THIS_POINTER_IDENTIFIER)
			{
				throw Errors.Get(destination.Position, $"Cannot declare variable called '{Function.THIS_POINTER_IDENTIFIER}' since the name is reserved");
			}

			var category = context.IsType ? VariableCategory.MEMBER : VariableCategory.LOCAL;
			var is_constant = !context.IsInsideFunction && !context.IsInsideType;

			var variable = new Variable
			(
				context,
				Types.UNKNOWN,
				category,
				destination.Value,
				AccessModifier.PUBLIC | (is_constant ? AccessModifier.CONSTANT : 0)
			);

			return new VariableNode(variable);
		}

		return new VariableNode(context.GetVariable(destination.Value)!);
	}

	public override int GetEnd()
	{
		return 1;
	}
}