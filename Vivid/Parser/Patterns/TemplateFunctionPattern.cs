using System.Collections.Generic;
using System.Linq;

public class TemplateFunctionPattern : Pattern
{
	public const int PRIORITY = 23;

	public const int NAME = 0;
	public const int PARAMETERS_OFFSET = 1;

	public const int TEMPLATE_PARAMETERS_START = 2;
	public const int TEMPLATE_PARAMETERS_END = 4;

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

		// Try to consume a function body
		if (Common.ConsumeBody(state))
		{
			return true;
		}

		// If there's an implication operator next, then this is a template function
		return Try(state, () => Consume(state, out Token? token, TokenType.OPERATOR) && token!.Is(Operators.IMPLICATION));
	}

	public override Node Build(Context context, PatternState state, List<Token> tokens)
	{
		var name = tokens[NAME].To<IdentifierToken>();
		var blueprint = (ContentToken?)null;

		if (tokens.Last().Is(ParenthesisType.CURLY_BRACKETS))
		{
			blueprint = tokens.Last().To<ContentToken>();
		}

		var template_parameters_end = tokens.FindLastIndex(tokens.Count - 1, i => i.Is(Operators.GREATER_THAN));

		if (template_parameters_end == -1)
		{
			throw Errors.Get(tokens[TEMPLATE_PARAMETERS_START].Position, "Could not find the end of the template parameters");
		}

		var template_parameter_tokens = tokens.GetRange(TEMPLATE_PARAMETERS_START, template_parameters_end - TEMPLATE_PARAMETERS_START);
		var template_parameter_names = Common.GetTemplateParameterNames(template_parameter_tokens, tokens[TEMPLATE_PARAMETERS_START + 1].Position);

		var parameters = tokens[template_parameters_end + PARAMETERS_OFFSET].To<ContentToken>();
		var descriptor = new FunctionToken(name, parameters) { Position = name.Position };
		
		var template_function = new TemplateFunction(context, Modifier.PUBLIC, name.Value, template_parameter_names)
		{
			Position = name.Position
		};

		// Declare a self pointer if the function is a member of a type, since consuming the body may require it
		if (template_function.IsMember)
		{
			template_function.DeclareSelfPointer();
		}

		if (blueprint == null)
		{
			// Take the implication token into the blueprint as well
			var result = new List<Token>() { tokens.Last() };

			if (!Common.ConsumeBlock(template_function, state, result))
			{
				throw Errors.Get(descriptor.Position, "Short template function has an empty body");
			}

			blueprint = new ContentToken(result) { Type = ParenthesisType.CURLY_BRACKETS };
		}

		template_function.Blueprint.Add(descriptor);
		template_function.Blueprint.Add(blueprint);

		template_function.Parameters.AddRange(((FunctionToken)descriptor.Clone()).GetParameters(template_function));
		
		context.Declare(template_function);

		return new FunctionDefinitionNode(template_function, name.Position);
	}
}