using System;
using System.Collections.Generic;
using System.Linq;

public struct InlineContainer
{
	public Node Destination { get; private set; }
	public Node Node { get; private set; }
	public Variable Result { get; private set; }

	public InlineContainer(Node destination, Node node, Variable result)
	{
		Destination = destination;
		Node = node;
		Result = result;
	}
}

public class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
{
	public bool Equals(T? x, T? y)
	{
		return ReferenceEquals(x, y);
	}

	public int GetHashCode(T x)
	{
		return HashCode.Combine(x);
	}
}

public static class Common
{
	/// <summary>
	/// Returns whether the specified actual types are compatible with the specified expected types, that is whether the actual types can be casted to match the expected types. This function also requires that the actual parameters are all resolved, otherwise this function returns false.
	/// </summary>
	public static bool Compatible(List<Type?> expected_types, List<Type?> actual_types)
	{
		if (expected_types.Count != actual_types.Count) return false;

		for (var i = 0; i < expected_types.Count; i++)
		{
			var expected = expected_types[i];
			if (expected == null) continue;

			var actual = actual_types[i];
			if (actual == null) return false;

			if (Equals(expected, actual)) continue;

			if (!expected.IsPrimitive || !actual.IsPrimitive)
			{
				if (!expected.IsTypeInherited(actual) && !actual.IsTypeInherited(expected)) return false;
			}
			else if (Resolver.GetSharedType(expected, actual) == null) return false;
		}

		return true;
	}

	/// <summary>
	/// Returns whether the specified actual types are compatible with the specified expected types, that is whether the actual types can be casted to match the expected types. This function also requires that the actual parameters are all resolved, otherwise this function returns false.
	/// </summary>
	public static bool Compatible(Type? expected, Type? actual)
	{
		if (expected == null || actual == null || expected.IsUnresolved || actual.IsUnresolved) return false;

		if (Equals(expected, actual)) return true;

		if (!expected.IsPrimitive || !actual.IsPrimitive)
		{
			if (!expected.IsTypeInherited(actual) && !actual.IsTypeInherited(expected)) return false;
		}
		else if (Resolver.GetSharedType(expected, actual) == null) return false;

		return true;
	}

	/// <summary>
	/// Tries to build a virtual function call which has a specified owner
	/// </summary>
	public static CallNode? TryGetVirtualFunctionCall(Node self, Type self_type, string name, Node parameters, List<Type?> parameter_types, Position? position)
	{
		if (!self_type.IsVirtualFunctionDeclared(name)) return null;

		// Ensure all the parameter types are resolved
		if (parameter_types.Any(i => i == null || i.IsUnresolved)) return null;

		// Try to find a virtual function with the parameter types
		var overload = (VirtualFunction?)self_type.GetVirtualFunction(name)!.GetOverload(parameter_types!);
		if (overload == null || overload.ReturnType == null) return null;

		var required_self_type = overload.FindTypeParent() ?? throw new ApplicationException("Could not retrieve virtual function parent type");
		if (required_self_type.Configuration == null) return null;

		var configuration = required_self_type.GetConfigurationVariable();
		var alignment = (long)required_self_type.GetAllVirtualFunctions().IndexOf(overload);
		if (alignment == -1) throw new ApplicationException("Could not compute virtual function alignment");

		var function_pointer = new AccessorNode(new LinkNode(self.Clone(), new VariableNode(configuration)), new NumberNode(Parser.Format, alignment + 1));
		
		if (self_type != required_self_type)
		{
			self = new CastNode(self, new TypeNode(required_self_type), self.Position);
		}

		var descriptor = new FunctionType(required_self_type, overload.Parameters.Select(i => i.Type!).ToList()!, overload.ReturnType, position);

		return new CallNode(self, function_pointer, parameters, descriptor, position);
	}

	/// <summary>
	/// Tries to build a virtual function call which has a specified owner
	/// </summary>
	public static CallNode? TryGetVirtualFunctionCall(Context environment, Node self, Type self_type, FunctionToken descriptor)
	{
		var parameters = descriptor.Parse(environment);
		var parameter_types = parameters.Select(i => i.TryGetType()).ToList();

		return TryGetVirtualFunctionCall(self, self_type, descriptor.Name, parameters, parameter_types!, descriptor.Position);
	}

	/// <summary>
	/// Tries to build a virtual function call which uses the current self pointer
	/// </summary>
	public static CallNode? TryGetVirtualFunctionCall(Context environment, string name, Node parameters, List<Type?> parameter_types, Position? position)
	{
		if (!environment.IsInsideFunction) return null;

		var type = environment.FindTypeParent();
		if (type == null) return null;

		var self = GetSelfPointer(environment, null);

		return TryGetVirtualFunctionCall(self, type, name, parameters, parameter_types, position);
	}

	/// <summary>
	/// Tries to build a virtual function call which uses the current self pointer
	/// </summary>
	public static CallNode? TryGetVirtualFunctionCall(Context environment, FunctionToken descriptor)
	{
		if (!environment.IsInsideFunction) return null;

		var type = environment.FindTypeParent();
		if (type == null) return null;

		var self = GetSelfPointer(environment, null);

		return TryGetVirtualFunctionCall(environment, self, type, descriptor);
	}

