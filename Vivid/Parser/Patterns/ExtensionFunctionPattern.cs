using System;
using System.Collections.Generic;
using System.Linq;

public class ExtensionFunctionPattern : Pattern
{
	// Pattern: ($type) . $name [<$T1, $T2, ..., $Tn>] () [: $return-type] [\n] {...}
	public ExtensionFunctionPattern() : base(TokenType.PARENTHESIS)
	{
		Priority = 23;
		IsConsumable = false;
	}

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		// Consume a dot operator
		if (!state.ConsumeOperator(Operators.DOT)) return false;

		// Attempt to consume a function token. If that fails, we expect a template function extension.
		if (!state.Consume(TokenType.FUNCTION))
		{
			// Consume the name
			if (!state.Consume(TokenType.IDENTIFIER)) return false;

			// Consume the template parameters
			if (!Common.ConsumeTemplateParameters(state)) return false;

			// Consume parenthesis
			if (!state.Consume(TokenType.PARENTHESIS)) return false;
		}

		// Look for a return type
		if (state.ConsumeOperator(Operators.COLON))
		{
			// Expected: ($type).$name [<$T1, $T2, ..., $Tn>] () : $return-type [\n] {...}
			if (!Common.ConsumeType(state)) return false;
		}

		// Optionally consume a line ending
		state.ConsumeOptional(TokenType.END);

		// The last token must be the body of the function
		var next = state.Peek();
		if (next == null || !next.Is(ParenthesisType.CURLY_BRACKETS)) return false;
		
		state.Consume();
		return true;
	}

	private static bool IsTemplateFunctionExtension(List<Token> tokens)
	{
		return tokens[2].Type != TokenType.FUNCTION;
	}

	private static int GetTemplateParameters(List<Token> tokens, List<string> parameters)
	{
		var i = 4; // Pattern: ($type) . $name < $T1, $T2, ..., $Tn > () [: $return-type] [\n] {...}

		for (; i + 1 < tokens.Count; i += 2)
		{
			parameters.Add(tokens[i].To<IdentifierToken>().Value);
			if (tokens[i + 1].Is(Operators.GREATER_THAN)) return i + 2;
		}

		throw new ApplicationException("Failed to find the end of template parameters");
	}

	private static Node CreateTemplateFunctionExtension(Context environment, Type destination, ParserState state, List<Token> tokens, List<Token> body)
	{
		// Extract the extension function name
		var name = tokens[2].To<IdentifierToken>();

		// Extract the template parameters and the index of the parameters
		var template_parameters = new List<string>();
		var parameters_index = GetTemplateParameters(tokens, template_parameters);

		// Create a function token from the name and parameters (helper object)
		var descriptor = new FunctionToken(new IdentifierToken(name.Value), tokens[parameters_index].To<ParenthesisToken>(), name.Position);

		// Extract the return type if it is specified
		var return_type_tokens = new List<Token>();
		var colon_index = parameters_index + 1;

		if (tokens[colon_index].Is(Operators.COLON))
		{
			var return_type_start = colon_index;
			var return_type_end = tokens.Count - 2;
			return_type_tokens = tokens.GetRange(return_type_start, return_type_end - return_type_start);
		}

		return new ExtensionFunctionNode(destination, descriptor, template_parameters, return_type_tokens, body, tokens.First().Position, tokens[tokens.Count - 1].To<ParenthesisToken>().End);
	}

	private static Node CreateStandardFunctionExtension(Context environment, Type destination, ParserState state, List<Token> tokens, List<Token> body)
	{
		// // Pattern: ($type) . function() [: $return-type] [\n] {...}
		var descriptor = tokens[2].To<FunctionToken>();

		// Extract the return type if it is specified
		var return_type_tokens = new List<Token>();
		var colon_index = 3;

		if (tokens[colon_index].Is(Operators.COLON))
		{
			var return_type_start = colon_index;
			var return_type_end = tokens.Count - 2;
			return_type_tokens = tokens.GetRange(return_type_start, return_type_end - return_type_start);
		}

		return new ExtensionFunctionNode(destination, descriptor, return_type_tokens, body, tokens[0].Position, tokens[tokens.Count - 1].To<ParenthesisToken>().End);
	}

	public override Node Build(Context environment, ParserState state, List<Token> tokens)
	{
		var destination = Common.ReadType(environment, tokens[0].To<ParenthesisToken>().Tokens) ?? throw Errors.Get(tokens[0].Position, "Can not resolve the destination type");

		// Extract the body tokens
		var body = tokens[tokens.Count - 1].To<ParenthesisToken>().Tokens;

		if (IsTemplateFunctionExtension(tokens)) return CreateTemplateFunctionExtension(environment, destination, state, tokens, body);
		return CreateStandardFunctionExtension(environment, destination, state, tokens, body);
	}
}