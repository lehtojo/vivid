using System.Collections.Generic;
using System;

public class ExtensionFunctionPattern : Pattern
{
	private const int PARAMETERS_OFFSET = 2;
	private const int BODY_OFFSET = 0;

	private const int TEMPLATE_FUNCTION_EXTENSION_TEMPLATE_ARGUMENTS_END_OFFSET = PARAMETERS_OFFSET + 1;
	private const int STANDARD_FUNCTION_EXTENSION_LAST_DOT_OFFSET = PARAMETERS_OFFSET + 1;

	// Pattern: $T1.$T2. ... .$Tn.$name [<$T1, $T2, ..., $Tn>] () [\n] {...}
	public ExtensionFunctionPattern() : base
	(
		TokenType.IDENTIFIER
	)
	{ Priority = 23; IsConsumable = false; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		// Optionally consume template arguments
		var backup = state.Save();
		if (!Common.ConsumeTemplateArguments(state)) state.Restore(backup);

		// Ensure the first operator is a dot operator
		if (!state.Consume(out Token? consumed, TokenType.OPERATOR) || !consumed!.Is(Operators.DOT)) return false;

		while (true)
		{
			// If there is a function token after the dot operator, this is the function to be added
			if (state.Consume(TokenType.FUNCTION)) break;

			// Consume a normal type or a template type
			if (!state.Consume(TokenType.IDENTIFIER)) return false;

			// Optionally consume template arguments
			backup = state.Save();
			if (!Common.ConsumeTemplateArguments(state)) state.Restore(backup);

			if (state.Consume(out consumed, TokenType.OPERATOR))
			{
				// If an operator was consumed, it must be a dot operator
				if (!consumed!.Is(Operators.DOT)) return false;
				continue;
			}

			if (state.Consume(out consumed, TokenType.PARENTHESIS))
			{
				// If parenthesis were consumed, it must be standard parenthesis
				if (!consumed!.Is(ParenthesisType.PARENTHESIS)) return false;
				break;
			}

			// There is an unexpected token
			return false;
		}

		// Optionally consume a line ending
		state.ConsumeOptional(TokenType.END);

		// The last token must be the body of the function
		return state.Consume(out consumed, TokenType.PARENTHESIS) && consumed!.Is(ParenthesisType.CURLY_BRACKETS);
	}

	private static bool IsTemplateFunction(List<Token> tokens)
	{
		return !tokens[tokens.Count - 1 - PARAMETERS_OFFSET].Is(TokenType.FUNCTION);
	}

	private static int FindTemplateArgumentsStart(List<Token> tokens)
	{
		var i = tokens.Count - 1 - TEMPLATE_FUNCTION_EXTENSION_TEMPLATE_ARGUMENTS_END_OFFSET;
		var j = 0;

		while (i >= 0)
		{
			var token = tokens[i];

			if (token.Is(Operators.LESS_THAN)) j--;
			else if (token.Is(Operators.GREATER_THAN)) j++;

			if (j == 0) break;

			i--;
		}

		return i;
	}

	private static Node CreateTemplateFunctionExtension(Context environment, List<Token> tokens)
	{
		// Find the starting index of the template arguments
		var i = FindTemplateArgumentsStart(tokens);
		if (i < 0) throw new ApplicationException("Invalid template function extension");

		// Collect all the tokens before the name of the extension function
		// NOTE: This excludes the dot operator
		var destination = Common.ReadType(environment, tokens.GetRange(0, i - 2));

		if (destination == null) throw new ApplicationException("Invalid template function extension");

		var template_parameters_start = i + 1;
		var template_parameters_end = tokens.Count - 1 - TEMPLATE_FUNCTION_EXTENSION_TEMPLATE_ARGUMENTS_END_OFFSET;
		var template_parameters = Common.GetTemplateParameters(tokens.GetRange(template_parameters_start, template_parameters_end - template_parameters_start), tokens[i].Position);
		
		var name = tokens[i - 1].To<IdentifierToken>();
		var parameters = tokens[tokens.Count - 1 - PARAMETERS_OFFSET].To<ParenthesisToken>();
		var body = tokens[tokens.Count - 1 - BODY_OFFSET].To<ParenthesisToken>();

		var descriptor = new FunctionToken(name, parameters) { Position = name.Position };

		return new ExtensionFunctionNode(destination, descriptor, template_parameters, body.Tokens, descriptor.Position, body.End);
	}

	private static Node? CreateStandardFunctionExtension(Context environment, List<Token> tokens)
	{
		var destination = Common.ReadType(environment, tokens.GetRange(0, tokens.Count - 1 - STANDARD_FUNCTION_EXTENSION_LAST_DOT_OFFSET));
		if (destination == null) throw new ApplicationException("Invalid template function extension");

		var descriptor = tokens[tokens.Count - 1 - PARAMETERS_OFFSET].To<FunctionToken>();
		var body = tokens[tokens.Count - 1 - BODY_OFFSET].To<ParenthesisToken>();

		return new ExtensionFunctionNode(destination, descriptor, body.Tokens, descriptor.Position, body.End);
	}

	public override Node? Build(Context environment, ParserState state, List<Token> tokens)
	{
		if (IsTemplateFunction(tokens))
		{
			return CreateTemplateFunctionExtension(environment, tokens);
		}

		return CreateStandardFunctionExtension(environment, tokens);
	}
}