	/// <summary>
	/// Tries to build a lambda call which is stored inside a specified owner
	/// </summary>
	public static CallNode? TryGetLambdaCall(Context primary, Node left, string name, Node arguments, List<Type?> parameter_types)
	{
		if (!primary.IsVariableDeclared(name)) return null;

		var variable = primary.GetVariable(name)!;

		// Require the variable to represent a function
		if (!(variable.Type is FunctionType properties && Compatible(properties.Parameters, parameter_types))) return null;

		var position = left.Position;
		var self = new LinkNode(left, new VariableNode(variable));

		// If system mode is enabled, lambdas are just function pointers and capturing variables is not allowed
		if (Settings.IsSystemModeEnabled) return new CallNode(new Node(), self, arguments, properties, position);

		// Determine where the function pointer is located
		var offset = Analysis.IsGarbageCollectorEnabled ? 2L : 1L;

		// Load the function pointer using the offset
		var function_pointer = new AccessorNode(self.Clone(), new NumberNode(Parser.Format, offset));

		return new CallNode(self, function_pointer, arguments, properties);
	}

	/// <summary>
	/// Tries to build a lambda call which is stored inside a specified owner
	/// </summary>
	public static CallNode? TryGetLambdaCall(Context environment, Context primary, Node left, FunctionToken descriptor)
	{
		var parameters = descriptor.Parse(environment);
		var parameter_types = parameters.Select(i => i.TryGetType()).ToList();

		return TryGetLambdaCall(primary, left, descriptor.Name, parameters, parameter_types);
	}

	/// <summary>
	/// Tries to build a lambda call which is stored inside the current scope or in the self pointer
	/// </summary>
	public static CallNode? TryGetLambdaCall(Context environment, string name, Node arguments, List<Type?> argument_types)
	{
		if (!environment.IsVariableDeclared(name)) return null;

		var variable = environment.GetVariable(name)!;

		// Require that the specified argument types pass the required parameter types
		if (!(variable.Type is FunctionType properties && Compatible(properties.Parameters, argument_types))) return null;

		var self = (Node?)null;
		var position = arguments.Position;

		if (variable.IsMember)
		{
			var self_pointer = GetSelfPointer(environment, null);

			self = new LinkNode(self_pointer, new VariableNode(variable));
		}
		else
		{
			self = new VariableNode(variable);
		}
		
		// If system mode is enabled, lambdas are just function pointers and capturing variables is not allowed
		if (Settings.IsSystemModeEnabled) return new CallNode(new Node(), self, arguments, properties, position);

		// Determine where the function pointer is located
		var offset = Analysis.IsGarbageCollectorEnabled ? 2L : 1L;

		// Load the function pointer using the offset
		var function_pointer = new AccessorNode(self.Clone(), new NumberNode(Parser.Format, offset));

		return new CallNode(self, function_pointer, arguments, properties);
	}

	/// <summary>
	/// Tries to build a lambda call which is stored inside the current scope or in the self pointer
	/// </summary>
	public static CallNode? TryGetLambdaCall(Context environment, FunctionToken descriptor)
	{
		var parameters = descriptor.Parse(environment);
		var parameter_types = parameters.Select(i => i.TryGetType()).ToList();

		return TryGetLambdaCall(environment, descriptor.Name, parameters, parameter_types);
	}

	/// <summary>
	/// Attempts to return the context of the specified node. If there is no context, none is returned.
	/// </summary>
	public static Context? GetContext(Node node)
	{
		// If the node is a special context node (global scope syntax for example), return its context
		if (node.Instance == NodeType.CONTEXT) return node.To<ContextNode>().Context;

		// If the node has a type, return the type as a context
		return node.TryGetType();
	}

	/// <summary>
	/// Pattern: <$1, $2, ... $n>
	/// </summary>
	public static bool ConsumeTemplateArguments(ParserState state)
	{
		// Next there must be the opening of the template parameters
		var next = state.Peek();
		if (next == null || !next.Is(Operators.LESS_THAN)) return false;
		state.Consume();

		// Keep track whether at least one argument has been consumed
		var is_argument_consumed = false;

		while (true)
		{
			next = state.Peek();
			if (next == null) return false;

			// If the consumed operator is a greater than operator, it means the template arguments have ended
			if (next.Is(Operators.GREATER_THAN))
			{
				state.Consume();
				return is_argument_consumed;
			}

			// If the operator is a comma, it means the template arguments have not ended
			if (next.Is(Operators.COMMA))
			{
				state.Consume();
				continue;
			}

			if (ConsumeType(state))
			{
				is_argument_consumed = true;
				continue;
			}

			// The template arguments must be invalid
			return false;
		}
	}

	/// <summary>
	/// Pattern: <T1, T2, ..., Tn>
	/// </summary>
	public static bool ConsumeTemplateParameters(ParserState state)
	{
		// Next there must be the opening of the template parameters
		if (!state.ConsumeOperator(Operators.LESS_THAN)) return false;

		// Keep track whether at least one parameter has been consumed
		var is_parameter_consumed = false;

		while (true)
		{
			// If the next token is a greater than operator, it means the template parameters have ended
			if (state.ConsumeOperator(Operators.GREATER_THAN)) return is_parameter_consumed;

			// If the next token is a comma, it means the template parameters have not ended
			if (state.ConsumeOperator(Operators.COMMA)) continue;

			// Now we expect a template parameter name
			if (state.Consume(TokenType.IDENTIFIER))
			{
				is_parameter_consumed = true;
				continue;
			}

			// The template parameters must be invalid
			return false;
		}
	}

	/// <summary>
	/// Consumes a function type
	/// Pattern: (...) -> $type
	/// </summary>
	public static bool ConsumeFunctionType(ParserState state)
	{
		// Consume a normal parenthesis token
		if (!state.Consume(out Token? parameters, TokenType.PARENTHESIS) || !parameters!.Is(ParenthesisType.PARENTHESIS)) return false;

		// Consume an arrow operator
		if (!state.Consume(out Token? arrow, TokenType.OPERATOR) || !arrow!.Is(Operators.ARROW)) return false;

		// Consume the return type
		return ConsumeType(state);
	}

