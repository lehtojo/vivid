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
	public static Node GetIdentifier(Context context, IdentifierToken identifier, bool linked = false)
	{
		if (context.IsVariableDeclared(identifier.Value))
		{
			var variable = context.GetVariable(identifier.Value)!;

			if (variable.IsMember && !linked)
			{
				var self = context.GetSelfPointer() ?? throw new ApplicationException("Missing self pointer");

				return new LinkNode(
					new VariableNode(self, identifier.Position),
					new VariableNode(variable, identifier.Position),
					identifier.Position
				);
			}

			return new VariableNode(variable, identifier.Position);
		}
		else if (context.IsTypeDeclared(identifier.Value))
		{
			return new TypeNode(context.GetType(identifier.Value)!, identifier.Position);
		}
		else
		{
			return new UnresolvedIdentifier(identifier.Value, identifier.Position);
		}
	}

	/// <summary>
	/// Tries to find function or constructor by name with the specified parameter types
	/// </summary>
	public static FunctionImplementation? GetFunctionByName(Context context, string name, List<Type> parameters)
	{
		return GetFunctionByName(context, name, parameters, Array.Empty<Type>());
	}

	/// <summary>
	/// Tries to find function or constructor by name with the specified template parameter types and parameter types
	/// </summary>
	public static FunctionImplementation? GetFunctionByName(Context context, string name, List<Type> parameters, Type[] template_arguments)
	{
		FunctionList functions;

		if (context.IsTypeDeclared(name))
		{
			// NOTE: There can not be template constructors (only template types)
			var type = context.GetType(name)!;

			if (template_arguments.Any())
			{
				// If there are template parameters and the if any of the template parameters is unresolved, then this function should fail
				if (template_arguments.Any(i => i.IsUnresolved))
				{
					return null;
				}

				if (type is TemplateType template_type)
				{
					// Since the function name refers to a type, the constructors of the type should be explored next
					functions = template_type.GetVariant(template_arguments).GetConstructors();
				}
				else
				{
					return null;
				}
			}
			else
			{
				functions = context.GetType(name)!.GetConstructors();
			}
		}
		else if (context.IsFunctionDeclared(name))
		{
			functions = context.GetFunction(name)!;

			// If there are template parameters, then the function should be retrieved based on them
			if (template_arguments.Any())
			{
				return functions.GetImplementation(parameters, template_arguments);
			}
		}
		else
		{
			return null;
		}

		return functions.GetImplementation(parameters);
	}

	/// <summary>
	/// Tries to build function into a node
	/// </summary>
	public static Node GetFunction(Context environment, Context primary, FunctionToken token, bool linked = false)
	{
		var descriptor = (FunctionToken)token.Clone();
		var parameters = descriptor.GetParsedParameters(environment);

		var types = Resolver.GetTypes(parameters);

		if (types == null)
		{
			return new UnresolvedFunction(descriptor.Name, descriptor.Position).SetParameters(parameters);
		}

		var function = GetFunctionByName(primary, descriptor.Name, types);

		if (function != null)
		{
			var node = new FunctionNode(function, descriptor.Position).SetParameters(parameters);

			if (function.IsConstructor)
			{
				return new ConstructionNode(node, node.Position);
			}

			if (function.IsMember && !linked)
			{
				var self = environment.GetSelfPointer() ?? throw new ApplicationException("Missing self pointer");

				return new LinkNode(new VariableNode(self, descriptor.Position), node, descriptor.Position);
			}

			return node;
		}

		if (!linked)
		{
			// Try to form a virtual function call
			var result = Common.TryGetVirtualFunctionCall(environment, descriptor);

			if (result != null)
			{
				result.Position = descriptor.Position;
				return result;
			}

			// Try to form a lambda function call
			result = Common.TryGetLambdaCall(environment, descriptor);

			if (result != null)
			{
				result.Position = descriptor.Position;
				return result;
			}
		}

		return new UnresolvedFunction(descriptor.Name, descriptor.Position).SetParameters(parameters);
	}

	/// <summary>
	/// Tries to build function into a node
	/// </summary>
	public static Node GetFunction(Context environment, Context primary, FunctionToken descriptor, Type[] template_arguments, bool linked = false)
	{
		var parameters = descriptor.GetParsedParameters(environment);
		var types = Resolver.GetTypes(parameters);

		if (types == null || template_arguments.Any(i => i.IsUnresolved))
		{
			return new UnresolvedFunction(descriptor.Name, template_arguments, descriptor.Position).SetParameters(parameters);
		}

		var function = GetFunctionByName(primary, descriptor.Name, types, template_arguments);

		if (function != null)
		{
			var node = new FunctionNode(function, descriptor.Position).SetParameters(parameters);

			if (function.IsConstructor)
			{
				return new ConstructionNode(node, node.Position);
			}

			if (function.IsMember && !linked)
			{
				var self = environment.GetSelfPointer() ?? throw new ApplicationException("Missing self pointer");

				return new LinkNode(new VariableNode(self, descriptor.Position), node, descriptor.Position);
			}

			return node;
		}

		// NOTE: Template lambdas are not supported
		return new UnresolvedFunction(descriptor.Name, template_arguments, descriptor.Position).SetParameters(parameters);
	}

	/// <summary>
	/// Tries to build number into a node
	/// </summary>
	public static Node GetNumber(NumberToken number)
	{
		return new NumberNode(number.NumberType, number.Value, number.Position);
	}

	/// <summary>
	/// Tries to build content into a node
	/// </summary>
	public static Node GetContent(Context context, ContentToken content)
	{
		var node = new ContentNode(content.Position);

		foreach (var section in content.GetSections())
		{
			Parser.Parse(node, context, section);
		}

		return node;
	}

	/// <summary>
	/// Tries to build string into a node
	/// </summary>
	public static Node GetString(StringToken token)
	{
		return new StringNode(token.Text, token.Position);
	}

	public static Node Parse(Context context, Token token)
	{
		return Parse(context, context, token);
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
				return GetFunction(environment, primary, (FunctionToken)token, environment != primary);
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

		throw new NotImplementedException("Could not parse the node");
	}

	public static Node GetUnresolved(Context environment, Token token)
	{
		return token.Type switch
		{
			TokenType.IDENTIFIER => new UnresolvedIdentifier(token.To<IdentifierToken>().Value, token.To<IdentifierToken>().Position),

			TokenType.FUNCTION => new UnresolvedFunction(token.To<FunctionToken>().Name, token.To<FunctionToken>().Position).SetParameters(token.To<FunctionToken>().GetParsedParameters(environment)),

			_ => throw new Exception($"Could not create unresolved token ({token.Type})"),
		};
	}
}