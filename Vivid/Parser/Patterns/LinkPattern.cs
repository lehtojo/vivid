﻿using System;
using System.Collections.Generic;
using System.Linq;

public class LinkPattern : Pattern
{
	public const int PRIORITY = 19;

	private const int LEFT = 0;
	private const int OPERATOR = 2;
	private const int RIGHT = 4;

	private const int STANDARD_TOKEN_COUNT = 5;

	// Pattern: ... [\n] . [\n] ...
	public LinkPattern() : base
	(
		TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.CONTENT | TokenType.DYNAMIC,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.OPERATOR,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.CONTENT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		// Ensure the operator is the dot operator
		if (!tokens[OPERATOR].Is(Operators.DOT))
		{
			return false;
		}

		// Try to consume template arguments
		if (tokens[RIGHT].Is(TokenType.IDENTIFIER))
		{
			Try(Common.ConsumeTemplateFunctionCall, state);
		}

		return true;
	}

	public override Node Build(Context environment, PatternState state, List<Token> tokens)
	{
		var template_arguments = Array.Empty<Type>();

		// When there are more tokens than the standard count, it means a template function has been consumed
		if (tokens.Count != STANDARD_TOKEN_COUNT)
		{
			template_arguments = Common.ReadTemplateArguments(environment, new Queue<Token>(tokens.Skip(STANDARD_TOKEN_COUNT)));
		}

		var left = Singleton.Parse(environment, tokens[LEFT]);

		// If the right operand is a content token, this is a cast expression
		if (tokens[RIGHT].Is(TokenType.CONTENT))
		{
			// Read the cast type from the content token
			var type = Common.ReadType(environment, new Queue<Token>(tokens[RIGHT].To<ContentToken>().Tokens));
			if (type == null) throw Errors.Get(tokens[RIGHT].Position, "Can not understand the cast");

			return new CastNode(left, new TypeNode(type, tokens[RIGHT].Position), tokens[OPERATOR].Position);
		}

		// Try to retrieve the primary context from the left token
		var primary = left.TryGetType();

		Node? right;

		// Ensure the primary context has been retrieved successfully
		if (primary == null)
		{
			// Since the primary context could not be retrieved, an unresolved link node must be returned
			if (template_arguments.Any())
			{
				var name = tokens[RIGHT].To<IdentifierToken>();
				var descriptor = new FunctionToken(name, tokens.Last().To<ContentToken>()) { Position = name.Position };

				right = new UnresolvedFunction(name.Value, template_arguments, name.Position).SetArguments(descriptor.GetParsedParameters(environment));
			}
			else
			{
				right = Singleton.GetUnresolved(environment, tokens[RIGHT]);
			}

			return new LinkNode(left, right, tokens[OPERATOR].Position);
		}

		// Try to create template function call if there are any template parameters
		if (template_arguments.Any())
		{
			var name = tokens[RIGHT].To<IdentifierToken>();
			var descriptor = new FunctionToken(name, tokens.Last().To<ContentToken>()) { Position = name.Position };

			right = Singleton.GetFunction(environment, primary, descriptor, template_arguments, true);
		}
		else
		{
			right = Singleton.Parse(environment, primary, tokens[RIGHT]);
		}

		// Try to build the right node as a virtual function or lambda call
		if (right is UnresolvedFunction function)
		{
			var types = function.Select(i => i.TryGetType()).ToList();

			// Try to form a virtual function call
			var position = tokens[OPERATOR].Position;
			var result = Common.TryGetVirtualFunctionCall(left, primary, function.Name, function, types, position);

			if (result != null) return result;

			// Try to form a lambda function call
			result = Common.TryGetLambdaCall(primary, left, function.Name, function, types);

			if (result != null)
			{
				result.Position = position;
				return result;
			}
		}

		return new LinkNode(left, right, tokens[OPERATOR].Position);
	}
}