	/// <summary>
	/// Consumes a pack type.
	/// Pattern: { $member-1: $type, $member-2: $type, ... }
	/// </summary>
	public static bool ConsumePackType(ParserState state)
	{
		// Consume curly brackets
		if (!state.Consume(out Token? brackets, TokenType.PARENTHESIS) || !brackets!.Is(ParenthesisType.CURLY_BRACKETS)) return false;

		// Verify the curly brackets contain pack members using sections
		// Pattern: { $member-1: $type, $member-2: $type, ... }
		var sections = brackets!.To<ParenthesisToken>().GetSections();
		if (sections.Count == 0) return false;

		foreach (var section in sections)
		{
			if (section.Count < 3) return false;

			// Verify the first token is a member name
			if (!section[0].Is(TokenType.IDENTIFIER)) return false;

			// Verify the second token is a colon
			if (!section[1].Is(Operators.COLON)) return false;
		}

		return true;
	}

	public static void ConsumeTypeEnd(ParserState state)
	{
		var next = (Token?)null;

		// Consume pointers
		while (true)
		{
			next = state.Peek();
			if (next == null) return;

			if (!next.Is(Operators.MULTIPLY)) break;
			state.Consume();
		}

		// Do not allow creating nested arrays
		if (next.Is(ParenthesisType.BRACKETS))
		{
			state.Consume();
		}
	}

	public static bool ConsumeType(ParserState state)
	{
		if (!state.Consume(TokenType.IDENTIFIER))
		{
			var next = state.Peek();
			if (next == null) return false;

			if (next.Is(ParenthesisType.CURLY_BRACKETS))
			{
				if (!ConsumePackType(state)) return false;
			}
			else if (next.Is(ParenthesisType.PARENTHESIS))
			{
				if (!ConsumeFunctionType(state)) return false;
			}
			else
			{
				return false;
			}

			return true;
		}

		while (true)
		{
			var next = state.Peek();
			if (next == null) return true;

			if (next.Is(Operators.DOT))
			{
				state.Consume();
				if (!state.Consume(TokenType.IDENTIFIER)) return false;
			}
			else if (next.Is(Operators.LESS_THAN))
			{
				if (!ConsumeTemplateArguments(state)) return false;
			}
			else
			{
				break;
			}
		}

		ConsumeTypeEnd(state);
		return true;
	}

	/// <summary>
	/// Consumes a template function call except the name in the beginning
	/// Pattern: <$1, $2, ... $n> (...)
	/// </summary>
	public static bool ConsumeTemplateFunctionCall(ParserState state)
	{
		// Consume pattern: <$1, $2, ... $n>
		if (!ConsumeTemplateArguments(state))
		{
			return false;
		}

		// Now there must be function parameters next
		return state.Consume(out Token? parameters, TokenType.PARENTHESIS) && parameters!.To<ParenthesisToken>().Opening == ParenthesisType.PARENTHESIS;
	}

	/// <summary>
	/// Reads a type which represents a function from the specified tokens.
	/// Pattern: ($type-1, $type-2, ... $type-n) -> $type
	/// </summary>
	private static FunctionType? ReadFunctionType(Context context, List<Token> tokens, Position? position)
	{
		// Pop the parameter types
		var parameters = tokens.Pop()!.To<ParenthesisToken>();

		// Pop the arrow operator
		if (tokens.Pop() == null) return null;

		// Pop the return type
		var return_type = ReadType(context, tokens);

		// The return type must exist
		if (return_type == null) return null;

		// Read all the parameter types
		var parameter_types = new List<Type>();
		tokens = new List<Token>(parameters.Tokens);

		while (tokens.Count > 0)
		{
			var parameter_type = Common.ReadType(context, tokens);
			if (parameter_type == null) throw Errors.Get(tokens.First().Position, "Could not understand the parameter type");
			parameter_types.Add(parameter_type);
			tokens.Pop();
		}

		return new FunctionType(parameter_types!, return_type, position);
	}

	/// <summary>
	/// Creates an unnamed pack type from the specified tokens.
	/// Pattern: { $member-1: $type-1, $member-2: $type-2, ... }
	/// </summary>
	private static Type? ReadPackType(Context context, List<Token> tokens, Position? position)
	{
		var pack = context.DeclareUnnamedPack(position);
		var sections = tokens.Pop()!.To<ParenthesisToken>().GetSections();

		// We are not going to feed the sections straight to the parser while using the pack type as context, because it would allow defining whole member functions
		foreach (var section in sections)
		{
			// Determine the member name and its type
			var member = section.First().To<IdentifierToken>().Value;

			var type = ReadType(context, new List<Token>(section.Skip(2)));
			if (type == null) return null;

			// Create the member using the determined properties
			pack.Declare(type, VariableCategory.MEMBER, member);
		}

		return pack;
	}
	
	/// <summary>
	/// Reads a type component from the tokens and returns it
	/// </summary>
	public static UnresolvedTypeComponent ReadTypeComponent(Context context, List<Token> tokens)
	{
		var name = tokens.Pop()!.To<IdentifierToken>().Value;

		if (tokens.Any() && tokens.First().Is(Operators.LESS_THAN))
		{
			var template_arguments = ReadTemplateArguments(context, tokens);

			return new UnresolvedTypeComponent(name, template_arguments);
		}

		return new UnresolvedTypeComponent(name);
	}

