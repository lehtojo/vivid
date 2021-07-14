using System.Collections.Generic;
using System.Linq;

public class VirtualFunctionPattern : Pattern
{
	public const int PRIORITY = 22;

	public const int VIRTUAL = 0;
	public const int FUNCTION = 1;
	public const int COLON = 2;
	public const int RETURN_TYPE = 3;

	// Pattern: virtual $function [: $type] [\n] [{...}]
	public VirtualFunctionPattern() : base
	(
		TokenType.KEYWORD, TokenType.FUNCTION, TokenType.OPERATOR | TokenType.OPTIONAL
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		if (!tokens[VIRTUAL].Is(Keywords.VIRTUAL) || !context.IsType) return false;

		var colon = tokens[COLON];

		// If the colon token is not none, it must represent colon operator and the return type must be consumed successfully
		if (!colon.Is(TokenType.NONE) && (!colon.Is(Operators.COLON) || !Common.ConsumeType(state))) return false;

		Consume(state, TokenType.END);

		// Try to consume a function body, which would be the default implementation of the virtual function
		var next = Pattern.Peek(state);
		if (next == null) return true;

		if (next.Is(ParenthesisType.CURLY_BRACKETS) || next.Is(Operators.HEAVY_ARROW))
		{
			Pattern.Consume(state);
		}

		return true;
	}

	/// <summary>
	/// Creates a virtual function which does not have a default implementation
	/// </summary>
	private static Function CreateVirtualFunctionWithoutImplementation(Context context, List<Token> tokens)
	{
		// The default return type is unit, if the return type is not defined
		var return_type = Primitives.CreateUnit();
		var colon = tokens[COLON];

		if (!colon.Is(TokenType.NONE))
		{
			return_type = Common.ReadType(context, new Queue<Token>(tokens.Skip(RETURN_TYPE)));
			if (return_type == null) throw Errors.Get(colon.Position, "Can not resolve return type of the virtual function");
		}

		var descriptor = tokens[FUNCTION].To<FunctionToken>();
		var start = tokens.First().Position;

		// Ensure there is no other virtual function with the same name as this virtual function
		var type = context.FindTypeParent() ?? throw Errors.Get(start, "Missing virtual function type parent");
		if (type.IsVirtualFunctionDeclared(descriptor.Name)) throw Errors.Get(start, "Virtual function with same name is already declared in one of the inherited types");

		var function = new VirtualFunction(type, descriptor.Name, return_type, start, null);

		var parameters = descriptor.GetParameters(function);
		if (parameters.Any(i => i.Type == null)) throw Errors.Get(start, "All parameters of a virtual function must have a type");

		function.Parameters.AddRange(parameters);

		type.Declare(function);
		return function;
	}

	/// <summary>
	/// Creates a virtual function which does have a default implementation
	/// </summary>
	private static Function CreateVirtualFunctionWithImplementation(Context context, PatternState state, List<Token> tokens)
	{
		// Try to resolve the return type
		var return_type = (Type?)null;
		var colon = tokens[COLON];

		if (!colon.Is(TokenType.NONE))
		{
			return_type = Common.ReadType(context, new Queue<Token>(tokens.Skip(RETURN_TYPE)));
			if (return_type == null) throw Errors.Get(colon.Position, "Can not resolve return type of the virtual function");
		}

		// Get the default implementation of this virtual function
		var blueprint = (List<Token>?)null;
		var end = (Position?)null;

		if (tokens.Last().Is(Operators.HEAVY_ARROW))
		{
			var position = tokens.Last().Position;
			blueprint = Common.ConsumeBlock(state);
			blueprint.Insert(0, new OperatorToken(Operators.HEAVY_ARROW, position));
			if (blueprint.Any()) { end = Common.GetEndOfToken(blueprint.Last()); }
		}
		else
		{
			blueprint = tokens.Last().To<ContentToken>().Tokens;
			end = tokens.Last().To<ContentToken>().End;
		}

		var descriptor = tokens[FUNCTION].To<FunctionToken>();
		var start = tokens.First().Position;

		// Ensure there is no other virtual function with the same name as this virtual function
		var type = context.FindTypeParent() ?? throw Errors.Get(start, "Missing virtual function type parent");
		if (type.IsVirtualFunctionDeclared(descriptor.Name)) throw Errors.Get(start, "Virtual function with same name is already declared in one of the inherited types");

		// Create the virtual function declaration
		var virtual_function = new VirtualFunction(type, descriptor.Name, return_type, start, null);

		// Define the virtual function parameters
		var parameters = descriptor.GetParameters(virtual_function);
		if (parameters.Any(i => i.Type == null)) throw Errors.Get(start, "All parameters of a virtual function must have a type");

		virtual_function.Parameters.AddRange(parameters);

		// Create the default implementation of the virtual function
		var function = new Function(context, Modifier.DEFAULT, descriptor.Name, blueprint, descriptor.Position, end);

		// Define the parameters of the default implementation
		function.Parameters.AddRange(descriptor.GetParameters(function));
		
		// Declare both the virtual function and its default implementation
		type.Declare(virtual_function);
		context.To<Type>().DeclareOverride(function);

		return virtual_function;
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		var function = (Function?)null;

		if (tokens.Last().Is(ParenthesisType.CURLY_BRACKETS) || tokens.Last().Is(Operators.HEAVY_ARROW))
		{
			function = CreateVirtualFunctionWithImplementation(context, state, tokens);
		}
		else
		{
			function = CreateVirtualFunctionWithoutImplementation(context, tokens);
		}

		return new FunctionDefinitionNode(function, tokens.First().Position);
	}
}