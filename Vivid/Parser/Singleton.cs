using System;
using System.Collections.Generic;
using System.Linq;

public static class Singleton
{
	/// <summary>
	/// Tries to build identifier into a node
	/// </summary>
	public static Node GetIdentifier(Context context, IdentifierToken identifier, bool linked = false)
	{
		if (context.IsVariableDeclared(identifier.Value, linked))
		{
			var variable = context.GetVariable(identifier.Value)!;

			if (variable.IsMember && !linked)
			{
				var self = Common.GetSelfPointer(context, identifier.Position);

				return new LinkNode(
					self,
					new VariableNode(variable, identifier.Position),
					identifier.Position
				);
			}

			return new VariableNode(variable, identifier.Position);
		}
		
		if (context.IsPropertyDeclared(identifier.Value, linked))
		{
			var implementation = context.GetProperty(identifier.Value)!.Get(Array.Empty<Type>())!;

			if (implementation.IsMember && !implementation.IsStatic && !linked)
			{
				var self = Common.GetSelfPointer(context, identifier.Position);

				return new LinkNode(
					self,
					new FunctionNode(implementation, identifier.Position),
					identifier.Position
				);
			}

			return new FunctionNode(implementation, identifier.Position);
		}
		
		if (context.IsTypeDeclared(identifier.Value, linked))
		{
			return new TypeNode(context.GetType(identifier.Value)!, identifier.Position);
		}
		
		return new UnresolvedIdentifier(identifier.Value, identifier.Position);
	}

	/// <summary>
	/// Tries to find function or constructor by name with the specified parameter types
	/// </summary>
	public static FunctionImplementation? GetFunctionByName(Context context, string name, List<Type> parameters, bool linked)
	{
		return GetFunctionByName(context, name, parameters, Array.Empty<Type>(), linked);
	}

	/// <summary>
	/// Tries to find function or constructor by name with the specified template parameter types and parameter types.
	/// </summary>
	public static FunctionImplementation? GetFunctionByName(Context context, string name, List<Type> parameters, Type[] template_arguments, bool linked)
	{
		FunctionList functions;

		if (context.IsTypeDeclared(name, linked))
		{
			// NOTE: There can not be template constructors (only template types)
			var type = context.GetType(name)!;

			if (template_arguments.Any())
			{
				// If there are template arguments and if any of the template parameters is unresolved, then this function should fail
				if (template_arguments.Any(i => i.IsUnresolved))
				{
					return null;
				}

				if (type is TemplateType template_type)
				{
					// Since the function name refers to a type, the constructors of the type should be explored next
					functions = template_type.GetVariant(template_arguments).Constructors;
				}
				else
				{
					return null;
				}
			}
			else
			{
				functions = context.GetType(name)!.Constructors;
			}
		}
		else if (context.IsFunctionDeclared(name, linked))
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
	public static Node GetFunction(Context environment, Context primary, FunctionToken token, bool linked)
	{
		var descriptor = (FunctionToken)token.Clone();
		var arguments = descriptor.GetParsedParameters(environment);

		var types = Resolver.GetTypes(arguments);

		if (types == null)
		{
			return new UnresolvedFunction(descriptor.Name, descriptor.Position).SetArguments(arguments);
		}

		if (!linked)
		{
			// Try to form a lambda function call
			var result = Common.TryGetLambdaCall(environment, descriptor);

			if (result != null)
			{
				result.Position = descriptor.Position;
				return result;
			}
		}

		var function = GetFunctionByName(primary, descriptor.Name, types, linked);

		if (function != null)
		{
			var node = new FunctionNode(function, descriptor.Position).SetArguments(arguments);

			if (function.IsConstructor)
			{
				var type = function.FindTypeParent() ?? throw new ApplicationException("Missing constructor parent type");

				// Consider the following situations:
				// Namespace.Type() <- Construction
				// Namespace.Type<large>() <- Construction
				// Namespace.Type.init() <- Direct call
				// Namespace.Type<large>.init() <- Direct call
				// Therefore, construction is only needed when the function name matches the name of the constructed type
				return type.Identifier != descriptor.Name ? node : (Node)new ConstructionNode(node, node.Position);
			}

			if (function.IsMember && !function.IsStatic && !linked)
			{
				var self = Common.GetSelfPointer(environment, descriptor.Position);

				return new LinkNode(self, node, descriptor.Position);
			}

			return node;
		}

		// Lastly, try to form a virtual function call if this function call is not linked
		if (!linked)
		{
			// Try to form a virtual function call
			var result = Common.TryGetVirtualFunctionCall(environment, descriptor);

			if (result != null)
			{
				result.Position = descriptor.Position;
				return result;
			}
		}

		return new UnresolvedFunction(descriptor.Name, descriptor.Position).SetArguments(arguments);
	}

	/// <summary>
	/// Tries to build function into a node
	/// </summary>
	public static Node GetFunction(Context environment, Context primary, FunctionToken descriptor, Type[] template_arguments, bool linked)
	{
		var parameters = descriptor.GetParsedParameters(environment);
		var types = Resolver.GetTypes(parameters);

		if (types == null || template_arguments.Any(i => i.IsUnresolved))
		{
			return new UnresolvedFunction(descriptor.Name, template_arguments, descriptor.Position).SetArguments(parameters);
		}

		var function = GetFunctionByName(primary, descriptor.Name, types, template_arguments, linked);

		if (function != null)
		{
			var node = new FunctionNode(function, descriptor.Position).SetArguments(parameters);

			if (function.IsConstructor)
			{
				var type = function.FindTypeParent() ?? throw new ApplicationException("Missing constructor parent type");

				// Consider the following situations:
				// Namespace.Type() <- Construction
				// Namespace.Type<large>() <- Construction
				// Namespace.Type.init() <- Direct call
				// Namespace.Type<large>.init() <- Direct call
				// Therefore, construction is only needed when the function name matches the name of the constructed type
				return type.Identifier != descriptor.Name ? node : (Node)new ConstructionNode(node, node.Position);
			}

			if (function.IsMember && !function.IsStatic && !linked)
			{
				var self = Common.GetSelfPointer(environment, descriptor.Position);

				return new LinkNode(self, node, descriptor.Position);
			}

			return node;
		}

		// NOTE: Template lambdas are not supported
		return new UnresolvedFunction(descriptor.Name, template_arguments, descriptor.Position).SetArguments(parameters);
	}

	/// <summary>
	/// Builds the specified number into a node
	/// </summary>
	public static Node GetNumber(NumberToken number)
	{
		return new NumberNode(number.NumberType, number.Value, number.Position);
	}

	/// <summary>
	/// Builds the specified content into a node
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
	/// Builds the specified string into a node
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

		throw new ApplicationException(Errors.Format(token.Position, "Could not understand the token"));
	}

	public static Node GetUnresolved(Context environment, Token token)
	{
		return token.Type switch
		{
			TokenType.IDENTIFIER => new UnresolvedIdentifier(token.To<IdentifierToken>().Value, token.To<IdentifierToken>().Position),

			TokenType.FUNCTION => new UnresolvedFunction(token.To<FunctionToken>().Name, token.To<FunctionToken>().Position).SetArguments(token.To<FunctionToken>().GetParsedParameters(environment)),

			_ => throw new Exception($"Could not create unresolved token ({token.Type})"),
		};
	}
}