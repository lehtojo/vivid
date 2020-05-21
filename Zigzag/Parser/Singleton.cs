using System;
using System.Collections.Generic;

public class Singleton
{
	/// <summary>
	/// Tries to build identifier into a node
	/// </summary>
	/// <param name="context">Context to use for linking indentifier</param>
	/// <param name="id">Identifier to link</param>
	public static Node GetIdentifier(Context context, IdentifierToken id)
	{
		if (context.IsVariableDeclared(id.Value))
		{
			return new VariableNode(context.GetVariable(id.Value)!);
		}
		else if (context.IsTypeDeclared(id.Value))
		{
			return new TypeNode(context.GetType(id.Value)!);
		}
		else if (context.IsType)
		{
			var variable = new Variable(context, Types.UNKNOWN, VariableCategory.MEMBER, id.Value, AccessModifier.PUBLIC);
			return new VariableNode(variable);
		}
		else
		{
			return new UnresolvedIdentifier(id.Value);
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
			functions = context.GetType(name)!.GetConstructors();
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
		else
		{
			return new UnresolvedFunction(info.Name).SetParameters(parameters);
		}
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
	public static Node GetString(StringToken @string)
	{
		return new StringNode(@string.Text);
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
				return GetIdentifier(primary, (IdentifierToken)token);
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
				return ((DynamicToken)token).Node;
			}
		}

		throw new NotImplementedException("Couldn't parse the node");
	}

	public static Node GetUnresolved(Context environment, Token token)
	{
		switch (token.Type)
		{
			case TokenType.IDENTIFIER:
			IdentifierToken id = (IdentifierToken)token;
			return new UnresolvedIdentifier(id.Value);
			case TokenType.FUNCTION:
			FunctionToken function = (FunctionToken)token;
			return new UnresolvedFunction(function.Name)
							.SetParameters(function.GetParsedParameters(environment));
		}

		throw new Exception($"Couldn't create unresolved token ({token.Type})");
	}
}