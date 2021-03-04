using System;
using System.Collections.Generic;
using System.Linq;

public static class Common
{
	/// <summary>
	/// Returns whether the specified actual types are compatible with the specified expected types, that is whether the actual types can be casted to match the expected types. This function also requires that the actual parameters are all resolved, otherwise this function returns false.
	/// </summary>
	public static bool Compatible(IEnumerable<Type?> expected, IEnumerable<Type?> actual)
	{
		if (expected.Count() != actual.Count())
		{
			return false;
		}

		return expected.Zip(actual).All(i => i.Second != null && (i.First == null || i.First.Is(i.Second)));
	}

	/// <summary>
	/// Tries to build a virtual function call which has a specified owner
	/// </summary>
	public static CallNode? TryGetVirtualFunctionCall(Node self, Type self_type, string name, Node parameters, List<Type?> parameter_types)
	{
		if (!self_type.IsVirtualFunctionDeclared(name))
		{
			return null;
		}

		// Ensure all the parameter types are resolved
		if (parameter_types.Any(i => i == Types.UNKNOWN || i.IsUnresolved))
		{
			return null;
		}

		// Try to find a virtual function with the parameter types
		var overload = (VirtualFunction?)self_type.GetVirtualFunction(name)!.GetOverload(parameter_types!);

		if (overload == null || self_type.Configuration == null)
		{
			return null;
		}

		var function_pointer = OffsetNode.CreateConstantOffset(new LinkNode(self.Clone(), new VariableNode(self_type.Configuration.Variable)), overload.Alignment, 1, Parser.Format);
		var required_self_type = overload.GetTypeParent() ?? throw new ApplicationException("Could not retrieve virtual function parent type");

		if (self_type != required_self_type)
		{
			self = new CastNode(self, new TypeNode(required_self_type), self.Position);
		}

		return new CallNode(self, function_pointer, parameters, new CallDescriptorType(required_self_type, overload.Parameters.Select(i => i.Type!).ToList()!, overload.ReturnType));
	}

	/// <summary>
	/// Tries to build a virtual function call which has a specified owner
	/// </summary>
	public static CallNode? TryGetVirtualFunctionCall(Context environment, Node self, Type self_type, FunctionToken descriptor)
	{
		var parameters = descriptor.GetParsedParameters(environment);
		var parameter_types = parameters.Select(i => i.TryGetType()).ToList();

		return TryGetVirtualFunctionCall(self, self_type, descriptor.Name, parameters, parameter_types!);
	}

	/// <summary>
	/// Tries to build a virtual function call which uses the current self pointer
	/// </summary>
	public static CallNode? TryGetVirtualFunctionCall(Context environment, string name, Node parameters, List<Type?> parameter_types)
	{
		if (!environment.IsInsideFunction)
		{
			return null;
		}

		var type = environment.GetTypeParent();

		if (type == null)
		{
			return null;
		}

		var self = Common.GetSelfPointer(environment, null);

		return TryGetVirtualFunctionCall(self, type, name, parameters, parameter_types);
	}

	/// <summary>
	/// Tries to build a virtual function call which uses the current self pointer
	/// </summary>
	public static CallNode? TryGetVirtualFunctionCall(Context environment, FunctionToken descriptor)
	{
		if (!environment.IsInsideFunction)
		{
			return null;
		}

		var type = environment.GetTypeParent();

		if (type == null)
		{
			return null;
		}

		var self = Common.GetSelfPointer(environment, null);

		return TryGetVirtualFunctionCall(environment, self, type, descriptor);
	}

	/// <summary>
	/// Tries to build a lambda call which is stored inside a specified owner
	/// </summary>
	public static CallNode? TryGetLambdaCall(Context primary, Node left, string name, Node parameters, List<Type?> parameter_types)
	{
		if (!primary.IsVariableDeclared(name))
		{
			return null;
		}

		var variable = primary.GetVariable(name)!;

		if (!(variable.Type is CallDescriptorType properties && Compatible(properties.Parameters, parameter_types)))
		{
			return null;
		}

		var self = new LinkNode(left, new VariableNode(variable));
		var function_pointer = OffsetNode.CreateConstantOffset(self.Clone(), 0, Parser.Size.Bytes, Parser.Format);

		return new CallNode(self, function_pointer, parameters, properties);
	}

