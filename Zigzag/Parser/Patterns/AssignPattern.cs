using System.Collections.Generic;

class AssignPattern : Pattern
{
	public const int PRIORITY = 19;

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
		var operation = (OperatorToken)tokens[OPERATOR];
		return operation.Operator == Operators.ASSIGN;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		var destination = (IdentifierToken)tokens[DESTINATION];

		if (!context.IsVariableDeclared(destination.Value))
		{
			if (destination.Value == Function.THIS_POINTER_IDENTIFIER)
			{
				throw Errors.Get(destination.Position, $"Cannot declare variable called '{Function.THIS_POINTER_IDENTIFIER}' since the name is reserved");
			}

			var category = context.IsType ? VariableCategory.MEMBER : VariableCategory.LOCAL;

			var variable = new Variable
			(
				context,
				Types.UNKNOWN,
				category,
				destination.Value,
				AccessModifier.PUBLIC
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