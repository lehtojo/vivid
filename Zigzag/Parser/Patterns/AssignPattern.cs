using System;
using System.Collections.Generic;
using System.Text;

class AssignPattern : Pattern
{
	public const int PRIORITY = 19;

	public const int DESTINATION = 0;
	public const int OPERATOR = 1;
	public const int SOURCE = 2;

	// (a-z) = ...
	public AssignPattern() : base
	(
		TokenType.IDENTIFIER, TokenType.OPERATOR, TokenType.IDENTIFIER | TokenType.NUMBER | TokenType.FUNCTION | TokenType.DYNAMIC
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, List<Token> tokens)
	{
		var operation = tokens[OPERATOR] as OperatorToken;
		return operation.Operator == Operators.ASSIGN;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		var destination = tokens[DESTINATION] as IdentifierToken;

		if (!context.IsVariableDeclared(destination.Value))
		{
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

		return new VariableNode(context.GetVariable(destination.Value));
	}

	public override int GetEnd()
	{
		return 1;
	}
}