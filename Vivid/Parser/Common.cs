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
		if (parameter_types.Any(i => i == null || i.IsUnresolved))
		{
			return null;
		}

		// Try to find a virtual function with the parameter types
		var overload = (VirtualFunction?)self_type.GetVirtualFunction(name)!.GetOverload(parameter_types!);

		if (overload == null || self_type.Configuration == null)
		{
			return null;
		}

		var function_pointer = new OffsetNode(new LinkNode(self.Clone(), new VariableNode(self_type.Configuration.Variable)), new NumberNode(Parser.Format, overload.Alignment));
		var required_self_type = overload.GetTypeParent() ?? throw new ApplicationException("Could not retrieve virtual function parent type");

		if (self_type != required_self_type)
		{
			self = new CastNode(self, new TypeNode(required_self_type), self.Position);
		}

		return new CallNode(self, function_pointer, parameters, new FunctionType(required_self_type, overload.Parameters.Select(i => i.Type!).ToList()!, overload.ReturnType));
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

		var self = GetSelfPointer(environment, null);

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

		var self = GetSelfPointer(environment, null);

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

		if (!(variable.Type is FunctionType properties && Compatible(properties.Parameters, parameter_types)))
		{
			return null;
		}

		var self = new LinkNode(left, new VariableNode(variable));
		var offset = Analysis.IsGarbageCollectorEnabled ? 2L : 1L;
		var function_pointer = new OffsetNode(self.Clone(), new NumberNode(Parser.Format, offset));

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

		if (!(variable.Type is FunctionType properties && Compatible(properties.Parameters, parameter_types)))
		{
			return null;
		}

		Node? self;

		if (variable.IsMember)
		{
			var self_pointer = GetSelfPointer(environment, null);

			self = new LinkNode(self_pointer, new VariableNode(variable));
		}
		else
		{
			self = new VariableNode(variable);
		}

		var offset = Analysis.IsGarbageCollectorEnabled ? 2L : 1L;
		var function_pointer = new OffsetNode(self.Clone(), new NumberNode(Parser.Format, offset));

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
	/// Consumes a lambda type
	/// </summary>
	public static bool ConsumeFunctionType(PatternState state)
	{
		// Consume a normal parenthesis token
		if (!Pattern.Consume(state, out Token? parameters, TokenType.CONTENT) || !parameters!.Is(ParenthesisType.PARENTHESIS))
		{
			return false;
		}

		// Consume an arrow operator
		if (!Pattern.Consume(state, out Token? arrow, TokenType.OPERATOR) || !arrow!.Is(Operators.ARROW))
		{
			return false;
		}

		// Consume the return type
		return ConsumeType(state);
	}

	/// <summary>
	/// Consumes a type
	/// </summary>
	public static bool ConsumeType(PatternState state)
	{
		if (!Pattern.Consume(state, TokenType.IDENTIFIER))
		{
			var next = Pattern.Peek(state);

			if (next == null || !next.Is(ParenthesisType.PARENTHESIS))
			{
				return false;
			}
		}

		while (true)
		{
			var next = Pattern.Peek(state);

			if (next == null) return true;

			if (next.Is(Operators.DOT))
			{
				Pattern.Consume(state);

				if (!Pattern.Consume(state, TokenType.IDENTIFIER)) return false;

				Pattern.Try(ConsumeTemplateArguments, state);
			}
			else if (next.Is(Operators.LESS_THAN))
			{
				if (!ConsumeTemplateArguments(state)) return false;
			}
			else if (next.Is(ParenthesisType.PARENTHESIS))
			{
				if (!ConsumeFunctionType(state)) return false;
			}
			else
			{
				return true;
			}
		}
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
	/// Reads a type which represents a function from the specified tokens
	/// </summary>
	private static FunctionType? ReadFunctionType(Context context, Queue<Token> tokens)
	{
		// Dequeue the parameter types
		var parameters = tokens.Dequeue().To<ContentToken>();

		// Dequeue the arrow operator
		tokens.Dequeue();

		// Dequeues the return type
		var return_type = ReadType(context, tokens);

		// The return type must exist
		if (return_type == null)
		{
			return null;
		}

		// Read all the parameter types
		var parameter_types = parameters.GetSections().Select(i => ReadType(context, new Queue<Token>(i))).ToList();

		// If any of the parameter types is null, it means there is a syntax error
		if (parameter_types.Any(i => i == null))
		{
			return null;
		}

		return new FunctionType(parameter_types, return_type);
	}
	
	/// <summary>
	/// Reads a type component from the tokens and returns it
	/// </summary>
	public static UnresolvedTypeComponent ReadTypeComponent(Context context, Queue<Token> tokens)
	{
		var name = tokens.Dequeue().To<IdentifierToken>().Value;

		if (tokens.Any() && tokens.Peek().Is(Operators.LESS_THAN))
		{
			var template_arguments = ReadTemplateArguments(context, tokens);

			return new UnresolvedTypeComponent(name, template_arguments);
		}

		return new UnresolvedTypeComponent(name);
	}

	/// <summary>
	/// Reads a type from the next tokens inside the specified queue
	/// Pattern: $name [<$1, $2, ... $n>]
	/// </summary>
	public static Type? ReadType(Context context, Queue<Token> tokens)
	{
		var next = tokens.Peek();

		if (next.Is(TokenType.CONTENT))
		{
			return ReadFunctionType(context, tokens);
		}

		if (!next.Is(TokenType.IDENTIFIER))
		{
			return null;
		}

		var components = new List<UnresolvedTypeComponent>();

		while (true)
		{
			components.Add(ReadTypeComponent(context, tokens));

			// Stop collecting type components if there are no tokens left or if the next token is not a dot operator
			if (!tokens.Any() || !tokens.Peek().Is(Operators.DOT))
			{
				break;
			}

			tokens.Dequeue();
		}

		var type = new UnresolvedType(components.ToArray());

		return type.TryResolveType(context) ?? type;
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
			throw Errors.Get(opening.Position, "Can not understand the template arguments");
		}

		var parameters = new List<Type>();

		Type? parameter;

		while ((parameter = ReadType(context, tokens)) != null)
		{
			parameters.Add(parameter);

			if (tokens.Peek().Is(Operators.COMMA))
			{
				tokens.Dequeue();
			}
		}

		if (!tokens.TryDequeue(out Token? closing) || !closing.Is(Operators.GREATER_THAN))
		{
			throw Errors.Get(opening.Position, "Can not understand the template arguments");
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
			throw Errors.Get(opening.Position, "Can not understand the template arguments");
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
			throw Errors.Get(opening.Position, "Can not understand the template arguments");
		}

		result.Add(closing);

		return result;
	}

	/// <summary>
	/// Reads template parameter names
	/// Pattern: <A, B, C, ...>
	/// Returns: { A, B, C, ... }
	/// </summary>
	public static List<string> GetTemplateParameters(List<Token> template_argument_tokens, Position template_arguments_start)
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
				throw Errors.Get(template_arguments_start, "Template type argument list is invalid");
			}

			template_argument_names.Add(template_argument_tokens[i].To<IdentifierToken>().Value);
		}

		if (template_argument_names.Count == 0)
		{
			throw Errors.Get(template_arguments_start, "Template type argument list can not be empty");
		}

		return template_argument_names;
	}

	/// <summary>
	/// Consumes a block of code which is considered to be a part of code which collapses into one dynamic token
	/// </summary>
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

	/// <summary>
	/// Consumes curly brackets
	/// </summary>
	public static bool ConsumeBody(PatternState state)
	{
		return Pattern.Try(state, () => Pattern.Consume(state, out Token? body, TokenType.CONTENT) && body!.To<ContentToken>().Type == ParenthesisType.CURLY_BRACKETS);
	}

	public static KeyValuePair<Type, DataPointer>[] CopyTypeDescriptors(Type type, List<Type> supertypes)
	{
		if (type.Configuration == null)
		{
			return Array.Empty<KeyValuePair<Type, DataPointer>>();
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
			descriptors[^1] = new KeyValuePair<Type, DataPointer>(type, new DataPointer(configuration.Entry));
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

	/// <summary>
	/// Constructs an object using heap memory
	/// </summary>
	public static Node CreateHeapConstruction(Type type, FunctionNode constructor)
	{
		var environment = constructor.GetParentContext();
		var inline = new ContextInlineNode(new Context(environment), constructor.Position);

		var size = Math.Max(1L, type.ContentSize);
		var instance = inline.Context.DeclareHidden(type);

		var arguments = new Node { new NumberNode(Assembler.Format, size) };
		
		if (Analysis.IsGarbageCollectorEnabled)
		{
			var linker = Parser.LinkFunction!.Get(type) ?? throw new ApplicationException("Missing link function overload");

			// The following example creates an instance of a type called Object
			// Example: instance = link(allocate(sizeof(Object))) as Object
			inline.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(instance),
				new CastNode(
					new FunctionNode(linker, constructor.Position).SetParameters(new Node {
						new FunctionNode(Parser.AllocationFunction!, constructor.Position).SetParameters(arguments)
					}),
					new TypeNode(type)
				)
			));
		}
		else
		{
			// The following example creates an instance of a type called Object
			// Example: instance = allocate(sizeof(Object)) as Object
			inline.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(instance),
				new CastNode(
					new FunctionNode(Parser.AllocationFunction!, constructor.Position).SetParameters(arguments),
					new TypeNode(type)
				)
			));
		}

		var supertypes = type.GetAllSupertypes();
		var descriptors = CopyTypeDescriptors(type, supertypes);

		// Register the runtime configurations
		foreach (var iterator in descriptors)
		{
			inline.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
				new LinkNode(new VariableNode(instance), new VariableNode(iterator.Key.Configuration!.Variable)),
				iterator.Value
			));
		}

		// Do not call the initializer function if it is empty
		if (!constructor.Function.IsEmpty)
		{
			inline.Add(new LinkNode(new VariableNode(instance), constructor, constructor.Position));
		}
		
		// The inline node must return the value of the constructed object
		inline.Add(new VariableNode(instance));

		return inline;
	}

	/// <summary>
	/// Constructs an object using stack memory
	/// </summary>
	public static Node CreateStackConstruction(Type type, FunctionNode constructor)
	{
		var environment = constructor.GetParentContext();
		var inline = new ContextInlineNode(new Context(environment), constructor.Position);

		var instance = inline.Context.DeclareHidden(type);

		inline.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
			new VariableNode(instance),
			new CastNode(new StackAddressNode(inline.Context, type), new TypeNode(type))
		));

		var supertypes = type.GetAllSupertypes();
		var descriptors = CopyTypeDescriptors(type, supertypes);

		// Register the runtime configurations
		foreach (var iterator in descriptors)
		{
			inline.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
				new LinkNode(new VariableNode(instance), new VariableNode(iterator.Key.Configuration!.Variable)),
				iterator.Value
			));
		}

		// Do not call the initializer function if it is empty
		if (!constructor.Function.IsEmpty)
		{
			inline.Add(new LinkNode(new VariableNode(instance), constructor, constructor.Position));
		}

		// The inline node must return the value of the constructed object
		inline.Add(new VariableNode(instance));

		return inline;
	}

	/// <summary>
	/// Returns the root of the expression which contains the specified node
	/// </summary>
	public static Node GetExpressionRoot(Node node)
	{
		var iterator = node;

		while (iterator.Parent != null && !ReconstructionAnalysis.IsStatement(iterator.Parent))
		{
			iterator = iterator.Parent;
		}

		return iterator;
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
	/// Returns the self pointer of the specified context
	/// </summary>
	public static Node GetSelfPointer(Context context, Position? position)
	{
		var self = context.GetSelfPointer();

		if (self != null)
		{
			return new VariableNode(self, position);
		}

		return new UnresolvedIdentifier(context.IsInsideLambda ? Lambda.SELF_POINTER_IDENTIFIER : Function.SELF_POINTER_IDENTIFIER, position);
	}

	/// <summary>
	/// Finds the condition node from the specified node
	/// </summary>
	public static Node FindCondition(Node start)
	{
		return start.GetRightWhile(i => i.Is(NodeType.SCOPE, NodeType.INLINE, NodeType.NORMAL, NodeType.CONTENT)) ?? throw new ApplicationException("Conditional statement did not have a condition");
	}

	/// <summary>
	/// Collects all types and subtypes from the specified context
	/// </summary>
	public static List<Type> GetAllTypes(Context context)
	{
		var result = context.Types.Values.ToList();
		result.AddRange(result.SelectMany(i => GetAllTypes(i)).ToList());

		return result.Where(i => !string.IsNullOrEmpty(i.Name)).ToList();
	}

	/// <summary>
	/// Collects all function implementations from the specified context
	/// </summary>
	public static FunctionImplementation[] GetAllFunctionImplementations(Context context)
	{
		var types = Common.GetAllTypes(context);

		// Collect all functions, constructors, destructors and virtual functions
		var type_functions = types.SelectMany(i => i.Functions.Values.SelectMany(j => j.Overloads));
		var type_constructors = types.SelectMany(i => i.Constructors.Overloads);
		var type_destructors = types.SelectMany(i => i.Destructors.Overloads);
		var type_virtual_functions = types.SelectMany(i => i.Virtuals.Values.SelectMany(j => j.Overloads));
		var context_functions = context.Functions.Values.SelectMany(i => i.Overloads);

		var implementations = type_functions.Concat(type_constructors).Concat(type_destructors).Concat(type_virtual_functions).Concat(context_functions).SelectMany(i => i.Implementations).ToArray();

		// Concat all functions with lambdas, which can be found inside the collected functions
		return implementations.Concat(implementations.SelectMany(i => GetAllFunctionImplementations(i)))
			.Distinct(new HashlessReferenceEqualityComparer<FunctionImplementation>()).ToArray();
	}

	/// <summary>
	/// Collects all functions from the specified context and its subcontexts.
	/// NOTE: This function does not return lambda functions.
	/// </summary>
	public static Function[] GetAllVisibleFunctions(Context context)
	{
		var types = GetAllTypes(context);

		// Collect all functions, constructors, destructors and virtual functions
		var type_functions = types.SelectMany(i => i.Functions.Values.SelectMany(j => j.Overloads));
		var type_constructors = types.SelectMany(i => i.Constructors.Overloads);
		var type_destructors = types.SelectMany(i => i.Destructors.Overloads);
		var type_virtual_functions = types.SelectMany(i => i.Virtuals.Values.SelectMany(j => j.Overloads));
		var context_functions = context.Functions.Values.SelectMany(i => i.Overloads);

		return type_functions.Concat(type_constructors).Concat(type_destructors).Concat(type_virtual_functions).Concat(context_functions).Distinct().ToArray();
	}

	/// <summary>
	/// Collects all functions which have implementations from the specified context and its subcontexts
	/// </summary>
	public static Function[] GetAllImplementedFunctions(Context context)
	{
		return GetAllFunctionImplementations(context).Select(i => i.Metadata!).Distinct().ToArray();
	}

	/// <summary>
	/// Returns whether the specified integer fullfills the following equation:
	/// x = 2^y where y is an integer constant
	/// </summary>
	public static bool IsPowerOfTwo(long x)
	{
		return (x & (x - 1)) == 0;
	}

	/// <summary>
	/// Joins the specified token lists with the specified separator
	/// </summary>
	public static Token[] Join(Token separator, Token[][] elements)
	{
		var result = new List<Token>();

		foreach (var element in elements)
		{
			result.AddRange(element);
			result.Add(separator);
		}

		return result.GetRange(0, result.Count - 1).ToArray();
	}

	/// <summary>
	/// Returns tokens which represent the specified type
	/// </summary>
	public static Token[] GetTokens(Type type, Position position)
	{
		var result = new List<Token>();

		if (type is FunctionType function)
		{
			var parameters = function.Parameters.Select(i => GetTokens(i!, position)).ToArray();
			var separator = new OperatorToken(Operators.COMMA) { Position = position };

			result.Add(new ContentToken(ParenthesisType.PARENTHESIS, Join(separator, parameters)) { Position = position });
			result.Add(new OperatorToken(Operators.ARROW) { Position = position });
			result.AddRange(GetTokens(function.ReturnType!, position));

			return result.ToArray();
		}

		if (type.Parent != null && type.Parent.IsType)
		{
			result.AddRange(GetTokens(type.Parent.To<Type>(), position));
			result.Add(new OperatorToken(Operators.DOT) { Position = position });
		}

		result.Add(new IdentifierToken(type.IsUserDefined ? type.Identifier : type.Name, position));

		if (type.TemplateArguments.Any())
		{
			result.Add(new OperatorToken(Operators.LESS_THAN) { Position = position });

			var arguments = type.TemplateArguments.Select(i => GetTokens(i, position)).ToArray();
			var separator = new OperatorToken(Operators.COMMA) { Position = position };

			result.AddRange(Join(separator, arguments));

			result.Add(new OperatorToken(Operators.GREATER_THAN) { Position = position });
		}

		return result.ToArray();
	}

	/// <summary>
	/// Returns whether the cast is safe
	/// </summary>
	public static bool IsCastSafe(Type from, Type to)
	{
		return from.Equals(to) || (from is Number && to is Number && (from is Link == to is Link)) || from.IsTypeInherited(to) || to.IsTypeInherited(from);
	}

	/// <summary>
	/// Returns how many bits the specified number requires
	/// </summary>
	public static int GetBits(object value)
	{
		if (value is double) return Parser.Size.Bits;

		var x = (long)value;

		if (x < 0)
		{
			if (x < int.MinValue) return 64;
			else if (x < short.MinValue) return 32;
			else if (x < byte.MinValue) return 16;
		}
		else
		{
			if (x > int.MaxValue) return 64;
			else if (x > short.MaxValue) return 32;
			else if (x > byte.MaxValue) return 16;
		}

		return 8;
	}
}
