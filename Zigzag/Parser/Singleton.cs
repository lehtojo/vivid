using System;
using System.Collections.Generic;
using System.Linq;

public static class Singleton
{
	/// <summary>
	/// Tries to build identifier into a node
	/// </summary>
	/// <param name="context">Context to use for linking indentifier</param>
	/// <param name="identifier">Identifier to link</param>
	public static Node GetIdentifier(Context context, IdentifierToken identifier, bool is_link = false)
	{
		if (context.IsVariableDeclared(identifier.Value))
		{
			var variable = context.GetVariable(identifier.Value)!;

			if (variable.IsMember && !is_link)
			{
				var self = context.GetSelfPointer() ?? throw new ApplicationException("Missing self pointer");
				
				return new LinkNode(
					new VariableNode(self), 
					new VariableNode(variable)
				);
			}

			return new VariableNode(variable);
		}
		else if (context.IsTypeDeclared(identifier.Value))
		{
			return new TypeNode(context.GetType(identifier.Value)!);
		}
		else
		{
			return new UnresolvedIdentifier(identifier.Value);
		}
	}

	/// <summary>
	/// Tries to find function or constructor by name 
	/// </summary>
	public static FunctionImplementation? GetFunctionByName(Context context, string name, List<Type> parameters)
	{
		FunctionList functions;

		if (context.IsTypeDeclared(name)) // Type constructors
		{
			var type = context.GetType(name)!;

			if (type.IsTemplateType)
			{
				var template_type = (TemplateType)type;
				functions = template_type[parameters.ToArray()].GetConstructors();

				parameters = parameters.Skip(template_type.TemplateArgumentNames.Count).ToList();
			}
			else
			{
				functions = context.GetType(name)!.GetConstructors();
			}
		}
		else if (context.IsFunctionDeclared(name)) // Functions
		{
			functions = context.GetFunction(name)!;
		}
		else
		{
			return null;
		}

		return functions[parameters];
	}

	/// <summary>
	/// Tries to build function into a node
	/// </summary>
	public static Node GetFunction(Context environment, Context primary, FunctionToken info)
	{
		var parameters = info.GetParsedParameters(environment);

		var types = Resolver.GetTypes(parameters);

		if (types == null)
		{
			return new UnresolvedFunction(info.Name).SetParameters(parameters);
		}

		var function = GetFunctionByName(primary, info.Name, types);

		if (function != null)
		{
			var node = new FunctionNode(function).SetParameters(parameters);

			if (function.Metadata is Constructor)
			{
				return new ConstructionNode(node);
			}

			return node;
		}
		else if (primary.IsVariableDeclared(info.Name))
		{
			var variable = primary.GetVariable(info.Name)!;

			if (variable.Type is LambdaType)
			{
				var call = new LambdaCallNode(parameters);

				return environment == primary
					? (Node)new LinkNode(new VariableNode(variable), call)
					: (Node)call;
			}
		}

		return new UnresolvedFunction(info.Name).SetParameters(parameters);
	}

	/// <summary>
	/// Tries to build number into a node
	/// </summary>
	public static Node GetNumber(NumberToken number)
	{
		return new NumberNode(number.NumberType, number.Value);
	}

	/// <summary>
	/// Tries to build content into a node
	/// </summary>
	public static Node GetContent(Context context, ContentToken content)
	{
		var node = new ContentNode();

		for (int i = 0; i < content.SectionCount; i++)
		{
			var tokens = content.GetTokens(i);
			Parser.Parse(node, context, tokens);
		}

		return node;
	}

	/// <summary>
	/// Tries to build string into a node
	/// </summary>
	public static Node GetString(StringToken string_token)
	{
		return new StringNode(string_token.Text);
	}

	public static Node Parse(Context context, Token token)
	{
		return Singleton.Parse(context, context, token);
	}

	public static Node Parse(Context environment, Context primary, Token token)
	{
		switch (token.Type)
		{
			case TokenType.IDENTIFIER:
			{
				return GetIdentifier(primary, (IdentifierToken)token, environment != primary);
			}

			case TokenType.FUNCTION:
			{
				return GetFunction(environment, primary, (FunctionToken)token);
			}

			case TokenType.NUMBER:
			{
				return GetNumber((NumberToken)token);
			}

			case TokenType.CONTENT:
			{
				return GetContent(primary, (ContentToken)token);
			}

			case TokenType.STRING:
			{
				return GetString((StringToken)token);
			}

			case TokenType.DYNAMIC:
			{
				return token.To<DynamicToken>().Node;
			}
		}

		throw new NotImplementedException("Couldn't parse the node");
	}

	public static Node GetUnresolved(Context environment, Token token)
	{
      return token.Type switch
      {
         TokenType.IDENTIFIER => new UnresolvedIdentifier(token.To<IdentifierToken>().Value),

         TokenType.FUNCTION => new UnresolvedFunction(token.To<FunctionToken>().Name)
				.SetParameters(token.To<FunctionToken>().GetParsedParameters(environment)),

         _ => throw new Exception($"Couldn't create unresolved token ({token.Type})"),
      };
   }
}