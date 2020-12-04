using System;
using System.Collections.Generic;
using System.Linq;

public class IsPattern : Pattern
{
	public const int PRIORITY = 5;

	private const int IS = 1;
	private const int TYPE = 2;

	private const int MINIMUM_LENGTH = 3;

	// Pattern: $object is $type[<$1, $2, ..., $n>] [$name]
	public IsPattern() : base
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
		if (!tokens[IS].Is(Keywords.IS))
		{
			return false;
		}

		// Try to consume template arguments
		Try(Common.ConsumeTemplateArguments, state);

		// Try consuming variable name
		Consume(state, out Token? _, TokenType.IDENTIFIER);

		return true;
	}

	public override Node? Build(Context context, List<Token> tokens)
	{
		var source = Singleton.Parse(context, tokens.First());
		var type = Common.ReadTypeArgument(context, new Queue<Token>(tokens.Skip(TYPE)));

		if (type == null)
		{
			throw Errors.Get(tokens[TYPE].Position, "Could not understand the type");
		}

		var is_template_type = tokens.Exists(i => i.Is(Operators.LESS_THAN));
		var has_result_variable = is_template_type ? tokens.Last().Is(TokenType.IDENTIFIER) : tokens.Count > MINIMUM_LENGTH;

		if (has_result_variable)
		{
			var name = tokens.Last().To<IdentifierToken>().Value;
			var result = new Variable(context, type, VariableCategory.LOCAL, name, AccessModifier.PUBLIC);

			return new IsNode(source, type, result, tokens[IS].Position);
		}

		return new IsNode(source, type, null, tokens[IS].Position);
	}
}
