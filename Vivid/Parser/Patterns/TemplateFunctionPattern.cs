using System.Collections.Generic;
using System.Linq;

public class TemplateFunctionPattern : Pattern
{
	public const int TEMPLATE_PARAMETERS_START = 2;

	// Pattern: $name <$1, $2, ... $n> (...) [: $return-type] [\n] {...}
	public TemplateFunctionPattern() : base
	(
		TokenType.IDENTIFIER
	)
	{ Priority = 23; IsConsumable = false; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		// Pattern: $name <$1, $2, ... $n> (...) [\n] {}
		if (!state.ConsumeOperator(Operators.LESS_THAN)) return false;

		while (true)
		{
			// Expect template parameter
			if (!state.Consume(TokenType.IDENTIFIER)) return false;

			// Expect an operator, either a comma or the end of the template parameters
			if (!state.Consume(out Token? consumed, TokenType.OPERATOR)) return false;

			// Stop if we reached the end of template parameters
			if (consumed!.To<OperatorToken>().Operator == Operators.GREATER_THAN) break;

			// If we consumed a comma, expect another template parameter
			if (consumed!.To<OperatorToken>().Operator == Operators.COMMA) continue;

			return false;
		}

		// Now there must be function parameters next
		if (!state.ConsumeParenthesis(ParenthesisType.PARENTHESIS)) return false;

		// Optionally consume return type
		if (state.ConsumeOperator(Operators.COLON))
		{
			Common.ConsumeType(state);
		}

		// Optionally consume a line ending
		state.Consume(TokenType.END);

		// Consume the function body
		return state.ConsumeParenthesis(ParenthesisType.CURLY_BRACKETS);
	}

	public override Node Build(Context context, ParserState state, List<Token> tokens)
	{
		var name = tokens.First().To<IdentifierToken>();
		var blueprint = tokens.Last().To<ParenthesisToken>();
		var start = name.Position;
		var end = blueprint.End;

		// Try to find the start of the optional return type
		var parameters_index = 0;
		var colon_index = tokens.FindIndex(i => i.Is(Operators.COLON));

		if (colon_index >= 0)
		{
			// Parameters are just to the left of the colon
			parameters_index = colon_index - 1;
		}
		else
		{
			// Index of the parameters can be determined from the end of tokens, because the user did not add the return type
			// Case 1: $name <$1, $2, ... $n> (...) {...}
			// Case 2: $name <$1, $2, ... $n> (...) \n {...}
			parameters_index = tokens.Count - 2;
			if (tokens[parameters_index].Type == TokenType.END) { parameters_index--; }
		}

		// Extract the template parameters
		var template_parameters_end = parameters_index - 1;
		var template_parameter_tokens = tokens.GetRange(TEMPLATE_PARAMETERS_START, template_parameters_end - TEMPLATE_PARAMETERS_START);
		var template_parameters = Common.GetTemplateParameters(template_parameter_tokens, tokens[TEMPLATE_PARAMETERS_START + 1].Position);

		if (!template_parameters.Any()) throw Errors.Get(start, "Expected at least one template parameter");

		var parameters = tokens[parameters_index].To<ParenthesisToken>();
		var descriptor = new FunctionToken(name, parameters) { Position = start };

		// Create the template function
		var template_function = new TemplateFunction(context, Modifier.DEFAULT, name.Value, template_parameters, parameters.Tokens, start, end);

		// Add parameters to the template function
		template_function.Parameters.AddRange(((FunctionToken)descriptor.Clone()).GetParameters(template_function));

		// Save the created blueprint
		template_function.Blueprint.Add(descriptor);
		template_function.Blueprint.AddRange(tokens.GetRange(parameters_index + 1, tokens.Count - (parameters_index + 1)));

		// Declare the template function
		context.Declare(template_function);

		return new FunctionDefinitionNode(template_function, start);
	}
}