using System.Collections.Generic;
using System.Linq;

public class HasPattern : Pattern
{
	public const int PRIORITY = 5;

	private const int HAS = 1;
	private const int NAME = 2;

	// Pattern: $object has $name
	public HasPattern() : base
	(
		TokenType.DYNAMIC | TokenType.IDENTIFIER | TokenType.FUNCTION,
		TokenType.KEYWORD,
		TokenType.IDENTIFIER
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return tokens[HAS].Is(Keywords.HAS);
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		var source = Singleton.Parse(context, tokens.First());
		var name = tokens[NAME].To<IdentifierToken>();

		if (context.IsLocalVariableDeclared(name.Value))
		{
			throw Errors.Get(name.Position, $"Variable '{name.Value}' already exists in this context");
		}

		var variable = Variable.Create(context, Types.UNKNOWN, VariableCategory.LOCAL, name.Value, AccessModifier.PUBLIC);
		variable.Position = name.Position;

		var result = new VariableNode(variable, name.Position);

		return new HasNode(source, result, tokens[HAS].Position);
	}
}