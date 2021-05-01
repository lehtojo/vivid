using System.Collections.Generic;
using System.Linq;

public class VariableDeclarationPattern : Pattern
{
	public const int PRIORITY = 19;

	public const int NAME = 0;
	public const int COLON = 1;

	// Pattern 1: $name : $type [<$1, $2, ..., $n>]
	// Pattern 2: $name : ($1, $2, ..., $n) -> $type [<$1, $2, ..., $n>]
	public VariableDeclarationPattern() : base
	(
		TokenType.IDENTIFIER, TokenType.OPERATOR
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		if (!tokens[COLON].Is(Operators.COLON))
		{
			return false;
		}

		// Try to consume a type
		return Common.ConsumeType(state);
	}

	public override Node Build(Context context, PatternState state, List<Token> tokens)
	{
		var name = tokens[NAME].To<IdentifierToken>();

		if (context.IsLocalVariableDeclared(name.Value))
		{
			throw Errors.Get(name.Position, $"Variable '{name.Value}' already exists in this context");
		}

		if (name.Value == Function.SELF_POINTER_IDENTIFIER || name.Value == Lambda.SELF_POINTER_IDENTIFIER)
		{
			throw Errors.Get(name.Position, $"Can not declare variable called '{name.Value}' since the name is reserved");
		}

		var type = Common.ReadType(context, new Queue<Token>(tokens.Skip(COLON + 1)));

		var constant = context.Parent == null;
		var category = context.IsType ? VariableCategory.MEMBER : (constant ? VariableCategory.GLOBAL : VariableCategory.LOCAL);
		var modifiers = Modifier.DEFAULT | (constant ? Modifier.CONSTANT : 0);

		if (context.IsNamespace)
		{
			modifiers |= Modifier.STATIC;
		}

		var variable = new Variable(context, type, category, name.Value, modifiers) { Position = tokens[NAME].Position };

		return new VariableNode(variable, name.Position);
	}
}