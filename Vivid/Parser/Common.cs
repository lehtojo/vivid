using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

public static class Common
{
	/// <summary>
	/// Consumes template parameters
	/// Pattern: <$1, $2, ... $n>
	/// </summary>
	public static bool ConsumeTemplateParameters(PatternState state)
	{
		// Next there must be the opening of the template parameters
		if (!Pattern.Consume(state, out Token? opening, TokenType.OPERATOR) || opening!.To<OperatorToken>().Operator != Operators.LESS_THAN)
		{
			return false;
		}

		while (true)
		{
			Pattern.Try(ConsumeType, state);

			if (!Pattern.Consume(state, out Token? consumed, TokenType.OPERATOR))
			{
				return false;
			}

			if (consumed!.To<OperatorToken>().Operator == Operators.GREATER_THAN)
			{
				return true;
			}

			if (consumed!.To<OperatorToken>().Operator == Operators.COMMA)
			{
				continue;
			}

			return false;
		}
	}

	/// <summary>
	/// Consumes a (template) type
	/// Pattern: $name [<$1, $2, ... $n>]
	/// </summary>
	public static bool ConsumeType(PatternState state)
	{
		if (!Pattern.Consume(state, out Token? _, TokenType.IDENTIFIER))
		{
			return false;
		}

		Pattern.Try(ConsumeTemplateParameters, state);

		return true;
	}

	/// <summary>
	/// Consumes a template function call except the name in the begining
	/// Pattern: <$1, $2, ... $n> (...)
	/// </summary>
	public static bool ConsumeTemplateFunctionCall(PatternState state)
	{
		// Consume pattern: <$1, $2, ... $n>
		if (!ConsumeTemplateParameters(state))
		{
			return false;
		}

		// Now there must be function parameters next
		return Pattern.Consume(state, out Token? parameters, TokenType.CONTENT) && parameters!.To<ContentToken>().Type == ParenthesisType.PARENTHESIS;
	}

	/// <summary>
	/// Reads a type from the next tokens inside the specified queue
	/// Pattern: $name [<$1, $2, ... $n>]
	/// </summary>
	public static Type? ReadTypeArgument(Context context, Queue<Token> tokens)
	{
		if (!tokens.Peek().Is(TokenType.IDENTIFIER))
		{
			return null;
		}

		var name = tokens.Dequeue().To<IdentifierToken>().Value;

		if (tokens.Peek().Is(TokenType.OPERATOR) && tokens.Peek().To<OperatorToken>().Operator == Operators.LESS_THAN)
		{
			var parameters = ReadTemplateArguments(context, tokens);

			if (parameters.All(i => !i.IsUnresolved) && context.IsTypeDeclared(name))
			{
				var type = context.GetType(name)!;

				if (type is TemplateType template)
				{
					return template.GetVariant(parameters);
				}
			}

			return new UnresolvedType(context, name, parameters);
		}

		return context.IsTypeDeclared(name) ? context.GetType(name) : new UnresolvedType(context, name);
	}

	/// <summary>
	/// Reads template parameters from the next tokens inside the specified queue
	/// Pattern: <$1, $2, ... $n>
	/// </summary>
	public static Type[] ReadTemplateArguments(Context context, Queue<Token> tokens)
	{
		var opening = tokens.Dequeue().To<OperatorToken>();

		if (opening.Operator != Operators.LESS_THAN)
		{
			throw new InvalidOperationException("Tried to read template parameters but its syntax was invalid");
		}

		var parameters = new List<Type>();

		Type? parameter;

		while ((parameter = ReadTypeArgument(context, tokens)) != null)
		{
			parameters.Add(parameter);

			if (tokens.Peek().Is(TokenType.OPERATOR) && tokens.Peek().To<OperatorToken>().Operator == Operators.COMMA)
			{
				tokens.Dequeue();
			}
		}

		var closing = tokens.Dequeue().To<OperatorToken>();

		if (closing.Operator != Operators.GREATER_THAN)
		{
			throw new InvalidOperationException("Tried to read template parameters but its syntax was invalid");
		}

		return parameters.ToArray();
	}

	public static List<string> GetTemplateArgumentNames(List<Token> template_argument_tokens, Position template_arguments_start)
	{
		var template_argument_names = new List<string>();

		for (var i = 0; i < template_argument_tokens.Count; i++)
		{
			if (i % 2 != 0)
			{
				continue;
			}

			if (!template_argument_tokens[i].Is(TokenType.IDENTIFIER))
			{
				throw Errors.Get(template_arguments_start, "Template type's argument list is invalid");
			}

			template_argument_names.Add(template_argument_tokens[i].To<IdentifierToken>().Value);
		}

		if (template_argument_names.Count == 0)
		{
			throw Errors.Get(template_arguments_start, "Template type's argument list cannot be empty");
		}

		return template_argument_names;
	}
}
