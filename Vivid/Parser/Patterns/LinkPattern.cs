using System;
using System.Collections.Generic;
using System.Linq;

public class LinkPattern : Pattern
{
	private const int LEFT = 0;
	private const int OPERATOR = 2;
	private const int RIGHT = 4;

	private const int STANDARD_TOKEN_COUNT = 5;

	// Pattern: ... [\n] . [\n] ...
	public LinkPattern() : base
	(
		TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.PARENTHESIS | TokenType.DYNAMIC,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.OPERATOR,
		TokenType.END | TokenType.OPTIONAL,
		TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.PARENTHESIS
	)
	{ Priority = 19; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		// Ensure the operator is the dot operator
		if (!tokens[OPERATOR].Is(Operators.DOT)) return false;

		// Try to consume template arguments
		if (tokens[RIGHT].Is(TokenType.IDENTIFIER))
		{
			var backup = state.Save();
			if (!Common.ConsumeTemplateFunctionCall(state)) state.Restore(backup);
		}

		return true;
	}

	private LinkNode BuildTemplateFunctionCall(Context context, List<Token> tokens, Node left)
	{
		// Load the properties of the template function call
		var name = tokens[RIGHT].To<IdentifierToken>();
		var descriptor = new FunctionToken(name, tokens.Last().To<ParenthesisToken>());
		descriptor.Position = name.Position;
		var template_arguments = Common.ReadTemplateArguments(context, tokens, RIGHT + 1);

		var primary = left.TryGetType();
		var right = (Node?)null;

		if (primary != null)
		{
			right = Singleton.GetFunction(context, primary, descriptor, template_arguments, true);
			return new LinkNode(left, right, tokens[OPERATOR].Position);
		}

		right = new UnresolvedFunction(name.Value, template_arguments, descriptor.Position);
		right.To<UnresolvedFunction>().SetArguments(descriptor.Parse(context));
		return new LinkNode(left, right, tokens[OPERATOR].Position);
	}

	public override Node Build(Context environment, ParserState state, List<Token> tokens)
	{
		var left = Singleton.Parse(environment, tokens[LEFT]);

		// When there are more tokens than the standard count, it means a template function has been consumed
		if (tokens.Count != STANDARD_TOKEN_COUNT) return BuildTemplateFunctionCall(environment, tokens, left);

		// If the right operand is a parenthesis token, this is a cast expression
		if (tokens[RIGHT].Is(TokenType.PARENTHESIS))
		{
			// Read the cast type from the content token
			var type = Common.ReadType(environment, tokens[RIGHT].To<ParenthesisToken>().Tokens);

			if (type == null) throw Errors.Get(tokens[RIGHT].Position, "Can not understand the cast");

			return new CastNode(left, new TypeNode(type, tokens[RIGHT].Position), tokens[OPERATOR].Position);
		}

		// Try to retrieve the primary context from the left token
		var primary = left.TryGetType();
		var right = (Node?)null;
		var token = tokens[RIGHT];

		if (primary == null)
		{
			// Since the primary context could not be retrieved, an unresolved link node must be returned
			if (token.Is(TokenType.IDENTIFIER))
			{
				right = new UnresolvedIdentifier(token.To<IdentifierToken>().Value, token.Position);
			}
			else if (token.Is(TokenType.FUNCTION))
			{
				right = new UnresolvedFunction(token.To<FunctionToken>().Name, token.Position).SetArguments(token.To<FunctionToken>().Parse(environment));
			}
			else
			{
				throw new ApplicationException("Could not create unresolved node");
			}

			return new LinkNode(left, right, tokens[OPERATOR].Position);
		}

		right = Singleton.Parse(environment, primary, token);

		// Try to build the right node as a virtual function or lambda call
		if (right.Is(NodeType.UNRESOLVED_FUNCTION))
		{
			var function = right.To<UnresolvedFunction>();
			var types = new List<Type?>();
			foreach (var argument in function) { types.Add(argument.TryGetType()); }

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