using System.Collections.Generic;
using System.Linq;

public class TemplateFunctionPattern : Pattern
{
	public const int PRIORITY = 23;

	public const int NAME = 0;
	public const int TEMPLATE_ARGUMENTS = 1;
	public const int BODY = 4;

	public const int PARAMETERS = -3;

	public const int TEMPLATE_ARGUMENTS_START = 2;
	public const int TEMPLATE_ARGUMENTS_END = 4;

	// Pattern: $name <$1, $2, ... $n> (...) [\n] {}
	public TemplateFunctionPattern() : base
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
		// Pattern: $name <$1, $2, ... $n> (...) [\n] {}

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

		// Now there must be function parameters next
		if (!Consume(state, out Token? parameters, TokenType.CONTENT) || parameters!.To<ContentToken>().Type != ParenthesisType.PARENTHESIS)
		{
			return false;
		}

		// Optionally consume a line-ending
		Consume(state, out Token? _, TokenType.END | TokenType.OPTIONAL);

		// Consume curly brackets
		return Consume(state, out Token? parenthesis, TokenType.CONTENT) && parenthesis!.To<ContentToken>().Type == ParenthesisType.CURLY_BRACKETS;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		var name = tokens[NAME].To<IdentifierToken>();

		var template_argument_tokens = tokens.GetRange(TEMPLATE_ARGUMENTS_START, tokens.Count - TEMPLATE_ARGUMENTS_END - TEMPLATE_ARGUMENTS_START);
		var template_argument_names = Common.GetTemplateParameterNames(template_argument_tokens, tokens[TEMPLATE_ARGUMENTS].Position);

		var parameters = tokens[tokens.Count + PARAMETERS].To<ContentToken>();
		var body = tokens.Last().To<ContentToken>();

		var blueprint = new List<Token>() { new FunctionToken(name, parameters), body };

		var template_function = new TemplateFunction(context, AccessModifier.PUBLIC, name.Value, blueprint, template_argument_names);
		template_function.Position = name.Position;
		template_function.Parameters.AddRange(((FunctionToken)blueprint.First().Clone()).GetParameters(template_function));
		
		context.Declare(template_function);

		return new FunctionDefinitionNode(template_function, name.Position);
	}
}