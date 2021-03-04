using System.Collections.Generic;
using System.Linq;

public class TemplateTypePattern : Pattern
{
	public const int PRIORITY = 22;

	public const int NAME = 0;
	public const int TEMPLATE_ARGUMENTS = 1;
	public const int BODY = 3;

	public const int TEMPLATE_ARGUMENTS_START = 2;
	public const int TEMPLATE_ARGUMENTS_END = 3;

	// Pattern: $name <$1, $2, ... $n> [\n] {}
	public TemplateTypePattern() : base
	(
		TokenType.IDENTIFIER
	)
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		// Pattern: $name <$1, $2, ... $n> [\n] {}

		if (!Consume(state, out Token? opening, TokenType.OPERATOR) || opening!.To<OperatorToken>().Operator != Operators.LESS_THAN)
		{
			return false;
		}

		while (true)
		{
			if (!Consume(state, out Token? _, TokenType.IDENTIFIER))
			{
				return false;
			}

			if (!Consume(state, out Token? consumed, TokenType.OPERATOR))
			{
				return false;
			}

			if (consumed!.To<OperatorToken>().Operator == Operators.GREATER_THAN)
			{
				break;
			}

			if (consumed!.To<OperatorToken>().Operator == Operators.COMMA)
			{
				continue;
			}

			return false;
		}

		// Optionally consume a line-ending
		Consume(state, out Token? _, TokenType.END | TokenType.OPTIONAL);

		return Consume(state, out Token? parenthesis, TokenType.CONTENT) && parenthesis!.To<ContentToken>().Type == ParenthesisType.CURLY_BRACKETS;
	}

	public override Node Build(Context context, PatternState state, List<Token> tokens)
	{
		var name = tokens[NAME].To<IdentifierToken>();
		var body = tokens.Last().To<ContentToken>();

		var template_argument_tokens = tokens.GetRange(TEMPLATE_ARGUMENTS_START, tokens.Count - TEMPLATE_ARGUMENTS_END - TEMPLATE_ARGUMENTS_START);
		var template_argument_names = Common.GetTemplateParameterNames(template_argument_tokens, tokens[TEMPLATE_ARGUMENTS].Position);

		var blueprint = new List<Token>() { (Token)name.Clone(), (Token)body.Clone() };

		var template_type = new TemplateType(context, name.Value, Modifier.DEFAULT, blueprint, template_argument_names, name.Position);

		return new TypeNode(template_type, name.Position) { IsDefinition = true };
	}
}
