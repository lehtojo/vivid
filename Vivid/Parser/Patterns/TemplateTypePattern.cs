using System.Collections.Generic;
using System.Linq;

public class TemplateTypePattern : Pattern
{
	public const int PRIORITY = 22;

	public const int NAME = 0;
	public const int TEMPLATE_PARAMETERS = 1;
	public const int BODY = 3;

	public const int TEMPLATE_PARAMETERS_START = 2;
	public const int TEMPLATE_PARAMETERS_END = 3;

	// Pattern: $name <$1, $2, ... $n> [\n] {...}
	public TemplateTypePattern() : base
	(
		TokenType.IDENTIFIER
	) { }

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
			if (!Consume(state, TokenType.IDENTIFIER))
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
		Consume(state, TokenType.END | TokenType.OPTIONAL);

		return Consume(state, out Token? parenthesis, TokenType.PARENTHESIS) && parenthesis!.To<ParenthesisToken>().Opening == ParenthesisType.CURLY_BRACKETS;
	}

	public override Node Build(Context context, PatternState state, List<Token> tokens)
	{
		var name = tokens[NAME].To<IdentifierToken>();
		var body = tokens.Last().To<ParenthesisToken>();

		var template_parameter_tokens = tokens.GetRange(TEMPLATE_PARAMETERS_START, tokens.Count - TEMPLATE_PARAMETERS_END - TEMPLATE_PARAMETERS_START);
		var template_parameters = Common.GetTemplateParameters(template_parameter_tokens, tokens[TEMPLATE_PARAMETERS].Position);

		var blueprint = new List<Token>() { (Token)name.Clone(), (Token)body.Clone() };

		var template_type = new TemplateType(context, name.Value, Modifier.DEFAULT, blueprint, template_parameters, name.Position);

		return new TypeNode(template_type, name.Position) { IsDefinition = true };
	}
}
