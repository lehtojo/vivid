﻿using System.Collections.Generic;
using System.Linq;

public class VirtualFunctionPattern : Pattern
{
	public const int PRIORITY = 1;

	public const int RETURN_TYPE_INDICATOR = 1;
	public const int RETURN_TYPE = 2;

	// NOTE: This pattern should execute after all member functions are declared, therefore the priority is low
	// Pattern: $function
	public VirtualFunctionPattern() : base
	(
		TokenType.FUNCTION, TokenType.OPERATOR | TokenType.OPTIONAL
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		if (!context.IsInsideType || context.IsInsideFunction)
		{
			return false;
		}

		var indicator = tokens[RETURN_TYPE_INDICATOR];

		return indicator.Is(TokenType.NONE) || indicator.Is(Operators.COLON) && Common.ConsumeType(state);
	}

	public override Node? Build(Context context, PatternState state, List<Token> tokens)
	{
		var return_type = Primitives.CreateUnit();
		var indicator = tokens[RETURN_TYPE_INDICATOR];

		if (!indicator.Is(TokenType.NONE))
		{
			return_type = Common.ReadType(context, new Queue<Token>(tokens.Skip(RETURN_TYPE)));

			if (return_type == null)
			{
				throw Errors.Get(indicator.Position, "Can not resolve return type of the virtual function");
			}
		}

		var start = tokens.First().Position;

		var type = context.GetTypeParent() ?? throw Errors.Get(start, "Missing uncompleted function type parent");
		var descriptor = tokens.First().To<FunctionToken>();

		if (type.IsVirtualFunctionDeclared(descriptor.Name))
		{
			throw Errors.Get(start, "Virtual function with same name is already declared in one of the inherited types");
		}

		var function = new VirtualFunction(type, descriptor.Name, return_type, start, null);

		var parameters = descriptor.GetParameters(function);

		if (parameters.Any(i => i.Type == null))
		{
			throw Errors.Get(start, "All parameters of a uncompleted function must have a type");
		}

		function.Parameters.AddRange(parameters);

		type.Declare(function);

		return new FunctionDefinitionNode(function, start);
	}
}