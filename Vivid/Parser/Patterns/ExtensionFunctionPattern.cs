using System.Collections.Generic;
using System;

public class ExtensionFunctionPattern : Pattern
{
	public const int PRIORITY = 23;

	private const int PARAMETERS_OFFSET = 2;
	private const int BODY_OFFSET = 0;

	private const int TEMPLATE_FUNCTION_EXTENSION_TEMPLATE_ARGUMENTS_END_OFFSET = PARAMETERS_OFFSET + 1;
	private const int STANDARD_FUNCTION_EXTENSION_LAST_DOT_OFFSET = PARAMETERS_OFFSET + 1;

	// Examples:
	// Player.spawn(position: Vector) [\n] {...}

	// Pattern 1: $T1.$T2. ... .$Tn.$name [<$T1, $T2, ..., $Tn>] () [\n] {...}
	public ExtensionFunctionPattern() : base
	(
	   TokenType.IDENTIFIER
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		// Optionally consume template arguments
		Try(Common.ConsumeTemplateArguments, state);

		// Ensure the first operator is a dot operator
		if (!Consume(state, out Token? consumed, TokenType.OPERATOR) || !consumed!.Is(Operators.DOT)) return false;

		while (true)
		{
			// If there is a function token after the dot operator, this is the function to be added
			if (Consume(state, TokenType.FUNCTION)) break;

			// Consume a normal type or a template type
			if (!Consume(state, TokenType.IDENTIFIER)) return false;
			Try(Common.ConsumeTemplateArguments, state);

			if (Consume(state, out consumed, TokenType.OPERATOR))
			{
				// If an operator was consumed, it must be a dot operator
				if (!consumed!.Is(Operators.DOT)) return false;
				continue;
			}

			if (Consume(state, out consumed, TokenType.CONTENT))
			{
				// If parenthesis were consumed, it must be standard parenthesis
				if (!consumed!.Is(ParenthesisType.PARENTHESIS)) return false;
				break;
			}

			// There is an unexpected token
			return false;
		}

		// Optionally consume a line ending
		Consume(state, TokenType.END | TokenType.OPTIONAL);

		// The last token must be the body of the function
		return Consume(state, out consumed, TokenType.CONTENT) && consumed!.Is(ParenthesisType.CURLY_BRACKETS);
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
		var queue = new Queue<Token>(tokens.GetRange(0, i - 2));
		var destination = Common.ReadType(environment, queue);

		if (destination == null) throw new ApplicationException("Invalid template function extension");

		var template_parameters_start = i + 1;
		var template_parameters_end = tokens.Count - 1 - TEMPLATE_FUNCTION_EXTENSION_TEMPLATE_ARGUMENTS_END_OFFSET;
		var template_parameters = Common.GetTemplateParameters(tokens.GetRange(template_parameters_start, template_parameters_end - template_parameters_start), tokens[i].Position);
		
		var name = tokens[i - 1].To<IdentifierToken>();
		var parameters = tokens[tokens.Count - 1 - PARAMETERS_OFFSET].To<ContentToken>();
		var body = tokens[tokens.Count - 1 - BODY_OFFSET].To<ContentToken>();

		var descriptor = new FunctionToken(name, parameters) { Position = name.Position };

		return new ExtensionFunctionNode(destination, descriptor, template_parameters, body.Tokens, descriptor.Position, body.End);
	}

	private static Node? CreateStandardFunctionExtension(Context environment, List<Token> tokens)
	{
		var queue = new Queue<Token>(tokens.GetRange(0, tokens.Count - 1 - STANDARD_FUNCTION_EXTENSION_LAST_DOT_OFFSET));
		var destination = Common.ReadType(environment, queue);

		if (destination == null) throw new ApplicationException("Invalid template function extension");

		var descriptor = tokens[tokens.Count - 1 - PARAMETERS_OFFSET].To<FunctionToken>();
		var body = tokens[tokens.Count - 1 - BODY_OFFSET].To<ContentToken>();

		return new ExtensionFunctionNode(destination, descriptor, body.Tokens, descriptor.Position, body.End);
	}

	public override Node? Build(Context environment, PatternState state, List<Token> tokens)
	{
		if (IsTemplateFunction(tokens))
		{
			return CreateTemplateFunctionExtension(environment, tokens);
		}

		return CreateStandardFunctionExtension(environment, tokens);
	}
}