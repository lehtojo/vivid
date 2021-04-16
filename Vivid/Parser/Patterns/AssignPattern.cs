using System.Collections.Generic;

public class AssignPattern : Pattern
{
	public const int PRIORITY = 19;

	public const int DESTINATION = 0;
	public const int OPERATOR = 1;

	// Pattern: $name = ...
	public AssignPattern() : base
	(
		TokenType.IDENTIFIER, TokenType.OPERATOR
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return tokens[OPERATOR].To<OperatorToken>().Operator == Operators.ASSIGN;
	}

	public override Node Build(Context context, PatternState state, List<Token> tokens)
	{
		var destination = tokens[DESTINATION].To<IdentifierToken>();

		Variable? variable;

		if (!context.IsVariableDeclared(destination.Value))
		{
			if (destination.Value == Function.SELF_POINTER_IDENTIFIER || destination.Value == Lambda.SELF_POINTER_IDENTIFIER)
			{
				throw Errors.Get(destination.Position, $"Can not declare variable with name '{destination.Value}' since the name is reserved");
			}

			var is_constant = !context.IsInsideFunction && !context.IsInsideType;
			var category = context.IsType ? VariableCategory.MEMBER : (is_constant ? VariableCategory.GLOBAL : VariableCategory.LOCAL);

			variable = new Variable(context, null, category, destination.Value, Modifier.DEFAULT | (is_constant ? Modifier.CONSTANT : 0))
			{
				Position = tokens[DESTINATION].Position
			};

			return new VariableNode(variable, destination.Position);
		}

		variable = context.GetVariable(destination.Value)!;

		if (variable.IsMember)
		{
			var self = Common.GetSelfPointer(context, destination.Position);

			return new LinkNode(self, new VariableNode(variable, destination.Position), destination.Position);
		}

		return new VariableNode(variable, destination.Position);
	}

	public override int GetEnd()
	{
		return 1;
	}
}