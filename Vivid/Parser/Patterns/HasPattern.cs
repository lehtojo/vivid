using System.Collections.Generic;
using System.Linq;

public class HasPattern : Pattern
{
	private const int HAS = 1;
	private const int NAME = 2;

	// Pattern: $object has [not] $name
	public HasPattern() : base
	(
		TokenType.DYNAMIC | TokenType.IDENTIFIER | TokenType.FUNCTION,
		TokenType.KEYWORD,
		TokenType.IDENTIFIER
	)
	{ Priority = 16; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		return tokens[HAS].Is(Keywords.HAS) || tokens[HAS].Is(Keywords.HAS_NOT);
	}

	public override Node? Build(Context context, ParserState state, List<Token> tokens)
	{
		var negate = tokens[HAS].Is(Keywords.HAS_NOT);

		var source = Singleton.Parse(context, tokens.First());
		var name = tokens[NAME].To<IdentifierToken>();
		var position = name.Position;

		if (context.IsLocalVariableDeclared(name.Value))
		{
			throw Errors.Get(position, $"Variable '{name.Value}' already exists in this context");
		}

		var variable = Variable.Create(context, null, VariableCategory.LOCAL, name.Value, Modifier.DEFAULT);
		variable.Position = position;

		var result = new HasNode(source, new VariableNode(variable, position), tokens[HAS].Position);

		return negate ? new NotNode(result, false, result.Position) : result;
	}
}