	/// <summary>
	/// Tries to build a lambda call which is stored inside a specified owner
	/// </summary>
	public static CallNode? TryGetLambdaCall(Context environment, Context primary, Node left, FunctionToken descriptor)
	{
		var parameters = descriptor.GetParsedParameters(environment);
		var parameter_types = parameters.Select(i => i.TryGetType()).ToList();

		return TryGetLambdaCall(primary, left, descriptor.Name, parameters, parameter_types);
	}

	/// <summary>
	/// Tries to build a lambda call which is stored inside the current scope or in the self pointer
	/// </summary>
	public static CallNode? TryGetLambdaCall(Context environment, string name, Node parameters, List<Type?> parameter_types)
	{
		if (!environment.IsVariableDeclared(name))
		{
			return null;
		}

		var variable = environment.GetVariable(name)!;

		if (!(variable.Type is CallDescriptorType properties && Compatible(properties.Parameters, parameter_types)))
		{
			return null;
		}

		Node? self;

		if (variable.IsMember)
		{
			var self_pointer = Common.GetSelfPointer(environment, null);

			self = new LinkNode(self_pointer, new VariableNode(variable));
		}
		else
		{
			self = new VariableNode(variable);
		}

		var function_pointer = OffsetNode.CreateConstantOffset(self.Clone(), 0, Parser.Size.Bytes, Parser.Format);

		return new CallNode(self, function_pointer, parameters, properties);
	}

	/// <summary>
	/// Tries to build a lambda call which is stored inside the current scope or in the self pointer
	/// </summary>
	public static CallNode? TryGetLambdaCall(Context environment, FunctionToken descriptor)
	{
		var parameters = descriptor.GetParsedParameters(environment);
		var parameter_types = parameters.Select(i => i.TryGetType()).ToList();

		return TryGetLambdaCall(environment, descriptor.Name, parameters, parameter_types);
	}

	/// <summary>
	/// Consumes template parameters
	/// Pattern: <$1, $2, ... $n>
	/// </summary>
	public static bool ConsumeTemplateArguments(PatternState state)
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

		Pattern.Try(ConsumeTemplateArguments, state);

