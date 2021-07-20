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
		var name = destination.Value;

		Variable? variable;

		if (!context.IsVariableDeclared(name))
		{
			if (name == Function.SELF_POINTER_IDENTIFIER || name == Lambda.SELF_POINTER_IDENTIFIER)
			{
				throw Errors.Get(destination.Position, $"Can not declare variable with name '{name}' since the name is reserved");
			}

			var constant = context.Parent == null;
			var category = context.IsType ? VariableCategory.MEMBER : (constant ? VariableCategory.GLOBAL : VariableCategory.LOCAL);
			var modifiers = Modifier.DEFAULT | (constant ? Modifier.CONSTANT : 0);

			if (context.IsNamespace)
			{
				modifiers |= Modifier.STATIC;
			}

			variable = new Variable(context, null, category, name, modifiers) { Position = destination.Position };

			return new VariableNode(variable, destination.Position);
		}

		variable = context.GetVariable(name)!;

		if (variable.IsStatic)
		{
			return new LinkNode(
				new TypeNode((Type)variable.Context, destination.Position),
				new VariableNode(variable, destination.Position),
				destination.Position
			);
		}
		else if (variable.IsMember)
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