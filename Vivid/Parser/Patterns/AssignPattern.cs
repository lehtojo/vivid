using System.Collections.Generic;

public class AssignPattern : Pattern
{
	public const int DESTINATION = 0;
	public const int OPERATOR = 1;

	// Pattern: $name = ...
	public AssignPattern() : base
	(
		TokenType.IDENTIFIER, TokenType.OPERATOR
	)
	{ Priority = 19; IsConsumable = false; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return tokens[OPERATOR].To<OperatorToken>().Operator == Operators.ASSIGN;
	}

	public override Node Build(Context context, ParserState state, List<Token> tokens)
	{
		// Do not remove the assign operator after building the tokens
		state.End--;

		var destination = tokens[DESTINATION].To<IdentifierToken>();
		var name = destination.Value;

		Variable? variable;

		if (!context.IsVariableDeclared(name))
		{
			// Ensure the name is not reserved
			if (name == Function.SELF_POINTER_IDENTIFIER || name == Lambda.SELF_POINTER_IDENTIFIER)
			{
				throw Errors.Get(destination.Position, $"Can not create variable with name '{name}' since the name is reserved");
			}

			// Determine the category and the modifiers of the variable
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

		// Static variables must be accessed using their parent types
		if (variable.IsStatic)
		{
			return new LinkNode(
				new TypeNode((Type)variable.Parent, destination.Position),
				new VariableNode(variable, destination.Position),
				destination.Position
			);
		}

		if (variable.IsMember)
		{
			var self = Common.GetSelfPointer(context, destination.Position);

			return new LinkNode(self, new VariableNode(variable, destination.Position), destination.Position);
		}

		return new VariableNode(variable, destination.Position);
	}
}