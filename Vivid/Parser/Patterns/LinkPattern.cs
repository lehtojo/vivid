using System.Collections.Generic;
using System;
using System.Linq;

public class LinkPattern : Pattern
{
	public const int PRIORITY = 19;

	private const int LEFT = 0;
	private const int OPERATOR = 2;
	private const int RIGHT = 4;

	private const int STANDARD_TOKEN_COUNT = 5;

	// ... [\n] . [\n] ...
	public LinkPattern() : base
	(
		TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.CONTENT | TokenType.DYNAMIC,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.OPERATOR,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.FUNCTION | TokenType.IDENTIFIER
	)
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		var operation = (OperatorToken)tokens[OPERATOR];

		// The operator between left and right token must be dot
		if (operation.Operator != Operators.DOT)
		{
			return false;
		}

		// When left token is dynamic, it must be contextable
		if (tokens[LEFT].Type == TokenType.DYNAMIC)
		{
			return tokens[LEFT].To<DynamicToken>().Node is IType;
		}

		// Try to consume a template function call, therefore an identifier token in the right hand side
		if (tokens[RIGHT].Is(TokenType.IDENTIFIER))
		{
			Try(Common.ConsumeTemplateFunctionCall, state);
		}

		return true;
	}

	public override Node Build(Context environment, List<Token> tokens)
	{
		var template_arguments = Array.Empty<Type>();

		// When there are more tokens than the standard count, it means a template function has been consumed
		if (tokens.Count != STANDARD_TOKEN_COUNT)
		{
			template_arguments = Common.ReadTemplateArguments(environment, new Queue<Token>(tokens.Skip(STANDARD_TOKEN_COUNT)));
		}

		// Try to retrieve the primary context from the left token
		var left = Singleton.Parse(environment, tokens[LEFT]);
		var primary = left.TryGetType();

		Node? right;

		// Ensure the primary context has been retrieved successfully
		if (primary == null)
		{
			// Since the primary context could not be retrieved, an unresolved link node must be returned
			if (template_arguments.Any())
			{
				right = new UnresolvedFunction(tokens[RIGHT].To<IdentifierToken>().Value, template_arguments);
			}
			else
			{
				right = Singleton.GetUnresolved(environment, tokens[RIGHT]);
			}

			return new LinkNode(left, right);
		}

		// Try to create template function call if there are any template parameters
		if (template_arguments.Any())
		{
			var name = tokens[RIGHT].To<IdentifierToken>();
			var descriptor = new FunctionToken(name, tokens.Last().To<ContentToken>());

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
			var result = Common.TryGetVirtualFunctionCall(left, primary, function.Name, function, types);

			if (result != null)
			{
				return result;
			}

			// Try to form a lambda function call
			result = Common.TryGetLambdaCall(primary, left, function.Name, function, types);

			if (result != null)
			{
				return result;
			}
		}

		return new LinkNode(left, right);
	}
}