		return true;
	}

	/// <summary>
	/// Consumes a template function call except the name in the begining
	/// Pattern: <$1, $2, ... $n> (...)
	/// </summary>
	public static bool ConsumeTemplateFunctionCall(PatternState state)
	{
		// Consume pattern: <$1, $2, ... $n>
		if (!ConsumeTemplateArguments(state))
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

		if (tokens.Any() && tokens.Peek().Is(Operators.LESS_THAN))
		{
			var template_arguments = ReadTemplateArguments(context, tokens);

			if (template_arguments.All(i => !i.IsUnresolved) && context.IsTypeDeclared(name))
			{
				var type = context.GetType(name)!;

				if (type is TemplateType template_type)
				{
					return template_type.GetVariant(template_arguments);
				}

				// Some base types are "manual template types" such as link meaning they can still receive template arguments even though they are not instances of a template type class
				if (type.IsTemplateType)
				{
					// Clone the type since it is shared and add the template types
					type = type.Clone();
					type.TemplateArguments = template_arguments;
					return type;
				}
			}

			return new UnresolvedType(context, name, template_arguments);
		}

		return context.IsTypeDeclared(name) ? context.GetType(name) : new UnresolvedType(context, name);
	}

	/// <summary>
	/// Reads a type from the next tokens inside the specified queue
	/// Pattern: $name [<$1, $2, ... $n>]
	/// </summary>
	public static List<Token>? ReadTypeArgumentTokens(Queue<Token> tokens)
	{
		if (!tokens.Any() || !tokens.Peek().Is(TokenType.IDENTIFIER))
		{
			return null;
		}

		var name = tokens.Dequeue().To<IdentifierToken>();
		var result = new List<Token> { name };

		if (tokens.Any() && tokens.Peek().Is(Operators.LESS_THAN))
		{
			result.AddRange(ReadTemplateArgumentTokens(tokens));
		}

		return result;
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
			throw Errors.Get(opening.Position, "Could not understand the template arguments");
		}

		var parameters = new List<Type>();

		Type? parameter;

		while ((parameter = ReadTypeArgument(context, tokens)) != null)
		{
			parameters.Add(parameter);

			if (tokens.Peek().Is(Operators.COMMA))
			{
				tokens.Dequeue();
			}
		}

		if (!tokens.TryDequeue(out Token? closing) || !closing.Is(Operators.GREATER_THAN))
		{
			throw Errors.Get(opening.Position, "Could not understand the template arguments");
		}

		return parameters.ToArray();
	}

	/// <summary>
	/// Reads template parameters from the next tokens inside the specified queue
	/// Pattern: <$1, $2, ... $n>
	/// </summary>
	public static List<Token> ReadTemplateArgumentTokens(Queue<Token> tokens)
	{
		var opening = tokens.Dequeue().To<OperatorToken>();
		var result = new List<Token> { opening };

		if (opening.Operator != Operators.LESS_THAN)
		{
			throw Errors.Get(opening.Position, "Could not understand the template arguments");
		}

		while (true)
		{
			var type_argument_tokens = ReadTypeArgumentTokens(tokens);

			if (type_argument_tokens == null)
			{
				break;
			}

			result.AddRange(type_argument_tokens);

			if (tokens.Any() && tokens.Peek().Is(Operators.COMMA))
			{
				result.Add(tokens.Dequeue());
			}
		}

		if (!tokens.TryDequeue(out Token? closing) || !closing.Is(Operators.GREATER_THAN))
		{
			throw Errors.Get(opening.Position, "Could not understand the template arguments");
		}

		result.Add(closing);

		return result;
	}

	/// <summary>
	/// Reads template parameter names
	/// Pattern: <A, B, C, ...>
	/// Returns: { A, B, C, ... }
	/// </summary>
	public static List<string> GetTemplateParameterNames(List<Token> template_argument_tokens, Position template_arguments_start)
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
			throw Errors.Get(template_arguments_start, "Template type's argument list can not be empty");
		}

		return template_argument_names;
	}

	public static bool ConsumeBlock(Context context, PatternState state, List<Token> destination)
	{
		var consumed = Parser.Consume(
			context,
			state,
			new List<System.Type>()
			{
				typeof(CastPattern),
				typeof(CommandPattern),
				typeof(HasPattern),
				typeof(IsPattern),
				typeof(LinkPattern),
				typeof(NotPattern),
				typeof(OffsetPattern),
				typeof(OperatorPattern),
				typeof(PreIncrementAndDecrementPattern),
				typeof(PostIncrementAndDecrementPattern),
				typeof(ReturnPattern),
				typeof(UnarySignPattern)
			}
		);

		destination.AddRange(consumed);
		return consumed.Any();
	}

	public static bool ConsumeBody(PatternState state)
	{
		return Pattern.Try(state, () => Pattern.Consume(state, out Token? body, TokenType.CONTENT) && body!.To<ContentToken>().Type == ParenthesisType.CURLY_BRACKETS);
	}

	public static KeyValuePair<Type, DataPointer>[] CopyTypeDescriptors(Type type, List<Type> supertypes)
	{
		if (type.Configuration == null)
		{
			return Array.Empty<KeyValuePair<Type, DataPointer>>(); ;
		}

		var configuration = type.Configuration;
		var descriptor_count = type.Supertypes.Any() ? supertypes.Count : supertypes.Count + 1;
		var descriptors = new KeyValuePair<Type, DataPointer>[descriptor_count];

		if (!configuration.IsCompleted)
		{
			// Complete the descriptor of the type
			configuration.Descriptor.Add(type.ContentSize);
			configuration.Descriptor.Add(type.Supertypes.Count);

			type.Supertypes.ForEach(i => configuration.Descriptor.Add(i.Configuration?.Descriptor ?? throw new ApplicationException("Missing runtime configuration from a supertype")));
		}

		if (!type.Supertypes.Any())
		{
			// Even though there are no supertypes inherited, an instance of this type can be created and casted to a link.
			// It should be possible to check whether the link represents this type or another
			descriptors[^1] = new KeyValuePair<Type, DataPointer>(type, new DataPointer(configuration.Entry, 0));
		}

		var offset = Parser.Bytes;

		for (var i = 0; i < supertypes.Count; i++)
		{
			var supertype = supertypes[i];

			// Append configuration information only if it is not generated
			if (!configuration.IsCompleted)
			{
				// Begin a new section inside the configuration table
				configuration.Entry.Add(configuration.Descriptor);
			}

			// Types should not inherited types which do not have runtime configurations such as standard integers
			if (supertype.Configuration == null)
			{
				throw new ApplicationException("Type inherited a type which did not have runtime configuration");
			}

			descriptors[i] = new KeyValuePair<Type, DataPointer>(supertype, new DataPointer(configuration.Entry, offset));
			offset += Parser.Bytes;

			// Interate all virtual functions of this supertype and connect their implementations
			foreach (var virtual_function in supertype.GetAllVirtualFunctions())
			{
				// Find all possible implementations of the virtual function inside the specified type
				var overloads = type.GetFunction(virtual_function.Name)?.Overloads;

				if (overloads == null)
				{
					continue;
				}

				// Retrieve all parameter types of the virtual function declaration
				var expected = virtual_function.Parameters.Select(i => i.Type).ToList();

				// Try to find a suitable implementation for the virtual function from the specified type
				FunctionImplementation? implementation = null;

				foreach (var overload in overloads)
				{
					var actual = overload.Parameters.Select(i => i.Type).ToList();

					if (actual.Count != expected.Count || !actual.SequenceEqual(expected))
					{
						continue;
					}

					implementation = overload.Get(expected!);
					break;
				}

				if (implementation == null)
				{
					// It seems there is no implementation for this virtual function
					continue;
				}

				// Append configuration information only if it is not generated
				if (!configuration.IsCompleted)
				{
					configuration.Entry.Add(new Label(implementation.GetFullname() + "_v"));
				}

				offset += Parser.Bytes;
			}
		}

		configuration.IsCompleted = true;
		return descriptors;
	}

	public static Node CreateHeapConstruction(FunctionNode constructor)
	{
		var type = constructor.GetType();

		var environment = constructor.GetParentContext();
		var inline = new ContextInlineNode(new Context(environment), constructor.Position);

		var allocation_size = Math.Max(1L, type.ContentSize);
		var instance = inline.Context.DeclareHidden(type);

		var allocation_parameters = new Node { new NumberNode(Assembler.Format, allocation_size) };

		inline.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
			new VariableNode(instance),
			new CastNode(
				new FunctionNode(Parser.AllocationFunction!, constructor.Position).SetParameters(allocation_parameters),
				new TypeNode(type)
			)
		));

		var supertypes = type.GetAllSupertypes();
		var descriptors = CopyTypeDescriptors(type, supertypes);

		foreach (var iterator in descriptors)
		{
			inline.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
				new LinkNode(new VariableNode(instance), new VariableNode(iterator.Key.Configuration!.Variable)),
				iterator.Value
			));
		}

		inline.Add(new LinkNode(new VariableNode(instance), constructor));
		return inline;
	}

	public static Node CreateStackConstruction(FunctionNode constructor)
	{
		var type = constructor.GetType();

		var environment = constructor.GetParentContext();
		var inline = new ContextInlineNode(new Context(environment), constructor.Position);

		var allocation_size = Math.Max(1, type.ContentSize);
		var instance = inline.Context.DeclareHidden(type);

		inline.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
			new VariableNode(instance),
			new CastNode(new StackAddressNode(allocation_size), new TypeNode(type))
		));

		var supertypes = type.GetAllSupertypes();
		var descriptors = CopyTypeDescriptors(type, supertypes);

		foreach (var iterator in descriptors)
		{
			inline.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
				new LinkNode(new VariableNode(instance), new VariableNode(iterator.Key.Configuration!.Variable)),
				iterator.Value
			));
		}

		inline.Add(new LinkNode(new VariableNode(instance), constructor, constructor.Position));
		inline.Add(new VariableNode(instance));

		return inline;
	}

	/// <summary>
	/// Returns whether the specified node is part of a function call argument
	/// </summary>
	public static bool IsCallArgument(Node node)
	{
		var result = node.FindParent(i => !i.Is(NodeType.CONTENT, NodeType.CAST));

		return result != null && result.Is(NodeType.CALL, NodeType.FUNCTION, NodeType.UNRESOLVED_FUNCTION);
	}

	/// <summary>
	/// Tries to return the type of the parameter that the specified node influences
	/// </summary>
	public static Type? TryGetParameterType(Node argument)
	{
		var iterator = argument;

		while (iterator != null)
		{
			if (iterator.Parent == null)
			{
				return null;
			}

			if (iterator.Parent.Is(NodeType.FUNCTION))
			{
				// The specified node is part of standard function call
				var function = iterator.Parent.To<FunctionNode>().Function;
				var index = iterator.Parent.ToList().IndexOf(iterator);

				// Ensure there are enough function parameters
				if (function.ParameterTypes.Count <= index)
				{
					throw Errors.Get(iterator.Parent.Position, "Found a function call which takes too many arguments");
				}

				return function.ParameterTypes[index];
			}
			else if (iterator.Parent.Is(NodeType.CALL))
			{
				// The specified node is part of a manual function call
				var descriptor = iterator.Parent.To<CallNode>().Descriptor;
				var index = iterator.Parent.ToList().IndexOf(iterator);

				// Ensure there are enough function parameters
				if (descriptor.Parameters.Count <= index)
				{
					throw Errors.Get(iterator.Parent.Position, "Found a function call which takes too many arguments");
				}

				return descriptor.Parameters[index];
			}
			else if (iterator is not IContext)
			{
				// The parent node represents a typed object so it can be part of a function call
				/// NOTE: Loops and such are context objects, so the condition should work
				iterator = iterator.Parent;
			}
			else
			{
				return null;
			}
		}

		return null;
	}

	public static Node GetSelfPointer(Context context, Position? position)
	{
		var self = context.GetSelfPointer();

		if (self != null)
		{
			return new VariableNode(self, position);
		}

		return new UnresolvedIdentifier(context.IsInsideLambda ? Lambda.SELF_POINTER_IDENTIFIER : Function.SELF_POINTER_IDENTIFIER, position);
	}

	public static OperatorNode GetCondition(Context environment, Token token)
	{
		var node = Singleton.Parse(environment, token);

		if (node.Is(NodeType.OPERATOR))
		{
			return node.To<OperatorNode>();
		}

		return new OperatorNode(Operators.NOT_EQUALS, node.Position).SetOperands(node, new NumberNode(Parser.Format, 0L, node.Position));
	}

	public static OperatorNode GetCondition(Context environment, List<Token> tokens)
	{
		var result = Parser.Parse(environment, tokens, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY);

		if (result.Count() == 1)
		{
			var node = result.First();

			if (node.Is(NodeType.OPERATOR))
			{
				return result.First().To<OperatorNode>();
			}

			return new OperatorNode(Operators.NOT_EQUALS, node.Position).SetOperands(node, new NumberNode(Parser.Format, 0L, node.Position));
		}

		return new OperatorNode(Operators.NOT_EQUALS, result.Position).SetOperands(result, new NumberNode(Parser.Format, 0L, result.Position));
	}

	public static Node FindCondition(Node start)
	{
		return start.GetRightWhile(i => i.Is(NodeType.CONTEXT, NodeType.INLINE, NodeType.NORMAL, NodeType.CONTENT)) ?? throw new ApplicationException("Conditional statement did not have a condition");
	}
}