	/// <summary>
	/// Reads type components from the specified tokens
	/// </summary>
	public static List<UnresolvedTypeComponent> ReadTypeComponents(Context context, List<Token> tokens)
	{
		var components = new List<UnresolvedTypeComponent>();

		while (true)
		{
			components.Add(ReadTypeComponent(context, tokens));

			// Stop collecting type components if there are no tokens left or if the next token is not a dot operator
			if (!tokens.Any() || !tokens.First().Is(Operators.DOT)) break;

			tokens.Pop();
		}

		return components;
	}

	/// <summary>
	/// Reads a type from the next tokens inside the specified list
	/// Pattern: $name [<$1, $2, ... $n>]
	/// </summary>
	public static Type? ReadType(Context context, List<Token> tokens)
	{
		if (!tokens.Any()) return null;

		var position = tokens.First().Position;
		var next = tokens.First();

		if (next.Is(TokenType.PARENTHESIS))
		{
			if (next.Is(ParenthesisType.PARENTHESIS)) return ReadFunctionType(context, tokens, next.Position);
			if (next.Is(ParenthesisType.CURLY_BRACKETS)) return ReadPackType(context, tokens, next.Position);

			return null;
		}

		if (!next.Is(TokenType.IDENTIFIER)) return null;

		// Self return type:
		if (next.To<IdentifierToken>().Value == Function.SELF_POINTER_IDENTIFIER) return Primitives.SELF;

		var components = ReadTypeComponents(context, tokens);
		var type = new UnresolvedType(components.ToArray(), position);

		// If there are no more tokens, return the type
		if (!tokens.Any()) return type.ResolveOrThis(context);

		// Array types:
		next = tokens.First();

		if (next.Is(ParenthesisType.BRACKETS))
		{
			tokens.Pop();

			type.Size = next.To<ParenthesisToken>();
			return type.ResolveOrThis(context);
		}

		// Count the number of pointers
		while (true)
		{
			// Require at least one token
			if (!tokens.Any()) break;

			// Expect a multiplication token (pointer)
			if (!tokens.First().Is(Operators.MULTIPLY)) break;
			tokens.Pop();

			// Wrap the current type around a pointer
			type.Pointers++;
		}

		return type.ResolveOrThis(context);
	}

	/// <summary>
	/// Reads a type from the next tokens inside the specified list
	/// Pattern: $name [<$1, $2, ... $n>]
	/// </summary>
	public static Type? ReadType(Context context, List<Token> tokens, int start)
	{
		return ReadType(context, tokens.GetRange(start, tokens.Count - start));
	}

	/// <summary>
	/// Returns whether the specified node is a function call
	/// </summary>
	public static bool IsFunctionCall(Node node)
	{
		if (node.Instance == NodeType.LINK) { node = node.Right!; }
		return node.Is(NodeType.CALL, NodeType.FUNCTION);
	}

	/// <summary>
	/// Reads template parameters from the next tokens inside the specified list
	/// Pattern: <$1, $2, ... $n>
	/// </summary>
	public static Type[] ReadTemplateArguments(Context context, List<Token> tokens)
	{
		var opening = tokens.Pop()!.To<OperatorToken>();
		if (opening.Operator != Operators.LESS_THAN) throw Errors.Get(opening.Position, "Can not understand the template arguments");

		var parameters = new List<Type>();

		while (true)
		{
			var parameter = ReadType(context, tokens);
			if (parameter == null) break;

			parameters.Add(parameter);

			// Consume the next token, if it is a comma
			if (tokens.First().Is(Operators.COMMA)) tokens.Pop();
		}

		var next = tokens.Pop();
		if (!next!.Is(Operators.GREATER_THAN)) throw Errors.Get(opening.Position, "Can not understand the template arguments");

		return parameters.ToArray();
	}

	/// <summary>
	/// Reads template parameters from the next tokens inside the specified list
	/// Pattern: <$1, $2, ... $n>
	/// </summary>
	public static Type[] ReadTemplateArguments(Context context, List<Token> tokens, int start)
	{
		return ReadTemplateArguments(context, tokens.GetRange(start, tokens.Count - start));
	}

	/// <summary>
	/// Returns the template parameters from the specified tokens.
	/// </summary>
	public static List<string> GetTemplateParameters(List<Token> tokens, Position position)
	{
		var parameters = new List<string>();

		for (var i = 0; i < tokens.Count; i += 2)
		{
			if (!tokens[i].Is(TokenType.IDENTIFIER)) throw Errors.Get(position, "Template parameter tokens were invalid");

			parameters.Add(tokens[i].To<IdentifierToken>().Value);
		}

		return parameters;
	}

	public static void ConsumeBlock(ParserState from, List<Token> destination)
	{
		ConsumeBlock(from, destination, 0);
	}

	public static void ConsumeBlock(ParserState from, List<Token> destination, long disabled)
	{
		// Return an empty list, if there is nothing to be consumed
		if (from.End >= from.All.Count) return;

		// Clone the tokens from the specified state
		var tokens = from.All.GetRange(from.End, from.All.Count - from.End).Select(i => (Token)i.Clone()).ToList();

		var state = new ParserState();
		state.All = tokens;

		var consumptions = new List<Pair<DynamicToken, int>>();
		var context = new Context("0") { IsLambdaContainer = true };

		for (var priority = Parser.MAX_FUNCTION_BODY_PRIORITY; priority >= Parser.MIN_PRIORITY; priority--)
		{
			while (true)
			{
				if (!Parser.NextConsumable(context, tokens, priority, 0, state, disabled)) break;

				var node = state.Pattern!.Build(context, state, state.Tokens);

				var length = state.End - state.Start;
				var consumed = 0;

				while (length-- > 0)
				{
					var token = tokens[state.Start];
					var area = 1;

					if (token.Type == TokenType.DYNAMIC)
					{
						// Look for the consumption, which is related to the current dynamic token, and increment the consumed tokens by the number of tokens it once consumed
						foreach (var consumption in consumptions)
						{
							if (consumption.First != token) continue;
							area = consumption.Second;
							break;
						}
					}

					consumed += area;
					tokens.RemoveAt(state.Start);
				}

				if (node == null)
				{
					throw new ApplicationException("Block consumption does not accept patterns returning nothing");
				}

				var result = new DynamicToken(node);
				tokens.Insert(state.Start, result);
				consumptions.Add(new Pair<DynamicToken, int>(result, consumed));
			}
		}

		var next = tokens.First();

		if (next.Type == TokenType.DYNAMIC)
		{
			var consumed = 1;

			// Determine how many tokens the next dynamic token consumed
			foreach (var consumption in consumptions)
			{
				if (consumption.First != next) continue;
				consumed = consumption.Second;
				break;
			}

			// Read the consumed tokens from the source state
			var source = from.All;
			var end = from.End;

			for (var i = 0; i < consumed; i++)
			{
				destination.Add(source[end + i]);
			}

			from.End += consumed;
			return;
		}

		// Just consume the first token
		from.End++;
		destination.Add(next);
	}

	/// <summary>
	/// Tries to find the override for the specified virtual function and registers it to the specified runtime configuration.
	/// If no override can be found, address of zero is registered.
	/// This function returns the next offset after registering the override function.
	/// </summary>
	private static int TryRegisterVirtualFunctionImplementation(Type type, VirtualFunction virtual_function, RuntimeConfiguration configuration, int offset)
	{
		// If the configuration is already completed, no need to do anything
		if (configuration.IsCompleted) return offset + Parser.Bytes;

		// Find all possible implementations of the virtual function inside the specified type
		var overloads = type.GetOverride(virtual_function.Name)?.Overloads;

		if (overloads == null)
		{
			// It seems there is no implementation for this virtual function, register address of zero
			configuration.Entry.Add(0L);
			return offset + Parser.Bytes;
		}

		// Retrieve all parameter types of the virtual function declaration
		var expected = virtual_function.Parameters.Select(i => i.Type).ToList();

		// Try to find a suitable implementation for the virtual function from the specified type
		FunctionImplementation? implementation = null;

		foreach (var overload in overloads)
		{
			var actual = overload.Parameters.Select(i => i.Type).ToList();

			if (actual.Count != expected.Count || !actual.SequenceEqual(expected)) continue;

			implementation = overload.Get(expected!);
			break;
		}

		if (implementation == null)
		{
			// It seems there is no implementation for this virtual function, register address of zero
			configuration.Entry.Add(0L);
			return offset + Parser.Bytes;
		}

		configuration.Entry.Add(new Label(implementation.GetFullname() + "_v"));
		return offset + Parser.Bytes;
	}

	public static KeyValuePair<Type, DataPointerNode>[] CopyTypeDescriptors(Type type, List<Type> supertypes)
	{
		if (type.Configuration == null) return Array.Empty<KeyValuePair<Type, DataPointerNode>>();

		var configuration = type.Configuration;
		var descriptor_count = type.Supertypes.Any() ? supertypes.Count : supertypes.Count + 1;
		var descriptors = new KeyValuePair<Type, DataPointerNode>[descriptor_count];

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
			descriptors[^1] = new KeyValuePair<Type, DataPointerNode>(type, new DataPointerNode(configuration.Entry));
		}

		var offset = Parser.Bytes;

		// Look for default implementations of virtual functions in the specified type
		foreach (var virtual_function in type.Virtuals.Values.SelectMany(i => i.Overloads).Cast<VirtualFunction>())
		{
			// Register an implementation for the current virtual function.
			offset = TryRegisterVirtualFunctionImplementation(type, virtual_function, configuration, offset);
		}

		for (var i = 0; i < supertypes.Count; i++)
		{
			var supertype = supertypes[i];

			// Append configuration information only if it is not generated
			if (!configuration.IsCompleted)
			{
				// Begin a new section inside the configuration table
				configuration.Entry.Add(configuration.Descriptor);
			}

			// Types should not inherit types which do not have runtime configurations such as standard integers
			if (supertype.Configuration == null) throw new ApplicationException("Type inherited a type which did not have runtime configuration");

			descriptors[i] = new KeyValuePair<Type, DataPointerNode>(supertype, new DataPointerNode(configuration.Entry, offset));
			offset += Parser.Bytes;

			// Iterate all virtual functions of this supertype and connect their implementations
			var perspective = i == 0 ? type : supertype;
			
			foreach (var virtual_function in perspective.GetAllVirtualFunctions())
			{
				offset = TryRegisterVirtualFunctionImplementation(type, virtual_function, configuration, offset);
			}
		}

		configuration.IsCompleted = true;
		return descriptors;
	}

	/// <summary>
	/// Constructs an object using heap memory
	/// </summary>
	public static InlineContainer CreateHeapConstruction(Type type, ConstructionNode construction, FunctionNode constructor)
	{
		var container = CreateInlineContainer(type, construction, true);
		var position = construction.Position;

		var size = Math.Max(1, type.ContentSize);
		var allocator = ReconstructionAnalysis.GetAllocator(type, construction, construction.Position, size);

		// Cast the allocation to the construction type if needed
		if (allocator.GetType() != type)
		{
			var casted = new CastNode(allocator, new TypeNode(type, position), position);
			allocator = casted;
		}

		// The following example creates an instance of a type called Object
		// Example: instance = allocate(sizeof(Object)) as Object
		container.Node.Add(new OperatorNode(Operators.ASSIGN, position).SetOperands(new VariableNode(container.Result, position), allocator));

		var supertypes = type.GetAllSupertypes();

		// Remove supertypes, which cause a configuration variable duplication
		for (var i = 0; i < supertypes.Count; i++)
		{
			var current = supertypes[i].GetConfigurationVariable();

			for (var j = supertypes.Count - 1; j >= i + 1; j--)
			{
				if (current != supertypes[j].GetConfigurationVariable()) continue;
				supertypes.RemoveAt(j);
			}
		}

		var descriptors = CopyTypeDescriptors(type, supertypes);

		// Register the runtime configurations
		foreach (var iterator in descriptors)
		{
			container.Node.Add(new OperatorNode(Operators.ASSIGN, position).SetOperands(
				new LinkNode(new VariableNode(container.Result, position), new VariableNode(iterator.Key.GetConfigurationVariable(), position)),
				iterator.Value
			));
		}

		// Do not call the initializer function if it is empty
		if (!constructor.Function.IsEmpty)
		{
			container.Node.Add(new LinkNode(new VariableNode(container.Result, position), constructor, position));
		}
		
		// The inline node must return the value of the constructed object
		container.Node.Add(new VariableNode(container.Result, position));

		return container;
	}

	/// <summary>
	/// Constructs an object using stack memory
	/// </summary>
	public static InlineContainer CreateStackConstruction(Type type, Node construction, FunctionNode constructor)
	{
		var container = CreateInlineContainer(type, construction, true);
		var position = construction.Position;

		container.Node.Add(new OperatorNode(Operators.ASSIGN, position).SetOperands(
			new VariableNode(container.Result, position),
			new CastNode(new StackAddressNode(construction.GetParentContext(), type, position), new TypeNode(type, position))
		));

		var supertypes = type.GetAllSupertypes();
		
		// Remove supertypes, which cause a configuration variable duplication
		for (var i = 0; i < supertypes.Count; i++)
		{
			var current = supertypes[i].GetConfigurationVariable();

			for (var j = supertypes.Count - 1; j >= i + 1; j--)
			{
				if (current != supertypes[j].GetConfigurationVariable()) continue;
				supertypes.RemoveAt(j);
			}
		}

		var descriptors = CopyTypeDescriptors(type, supertypes);

		// Register the runtime configurations
		foreach (var iterator in descriptors)
		{
			container.Node.Add(new OperatorNode(Operators.ASSIGN, position).SetOperands(
				new LinkNode(new VariableNode(container.Result, position), new VariableNode(iterator.Key.GetConfigurationVariable(), position)),
				iterator.Value
			));
		}

		// Do not call the initializer function if it is empty
		if (!constructor.Function.IsEmpty)
		{
			container.Node.Add(new LinkNode(new VariableNode(container.Result, position), constructor, position));
		}

		// The inline node must return the value of the constructed object
		container.Node.Add(new VariableNode(container.Result, position));

		return container;
	}

	/// <summary>
	/// Determines the variable which will store the result and the node that should contain the inlined content
	/// </summary>
	public static InlineContainer CreateInlineContainer(Type type, Node node, bool is_value_returned)
	{
		var editor = Analyzer.TryGetEditor(node);

		if (editor != null && editor.Is(Operators.ASSIGN))
		{
			var edited = Analyzer.GetEdited(editor);

			if (edited.Is(NodeType.VARIABLE) && edited.To<VariableNode>().Variable.IsPredictable)
			{
				return new InlineContainer(editor, new InlineNode(node.Position), edited.To<VariableNode>().Variable);
			}
		}

		var environment = node.GetParentContext();
		var container = new ScopeNode(new Context(environment), node.Position, null, is_value_returned);
		var instance = container.Context.DeclareHidden(type);
		
		return new InlineContainer(node, container, instance);
	}

	/// <summary>
	/// Returns the root of the expression which contains the specified node
	/// </summary>
	public static Node GetExpressionRoot(Node node)
	{
		var iterator = node;

		while (true)
		{
			var next = iterator.Parent;
			if (next == null) break;

			if (next.Is(NodeType.OPERATOR) && !next.Is(OperatorType.ASSIGNMENT)) { iterator = next; }
			else if (next.Is(NodeType.PARENTHESIS, NodeType.LINK, NodeType.NEGATE, NodeType.NOT, NodeType.ACCESSOR, NodeType.PACK)) { iterator = next; }
			else { break; }
		}

		return iterator;
	}

	/// <summary>
	/// Returns whether the specified node is part of a function call argument
	/// </summary>
	public static bool IsCallArgument(Node node)
	{
		var result = node.FindParent(i => !i.Is(NodeType.PARENTHESIS, NodeType.CAST));

		return result != null && result.Is(NodeType.CALL, NodeType.FUNCTION, NodeType.UNRESOLVED_FUNCTION);
	}

	/// <summary>
	/// Returns the self pointer of the specified context
	/// </summary>
	public static Node GetSelfPointer(Context context, Position? position)
	{
		var self = context.GetSelfPointer();
		if (self != null) return new VariableNode(self, position);

		return new UnresolvedIdentifier(context.IsInsideLambda ? Lambda.SELF_POINTER_IDENTIFIER : Function.SELF_POINTER_IDENTIFIER, position);
	}

	/// <summary>
	/// Finds the condition node from the specified node
	/// </summary>
	public static Node FindCondition(Node start)
	{
		return start.GetRightWhile(i => i.Is(NodeType.SCOPE, NodeType.INLINE, NodeType.NORMAL, NodeType.PARENTHESIS)) ?? throw new ApplicationException("Conditional statement did not have a condition");
	}

	/// <summary>
	/// Collects all types and subtypes from the specified context
	/// </summary>
	public static List<Type> GetAllTypes(Context context, bool include_imported = true)
	{
		var result = context.Types.Values.ToList();
		result.AddRange(result.SelectMany(i => GetAllTypes(i)).ToList());

		return result
			.Where(i => !string.IsNullOrEmpty(i.Name))
			.Where(i => include_imported || !i.IsImported)
			.ToList();
	}

	/// <summary>
	/// Collects all variables from the specified context and its subcontexts
	/// </summary>
	public static List<Variable> GetAllVariables(Context context)
	{
		return context.Variables.Values
			.Concat(context.Subcontexts.SelectMany(i => GetAllVariables(i)))
			.Distinct()
			.ToList();
	}

	/// <summary>
	/// Collects all variables from the specified context and its subcontexts, but not from functions
	/// </summary>
	public static List<Variable> GetAllVariablesOutsideFunctions(Context context)
	{
		var subcontexts = context.Subcontexts.Where(i => !i.IsFunction && !i.IsImplementation);

		return context.Variables.Values
			.Concat(subcontexts.SelectMany(i => GetAllVariablesOutsideFunctions(i)))
			.Distinct()
			.ToList();
	}

	/// <summary>
	/// Collects all local function implementations from the specified context
	/// </summary>
	public static FunctionImplementation[] GetLocalFunctionImplementations(Context context)
	{
		if (context.IsType)
		{
			var type = context.To<Type>();

			return type.Constructors.Overloads
				.Concat(type.Destructors.Overloads)
				.Concat(type.Functions.Values.SelectMany(i => i.Overloads))
				.Concat(type.Overrides.Values.SelectMany(i => i.Overloads))
				.SelectMany(i => i.Implementations)
				.Distinct(new ReferenceEqualityComparer<FunctionImplementation>()).ToArray();
		}

		return context.Functions.Values.SelectMany(i => i.Overloads).SelectMany(i => i.Implementations).ToArray();
	}

	/// <summary>
	/// Collects all function implementations from the specified context
	/// </summary>
	public static FunctionImplementation[] GetAllFunctionImplementations(Context context, bool include_imported = true)
	{
		var types = Common.GetAllTypes(context);

		// Collect all functions, constructors, destructors and virtual functions
		var type_functions = types.SelectMany(i => i.Functions.Values.SelectMany(j => j.Overloads));
		var type_constructors = types.SelectMany(i => i.Constructors.Overloads);
		var type_destructors = types.SelectMany(i => i.Destructors.Overloads);
		var type_overrides = types.SelectMany(i => i.Overrides.Values.SelectMany(j => j.Overloads));
		var context_functions = context.Functions.Values.SelectMany(i => i.Overloads);

		var implementations = type_functions
			.Concat(type_constructors)
			.Concat(type_destructors)
			.Concat(type_overrides)
			.Concat(context_functions)
			.SelectMany(i => i.Implementations).ToArray();

		// Combine all functions with lambdas, which can be found inside the collected functions
		return implementations
			.Concat(implementations.SelectMany(i => GetAllFunctionImplementations(i)))
			.Distinct(new ReferenceEqualityComparer<FunctionImplementation>())
			.Where(i => include_imported || !i.IsImported)
			.ToArray();
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
		var type_virtuals = types.SelectMany(i => i.Virtuals.Values.SelectMany(j => j.Overloads));
		var type_overrides = types.SelectMany(i => i.Overrides.Values.SelectMany(j => j.Overloads));
		var context_functions = context.Functions.Values.SelectMany(i => i.Overloads);

		return type_functions
			.Concat(type_constructors)
			.Concat(type_destructors)
			.Concat(type_virtuals)
			.Concat(type_overrides)
			.Concat(context_functions)
			.Distinct().ToArray();
	}

	/// <summary>
	/// Collects all functions which have implementations from the specified context and its subcontexts
	/// </summary>
	public static Function[] GetAllImplementedFunctions(Context context, bool include_imported = true)
	{
		return GetAllFunctionImplementations(context, include_imported).Select(i => i.Metadata!).Distinct().ToArray();
	}

	/// <summary>
	/// Returns whether the specified integer fulfills the following equation:
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
		
		if (result.Count == 0) return result.ToArray();

		if (result.Count == 0) return result.ToArray();

		return result.GetRange(0, result.Count - 1).ToArray();
	}

	/// <summary>
	/// Returns tokens which represent the specified type
	/// </summary>
	public static Token[] GetTokens(Type type, Position position)
	{
		var result = new List<Token>();

		if (type.IsUnnamedPack)
		{
			// Construct the following pattern from the members of the pack: [ $member-1: $type-1 ], [ $member-2: $type-2 ], ...
			var members = type.Variables.Values.Select(i =>
			{
				var member_name = new IdentifierToken(i.Name, position);
				var member_colon = new OperatorToken(Operators.COLON, position);
				var member_type = GetTokens(i.Type!, position);

				return new Token[] { member_name, member_colon }.Concat(member_type).ToArray();

			}).ToArray();

			// Now, join the token arrays with commas and put them inside curly brackets: { $member-1: $type-1, $member-2: $type-2, ... }
			result.Add(new ParenthesisToken(ParenthesisType.CURLY_BRACKETS, Join(new OperatorToken(Operators.COMMA, position), members), position));

			return result.ToArray();
		}

		if (type is FunctionType function)
		{
			var parameters = function.Parameters.Select(i => GetTokens(i!, position)).ToArray();
			var separator = new OperatorToken(Operators.COMMA) { Position = position };

			result.Add(new ParenthesisToken(ParenthesisType.PARENTHESIS, Join(separator, parameters)) { Position = position });
			result.Add(new OperatorToken(Operators.ARROW) { Position = position });
			result.AddRange(GetTokens(function.ReturnType!, position));

			return result.ToArray();
		}

		if (type is ArrayType array)
		{
			result.AddRange(GetTokens(array.Element, position));
			result.Add(new ParenthesisToken(ParenthesisType.BRACKETS, new NumberToken(array.Size)));
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
	/// Returns whether the specified node represents a local variable
	/// </summary>
	public static bool IsLocalVariable(Node node)
	{
		return node.Instance == NodeType.VARIABLE && node.To<VariableNode>().Variable.IsPredictable;
	}

	/// <summary>
	/// Returns how many bits the specified number requires
	/// </summary>
	public static int GetBits(object value)
	{
		if (value is double) return Parser.Bits;

		var x = (long)value;

		if (x < 0)
		{
			if (x < int.MinValue) return 64;
			else if (x < short.MinValue) return 32;
			else if (x < sbyte.MinValue) return 16;
		}
		else
		{
			if (x > int.MaxValue) return 64;
			else if (x > short.MaxValue) return 32;
			else if (x > sbyte.MaxValue) return 16;
		}

		return 8;
	}

	/// <summary>
	/// Returns the position which represents the end of the specified token
	/// </summary>
	public static Position? GetEndOfToken(Token token)
	{
		return token.Type switch
		{
			TokenType.PARENTHESIS => token.To<ParenthesisToken>().End ?? token.Position.Translate(1),
			TokenType.FUNCTION => token.To<FunctionToken>().Identifier.End,
			TokenType.IDENTIFIER => token.To<IdentifierToken>().End,
			TokenType.KEYWORD => token.To<KeywordToken>().End,
			TokenType.NUMBER => token.To<NumberToken>().End ?? token.Position.Translate(1),
			TokenType.OPERATOR => token.To<OperatorToken>().End,
			TokenType.STRING => token.To<StringToken>().End,
			TokenType.END => token.Position.Clone().NextLine(),
			_ => null
		};
	}

	public static string ToString(this long value, bool sign)
	{
		var result = value.ToString();

		if (value < 0) { result = '-' + result[1..]; }
		else if (sign) { result = '+' + result; }

		return result;
	}

	public static string ToString(this int value, bool sign)
	{
		var result = value.ToString();

		if (value < 0) { result = '-' + result[1..]; }
		else if (sign) { result = '+' + result; }

		return result;
	}

	public static string ToString(this double value, bool sign)
	{
		var result = value.ToString();

		if (value < 0) { result = '-' + result[1..]; }
		else if (sign) { result = '+' + result; }

		// Use dots as decimal separators
		result = result.Replace(',', '.');

		if (!result.Contains('.')) return result + ".0";
		return result;
	}

	/// <summary>
	/// Returns all local variables, which represent the specified pack variable
	/// </summary>
	private static List<Variable> GetPackProxies(Context context, string prefix, Type type, VariableCategory category)
	{
		var proxies = new List<Variable>();

		foreach (var member in type.Variables.Values)
		{
			// Do not process static or constant member variables
			if (member.IsStatic || member.IsConstant) continue;

			var name = prefix + '.' + member.Name;

			// Create proxies for each member, even for nested pack members
			var proxy = context.GetVariable(name);
			if (proxy == null) { proxy = context.Declare(member.Type!, category, name); }

			if (member.Type!.IsPack)
			{
				proxies.AddRange(GetPackProxies(context, name, member.Type!, category));
			}
			else
			{
				proxies.Add(proxy);
			}
		}

		return proxies;
	}

	/// <summary>
	/// Returns all local variables, which represent the specified pack variable
	/// </summary>
	public static List<Variable> GetPackProxies(Variable pack)
	{
		// If we are accessing a pack proxy, no need to add dot to the name
		var prefix = pack.Name.StartsWith('.') ? pack.Name : ('.' + pack.Name);

		return GetPackProxies(pack.Parent, prefix, pack.Type!, pack.Category);
	}

	/// <summary>
	/// Returns all non-static members from the specified type
	/// </summary>
	public static List<Variable> GetNonStaticMembers(Type type)
	{
		var result = new List<Variable>();

		foreach (var iterator in type.Variables)
		{
			// Skip static and constant member variables
			var member = iterator.Value;
			if (member.IsStatic || member.IsConstant) continue;

			result.Add(member);
		}

		return result;
	}

	/// <summary>
	/// Returns true if the specified node represents integer zero
	/// </summary>
	public static bool IsZero(Node? node)
	{
		return node != null && node.Is(NodeType.NUMBER) && Numbers.IsZero(node.To<NumberNode>().Value);
	}

	/// <summary>
	/// Reports the specified error to the user
	/// </summary>
	public static void Report(Status error)
	{
		Console.WriteLine(Errors.Format(error));
	}

	/// <summary>
	/// Reports the specified errors to the user
	/// </summary>
	public static void Report(List<Status> errors)
	{
		foreach (var error in errors) { Report(error); }
	}
}
